using System;
using System.Collections.Generic;

using WhiteSphereEngine.physics.rigidBody;
using WhiteSphereEngine.serialization;
using WhiteSphereEngine.math;
using WhiteSphereEngine.physics.solid;
using WhiteSphereEngine.game;
using WhiteSphereEngine.game.responsibilities;
using WhiteSphereEngine.geometry;
using WhiteSphereEngine.subsystemCommon;
using WhiteSphereEngine.entity;
using WhiteSphereEngine.game.entityComponents;
using WhiteSphereEngine.subsystems.renderer.prototypeFormRenderer;

namespace WhiteSphereEngine.builder {
    // builds an game object into the gameworld from a gameObjectTemplate
    public class GameObjectBuilder {
        public SoftwareRenderer softwareRenderer;
        public PhysicsEngine physicsEngine;
        public SolidResponsibility solidResponsibility;
        public EffectResponsibility effectResponsibility;
        public ThrusterResponsibility thrusterResponsibility;
        public AttitudeAndAccelerationControlResponsibility attitudeAndAccelerationControlResponsibility;

        // doesn't add the entity to the entity manager
        public Entity buildFromTemplate(GameObjectTemplate template, SpatialVectorDouble globalPosition, SpatialVectorDouble globalVelocity) {
            IList<IComponent> entityComponents = new List<IComponent>();
            


            PhysicsComponent physicsComponent = buildPhysicsComponentOfMainbody(template, globalPosition, globalVelocity);
            entityComponents.Add(physicsComponent);
            IList<ColliderComponent> colliderComponents = buildColliderComponents(template);

            // add physicsComponent with colliders to physics world
            physicsEngine.physicsAndMeshPairs.Add(new PhysicsComponentAndCollidersPair(physicsComponent, colliderComponents));

            // add solids
            SolidCluster solidCluster = buildSolidCluster(template);
            if ( solidCluster.solids.Count > 0 ) {
                solidResponsibility.mapping.physicsObjectIdToSolidCluster[physicsComponent.id] = solidCluster;
            }

            // add effects
            IList<game.responsibilities.Effect> effects = buildEffects(template);
            if( effects.Count > 0 ) {
                effectResponsibility.physicsObjectIdToEffects[physicsComponent.id] = effects;
            }

            // add special attributes
            buildSpecialAttributes(template, entityComponents);

            // add thrusters and recalculate thruster angular rotation cache
            buildThrustersAndAddToObjectAndRecalcThrusterRelatedCache(template, physicsComponent);

            // add rendering mesh
            buildAndAddRenderingMesh(template, physicsComponent);



            // misc

            // PID controller configuration
            readPidControllerConfiguration(template, physicsComponent.id);



            return Entity.make(entityComponents);
        }

        private void readPidControllerConfiguration(GameObjectTemplate template, ulong physicsObjectId) {
            if( template.pidControllerConfiguration != null ) {
                attitudeAndAccelerationControlResponsibility.PidControlConfigurationByPhysicsObjectId[physicsObjectId] = template.pidControllerConfiguration;
            }
        }

        private void buildSpecialAttributes(GameObjectTemplate template, IList<IComponent> entityComponents) {
            if( template.specialAttributes == null ) {
                return;
            }

            foreach( SpecialAttribute iSpecialAttribute in template.specialAttributes ) {
                if (iSpecialAttribute.type == "withVehicleControllerComponent") { // if the entity/object is with an EntityController
                    entityComponents.Add(new VehicleControllerComponent(attitudeAndAccelerationControlResponsibility));
                }
                else {
                    throw new Exception("Invalid specialAttribute " + iSpecialAttribute);
                }
            }
        }

        PhysicsComponent buildPhysicsComponentOfMainbody(GameObjectTemplate template, SpatialVectorDouble globalPosition, SpatialVectorDouble globalVelocity) {
            PhysicsComponent physicsComponent;
            {
                double mass = template.mainMass;

                Matrix inertiaTensor;
                if ( template.mainMassShapeType == "box" ) {
                    inertiaTensor = InertiaHelper.calcInertiaTensorForCube(mass, template.mainMassDimensions[0], template.mainMassDimensions[1], template.mainMassDimensions[2]);
                }
                else {
                    throw new Exception("Invalid mainMassShapeType " + template.mainMassShapeType);
                }
                physicsComponent = physicsEngine.createPhysicsComponent(globalPosition, globalVelocity, mass, inertiaTensor);
            }
            return physicsComponent;
        }

        static IList<ColliderComponent> buildColliderComponents(GameObjectTemplate template) {
            IList<ColliderComponent> colliderComponents = new List<ColliderComponent>();

            foreach( Collider iCollider in template.colliders ) {
                SpatialVectorDouble colliderComponentSize = new SpatialVectorDouble(iCollider.size);
                SpatialVectorDouble colliderComponentLocalPosition = new SpatialVectorDouble(iCollider.localPosition);
                SpatialVectorDouble colliderComponentLocalRotation = new SpatialVectorDouble(iCollider.localRotation);

                ColliderComponent colliderComponent;
                if (template.mainMassShapeType == "box") {
                    colliderComponent = ColliderComponent.makeBox(colliderComponentSize, colliderComponentLocalPosition, colliderComponentLocalRotation);
                }
                else {
                    throw new Exception("Invalid collider shapeType " + iCollider.shapeType);
                }
                colliderComponents.Add(colliderComponent);
            }

            return colliderComponents;
        }

        SolidCluster buildSolidCluster(GameObjectTemplate template) {
            SolidCluster solidCluster = new SolidCluster();

            if( template.solids == null ) {
                return solidCluster;
            }

            foreach(serialization.Solid iSolid in template.solids ) {
                SpatialVectorDouble solidSize = new SpatialVectorDouble(iSolid.size);
                double massOfSolidInKilogram = iSolid.fractionMass;
                IList<CompositionFraction> solidCompositionFractions = new List<CompositionFraction>() { new CompositionFraction(new Isotope(iSolid.fractionIsotopeName), massOfSolidInKilogram) };
                Composition solidComposition = new Composition(solidCompositionFractions);

                physics.solid.Solid solid = physics.solid.Solid.makeBox(solidComposition, solidSize);

                SpatialVectorDouble solidLocalPosition = new SpatialVectorDouble(iSolid.localPosition);
                SpatialVectorDouble solidLocalRotation = new SpatialVectorDouble(iSolid.localRotation);

                solidCluster.solids.Add(new SolidCluster.SolidWithPositionAndRotation(solid, solidLocalPosition, solidLocalRotation));
            }

            return solidCluster;
        }
        
        private IList<game.responsibilities.Effect> buildEffects(GameObjectTemplate template) {
            IList<game.responsibilities.Effect> resultEffects = new List<game.responsibilities.Effect>();
            
            if( template.effects == null ) {
                return resultEffects;
            }

            foreach (serialization.Effect iEffect in template.effects ) {
                if( iEffect.effectType == "explosion" ) {
                    resultEffects.Add(game.responsibilities.Effect.makeExplosion(new SpatialVectorDouble(iEffect.localPosition)));
                }
                else {
                    throw new Exception("Invalid effectType " + iEffect.effectType);
                }
            }

            return resultEffects;
        }

        private void buildThrustersAndAddToObjectAndRecalcThrusterRelatedCache(GameObjectTemplate template, PhysicsComponent @object) {
            buildThrustersAndAddToObject(template, @object);
            recalcThrusterCache(@object);
        }

        // recalculates the cached variables of the thrusters, like angular rotation acceleration for each thruster on full throttle
        private void recalcThrusterCache(PhysicsComponent @object) {
            thrusterResponsibility.recalcAngularAccelerationOfThrustersForObjectId(physicsEngine, @object.id);
        }

        private void buildThrustersAndAddToObject(GameObjectTemplate template, PhysicsComponent @object) {
            if (template.thrusters == null) {
                return;
            }

            thrusterResponsibility.physicsObjectIdToThrusters[@object.id] = new List<ThrusterResponsibility.ThrusterBinding>();

            foreach ( serialization.Thruster iThruster in template.thrusters ) {
                SpatialVectorDouble objectLocalPosition = new SpatialVectorDouble(iThruster.locationPosition);
                SpatialVectorDouble objectLocalDirection = new SpatialVectorDouble(iThruster.direction);
                double maximalForce = iThruster.maximalForce;

                AttachedForce attachedForce = new AttachedForce(objectLocalPosition, objectLocalDirection);
                @object.attachedForces.Add(attachedForce);

                ThrusterResponsibility.ThrusterBinding thrusterBinding = new ThrusterResponsibility.ThrusterBinding();
                thrusterBinding.attachedForce = attachedForce;
                thrusterBinding.maximalForce = maximalForce;
                thrusterBinding.tag = iThruster.tag;

                thrusterResponsibility.physicsObjectIdToThrusters[@object.id].Add(thrusterBinding);
            }
        }

        void buildAndAddRenderingMesh(GameObjectTemplate template, PhysicsComponent physicsComponent) {
            if( template.meshPath == "meta:fromMainShape") { // the mesh is derived from the mainshape
                MeshWithExplicitFaces renderMesh;

                if( template.mainMassShapeType == "box" ) {
                    renderMesh = PlatonicFactory.createBox(template.mainMassDimensions[0], template.mainMassDimensions[1], template.mainMassDimensions[2]);
                }
                else {
                    throw new NotImplementedException("meta:fromMainShape mesh path is not implemented for shape " + template.meshPath + "!");
                }

                MeshComponent meshComponent = new MeshComponent();
                meshComponent.mesh = renderMesh;

                TransformedMeshComponent transformedMeshComponentForRendering = new TransformedMeshComponent();
                transformedMeshComponentForRendering.meshComponent = meshComponent;

                softwareRenderer.physicsAndMeshPairs.Add(new PhysicsComponentAndMeshPair(physicsComponent, transformedMeshComponentForRendering));
            }
            else {
                throw new NotImplementedException();
            }
        }
    }
}
