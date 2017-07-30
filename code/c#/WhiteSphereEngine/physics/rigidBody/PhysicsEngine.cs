using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using WhiteSphereEngine.math;
using WhiteSphereEngine.geometry;
using WhiteSphereEngine.physics.intersection;
using WhiteSphereEngine.physics.boundingVolume;
using WhiteSphereEngine.celestial;
using WhiteSphereEngine.celestial.mathematics;

// TODO< solar system abstraction with it's own celestial bodies (which emit gravity), objects and particles >

namespace WhiteSphereEngine.physics.rigidBody {
    public class PhysicsEngine {

        private class Acceleration : RungeKutta4.IAcceleration {
            public Acceleration(PhysicsEngine physicsEngine) {
                this.physicsEngine = physicsEngine;
            }

            public SpatialVectorDouble calculateAcceleration(ref RungeKutta4State state, float time) {
                return accelerationForCurrentBody;
            }

            public SpatialVectorDouble accelerationForCurrentBody;

            PhysicsEngine physicsEngine;
        }

        public PhysicsEngine() {
            rungeKutta4.acceleration = new Acceleration(this);
        }

        CollisionCallbackHelper collisionCallbacks = new CollisionCallbackHelper();
        public IList<ICollisionHandler> collisionHandlers {
            get {
                return collisionCallbacks.collisionHandlers;
            }
            set {
                collisionCallbacks.collisionHandlers = value;
            }
        }
        
        public IList<CelestialObjectWithPosition> celestialObjects {
            get {
                return celestialPhysics.celestialObjects;
            }
        }

        public IList<PhysicsComponentAndCollidersPair> physicsAndMeshPairs = new List<PhysicsComponentAndCollidersPair>();

        // particles are infinitisimal objects (which can still collide)
        IList<PhysicsComponent>
            particles = new List<PhysicsComponent>(),
            particlesToRemove = new List<PhysicsComponent>();

        public void addParticle(PhysicsComponent particle) {
            particles.Add(particle);
        }

        public void removeParticle(PhysicsComponent particle) {
            // TODO< inform proximity detectors that particle was removed >

            particles.Remove(particle);
        }

        public PhysicsComponent createPhysicsComponent(SpatialVectorDouble position, SpatialVectorDouble velocity, double mass, Matrix? inertiaTensor) {
            return new PhysicsComponent(position, velocity, mass, inertiaTensor, physicsComponentAndParticleIdCounter++);
        }

        public PhysicsComponent getObjectById(ulong id) {
            // OPTIMIZATION TODO< superslow way, speed this thing up if it eats too many cycles >
            foreach ( PhysicsComponent iObject in physicsAndMeshPairs.Select(v => v.physicsComponent)) {
                if( iObject.id == id ) {
                    return iObject;
                }
            }

            // when we are here we hit an fatal bug
            throw new Exception("PhysicsComponent with id=" + id.ToString() + " couldn't be found!");
        }

        public bool existObjectById(ulong id) {
            // OPTIMIZATION TODO< superslow way, speed this thing up if it eats too many cycles >
            foreach (PhysicsComponent iObject in physicsAndMeshPairs.Select(v => v.physicsComponent)) {
                if (iObject.id == id) {
                    return true;
                }
            }

            return false;
        }

        public void tick() {
            resetPhysicsHelpersOfObjects();

            // celestials
            calcAccelerationOfObjectsFromCelestialObjects();

            // objects
            calcForcesForRigidBodies();

            // object particle collision and response
            calcCollisionsOfParticlesAndObjectsAndResponse();

            applyAngularChangeOfAllObjects();

            moveAllObjects();
            rotateAllObjects();

            transformAllObjects();
            recalcBoundingVolumes();

            // particles
            calcForcesForParticles();

            ///applyLinearChangeOfAllParticles();
            moveAllParticles();
            
            removeParticles();



            // proximity detectors
            removeParticlesFromProximityDetectors();
            checkProximityAndFireEvents();

            // cleanup
            resetParticlesToRemove();
        }

        public RayHitDescriptor? traceRaySyncronous(SpatialVectorDouble rayOrigin, SpatialVectorDouble rayDirection, out bool hit, double maxT = double.MaxValue) {
            return traceRayInternal(rayOrigin, rayDirection, out hit, maxT);
        }

        RayHitDescriptor? traceRayInternal(SpatialVectorDouble rayOrigin, SpatialVectorDouble rayDirection, out bool hit, double maxT = double.MaxValue) {
            CollisionDescriptor collisionDescriptor = new CollisionDescriptor();
            
            foreach (PhysicsComponentAndCollidersPair iPhysicsComponentAndColliders in physicsAndMeshPairs) {
                if( ConvexHullRayIntersection.checkRayCollision(rayOrigin, rayDirection, iPhysicsComponentAndColliders.physicsComponent.boundingVolume.convexHull) ) {
                    
                    // iterate over all ColliderComponents and check for collision
                    foreach( ColliderComponent iCollider in iPhysicsComponentAndColliders.colliders ) {
                        if( iCollider.isConvex ) {

                            SpatialVectorDouble[] convexHullPlanes = MeshUtilities.calcAllPlanes(iCollider.faces, iCollider.transformedVertices);

                            SpatialVectorDouble? hitNormal;
                            double hitTResult;
                            int? hitFaceNumber;
                            
                            bool isHit = ConvexHullRayIntersection.calcRayCollision(rayOrigin, rayDirection, convexHullPlanes, out hitNormal, out hitTResult, out hitFaceNumber);
                            if( !isHit ) {
                                continue;
                            }

                            if (hitTResult < 0.0) {
                                continue;
                            }

                            if( hitTResult > maxT ) {
                                continue;
                            }

                            if (collisionDescriptor.rayDistance < hitTResult) {
                                continue;
                            }

                            

                            // store collision information
                            collisionDescriptor.rayDistance = hitTResult;
                            collisionDescriptor.faceIndex = (uint)hitFaceNumber.Value;
                            collisionDescriptor.faceNormal = hitNormal.Value;
                            collisionDescriptor.physicsComponentAndCollider = new PhysicsComponentAndCollidersPair(iPhysicsComponentAndColliders.physicsComponent, iCollider);
                            collisionDescriptor.hit = true;
                        }
                        else {
                            throw new NotImplementedException();
                        }
                    }
                }
            }

            hit = collisionDescriptor.hit;
            if( hit ) {
                // translate hit informations
                RayHitDescriptor hitDescriptor = new RayHitDescriptor();
                hitDescriptor.hitNormal = collisionDescriptor.faceNormal;
                hitDescriptor.hitPhysicsComponentAndCollider = collisionDescriptor.physicsComponentAndCollider;
                hitDescriptor.hitPosition = rayOrigin + rayDirection.scale(collisionDescriptor.rayDistance);
                return hitDescriptor;
            }
            else {
                return null;
            }
        }

        void resetPhysicsHelpersOfObjects() {
            foreach (PhysicsComponentAndCollidersPair iPhysicsComponentAndColliders in physicsAndMeshPairs) {
                iPhysicsComponentAndColliders.physicsComponent.linearAcceleration = new SpatialVectorDouble(new double[] { 0, 0, 0 });
            }
        }

        void calcForcesForRigidBodies() {
            // TODO PERFORMANCE< parallel for >
            foreach (PhysicsComponent iPhysicsComponent in physicsAndMeshPairs.Select(v => v.physicsComponent)) {
                // see https://github.com/PtrMan/SpaceSimCore/blob/master/src/physics/PhysicsEngine.cpp#L87

                iPhysicsComponent.eulerAngularAcceleration = new SpatialVectorDouble(new double[] { 0, 0, 0 });

                foreach ( AttachedForce iterationAttachedForce in iPhysicsComponent.attachedForces) {
                    SpatialVectorDouble localForce = iterationAttachedForce.forceVectorInNewton;

                    applyForceToLinearAndAngularVelocity(iPhysicsComponent, localForce, iterationAttachedForce.objectLocalPosition);
                }
            }
        }

        void calcForcesForParticles() {
            // TODO PERFORMANCE< parallel for >
            foreach (PhysicsComponent iParticle in particles) {
                ///iParticle.linearVelocityDelta = new SpatialVectorDouble(new double[] { 0, 0, 0 });
            }
        }

        static void applyForceToLinearAndAngularVelocity(PhysicsComponent physicsComponent, SpatialVectorDouble localForce, SpatialVectorDouble objectLocalPositionOfForce) {
            { // linear part
              // to calculate the linear component we use the dot product
                double scaleOfLinearForce = 0.0;
                if (localForce.length > double.Epsilon) {
                    double dotOfForceAndLocalPosition = SpatialVectorDouble.dot(localForce.normalized(), objectLocalPositionOfForce.normalized());
                    scaleOfLinearForce = System.Math.Abs(dotOfForceAndLocalPosition);
                }

                // the linear force (and resulting acceleration) is the force scaled by the dot product

                Matrix rotationMatrix = physicsComponent.calcLocalToGlobalRotationMatrix();
                Matrix globalForceAsMatrix = rotationMatrix * SpatialVectorUtilities.toVector4(localForce).asMatrix;
                SpatialVectorDouble globalForce = SpatialVectorUtilities.toVector3(new SpatialVectorDouble(globalForceAsMatrix));

                physicsComponent.linearAcceleration += globalForce.scale(scaleOfLinearForce * physicsComponent.invMass);
            }

            { // angular part
                physicsComponent.eulerAngularAcceleration += physicsComponent.calcAngularAccelerationOfRigidBodyForAppliedForce(objectLocalPositionOfForce, localForce);
            }
        }




        // adds the rotation and velocity changes to the bodies
        /*
        void applyLinearAndAngularChangeOfAllObjects() {
            // TODO PERFORMANCE < parallel (linq) foreach? >
            foreach (PhysicsComponent iPhysicsComponent in physicsAndMeshPairs.Select(v => v.physicsComponent)) {
                // TODO< integration >
                iPhysicsComponent.rungeKutta4State.v += iPhysicsComponent.linearVelocityDelta;
            }
        }

        void applyLinearAndAngularChangeOfAllParticles() {
            // TODO PERFORMANCE < parallel (linq) foreach? >
            foreach (PhysicsComponent iPhysicsComponent in particles) {
                // TODO< integration >
                iPhysicsComponent.rungeKutta4State.v += iPhysicsComponent.linearVelocityDelta;
            }
        }
        */
        void applyAngularChangeOfAllObjects() {
            if(false) { // debugging 
                return;
            }

            // BUG< disable this so the an acceleration bug is visible, FIX THIS >

            // TODO PERFORMANCE < parallel (linq) foreach? >
            foreach (PhysicsComponent iPhysicsComponent in physicsAndMeshPairs.Select(v => v.physicsComponent)) {
                iPhysicsComponent.eulerLocalAngularVelocity += iPhysicsComponent.eulerAngularAcceleration.scale(dt);
            }
        }


        void transformAllObjects() {
            transformObjects(false);
        }

        void transformOnlyDirtyObjects() {
            transformObjects(true);
        }

        // \param onlyForDirty recalc only for dirty objects, dirtly objects are objects where the bounding volume got nulled
        void transformObjects(bool onlyForDirty) {
            // TODO PERFORMANCE< parallel for >
            foreach (PhysicsComponentAndCollidersPair iPhysicsComponentAndColliders in physicsAndMeshPairs) {
                if (onlyForDirty && iPhysicsComponentAndColliders.physicsComponent.boundingVolume != null) {
                    continue;
                }

                foreach ( ColliderComponent iCollider in iPhysicsComponentAndColliders.colliders ) {
                    Matrix localToGlobalRotationAndTranslationMatrix = iPhysicsComponentAndColliders.physicsComponent.calcLocalToGlobalRotationAndTranslationMatrix();
                    iCollider.transformByMatrix(localToGlobalRotationAndTranslationMatrix);
                }
            }
        }
        

        void moveAllObjects() {
            moveAllPhysicsComponents(physicsAndMeshPairs.Select(v => v.physicsComponent));
        }

        void moveAllParticles() {
            moveAllPhysicsComponents(particles);
        }

        void moveAllPhysicsComponents(IEnumerable<PhysicsComponent> physicsComponents) {
            // NOTE PERFORMANCE< can't be parallelized because it is using one RungeKutta acceleration helper object
            //                   accesses and operations of it would overlap in parallel execution >
            foreach (PhysicsComponent iPhysicsComponent in physicsComponents) {
                // update acceleration for rungeKutta4.acceleration
                ((Acceleration)rungeKutta4.acceleration).accelerationForCurrentBody = iPhysicsComponent.linearAcceleration;

                SpatialVectorDouble oldPosition = iPhysicsComponent.position;

                rungeKutta4.integrate(ref iPhysicsComponent.rungeKutta4State, 0, (float)dt);
                iPhysicsComponent.updatePosition(iPhysicsComponent.position, oldPosition);
            }
        }

        void rotateAllObjects() {
            // TODO PERFORMANCE< parallel for >
            foreach (PhysicsComponentAndCollidersPair iPhysicsComponentAndColliders in physicsAndMeshPairs) {
                SpatialVectorDouble eulerDeltaRotation = iPhysicsComponentAndColliders.physicsComponent.eulerLocalAngularVelocity.scale(dt);
                Quaternion quaternionDeltaRotation = QuaternionUtilities.makeFromEulerAngles(eulerDeltaRotation.x, eulerDeltaRotation.y, eulerDeltaRotation.z);

                iPhysicsComponentAndColliders.physicsComponent.rotation = (iPhysicsComponentAndColliders.physicsComponent.rotation * quaternionDeltaRotation).normalized(); // we need to normalize because else our rotation drifts off over time
            }
        }

        void calcAccelerationOfObjectsFromCelestialObjects() {
            celestialPhysics.calcForcesAndAccelerationsForPhysicsComponents(physicsAndMeshPairs.Select(v => v.physicsComponent));
        }

        // \param onlyForDirty recalc only for dirty objects, dirtly objects are objects where the bounding volume got nulled
        void recalcBoundingVolumes(bool onlyForDirty = false) {
            // TODO PERFORMANCE< parallel for >
            foreach (PhysicsComponentAndCollidersPair iPhysicsComponentAndColliders in physicsAndMeshPairs) {
                if( onlyForDirty && iPhysicsComponentAndColliders.physicsComponent.boundingVolume != null ) {
                    continue;
                }

                List<SpatialVectorDouble> verticesOfContainedMeshes = new List<SpatialVectorDouble>();

                foreach( var iCollider in iPhysicsComponentAndColliders.colliders ) {
                    verticesOfContainedMeshes.AddRange(iCollider.transformedVertices);
                }

                iPhysicsComponentAndColliders.physicsComponent.recalcBoundingVolume(verticesOfContainedMeshes);
            }
        }

        void recalcBoundingVolumesForDirtyObjects() {
            recalcBoundingVolumes(true);
        }

        void calcCollisionsOfParticlesAndObjectsAndResponse() {
            // parallel computation is difficult, because objects velocity and angular velocity are manipulated
            foreach( PhysicsComponent iParticle in particles ) {
                bool hit;
                RayHitDescriptor? rayHitDescriptor = traceRayInternal(iParticle.lastPosition, iParticle.position - iParticle.lastPosition, out hit, 1.0 /* maxT */);
                if( !hit ) {
                    continue;
                }

                Debug.Assert(rayHitDescriptor != null);

                CollisionInformation collisionInformation;
                collisionInformation.a = iParticle;
                collisionInformation.aIsParticle = true;
                collisionInformation.b = rayHitDescriptor.Value.hitPhysicsComponentAndCollider.physicsComponent;
                collisionInformation.globalPosition = rayHitDescriptor.Value.hitPosition;
                collisionInformation.globalNormal = rayHitDescriptor.Value.hitNormal;

                EnumParticleCollisionResponse particalCollisionResponse;
                collisionCallbacks.instantaniousParticleContact(ref collisionInformation, out particalCollisionResponse);
                
                PhysicsComponent hitObjectPhysicsComponent = rayHitDescriptor.Value.hitPhysicsComponentAndCollider.physicsComponent;

                if (particalCollisionResponse == EnumParticleCollisionResponse.REFLECT) {
                    // TODO
                }
                else if(particalCollisionResponse == EnumParticleCollisionResponse.ABSORB) {
                    particlesToRemove.Add(iParticle); // remove the particle

                    // impulse depends on the relative velocity between the particle and the object and not just the velocity of the object
                    SpatialVectorDouble globalImpulseFromParticleRelativeToObject = calcImpulse(iParticle.velocity - hitObjectPhysicsComponent.velocity, iParticle.mass); // PHYSICS CORRECTNESS NOTE< we work with the impulse >
                    SpatialVectorDouble globalImpulsePosition = rayHitDescriptor.Value.hitPosition;

                    // transform impulse and position into object local space from global space
                    SpatialVectorDouble localImpulseFromParticle = SpatialVectorUtilities.toVector3(new SpatialVectorDouble(hitObjectPhysicsComponent.calcGlobalToLocalRotationMatrix() * SpatialVectorUtilities.toVector4(globalImpulseFromParticleRelativeToObject).asMatrix));
                    
                    Matrix globalToLocalRotationAndTranslationMatrix = hitObjectPhysicsComponent.calcGlobalToLocalRotationAndTranslationMatrix();
                    SpatialVectorDouble localImpulsePosition = SpatialVectorUtilities.toVector3(new SpatialVectorDouble(globalToLocalRotationAndTranslationMatrix * SpatialVectorUtilities.toVector4(globalImpulsePosition).asMatrix));

                    // PHYSICS CORRECTNESS NOTE< we handle the impulse as a force, no idea if this is 100% right >
                    applyForceToLinearAndAngularVelocity(hitObjectPhysicsComponent, localImpulseFromParticle, localImpulsePosition);
                }
                else if(particalCollisionResponse == EnumParticleCollisionResponse.SCATTER) {
                    // nothing to do
                }
                else if (particalCollisionResponse == EnumParticleCollisionResponse.DELETEA) {
                    particlesToRemove.Add(iParticle);
                }
            }

        }

        void removeParticles() {
            foreach (PhysicsComponent iParticleToRemove in particlesToRemove) {
                particles.Remove(iParticleToRemove);
            }
        }

        // informs all proximity detectors which can detect particles that the removed particles got removed
        void removeParticlesFromProximityDetectors() {
            // TODO
        }

        void resetParticlesToRemove() {
            particlesToRemove.Clear();
        }

        void checkProximityAndFireEvents() {
            ProximityHelper.checkProximityDetectorsAndFireEventsForParticles(privateProximityDetectors, particles);
            ProximityHelper.checkProximityDetectorsAndFireEventsForObjects(privateProximityDetectors, physicsAndMeshPairs.Select(v => v.physicsComponent));
        }

        // helper
        static SpatialVectorDouble calcImpulse(SpatialVectorDouble relativeVelocity, double mass) {
            // e = 1/2 * m * v²
            double velocityMagnitude = relativeVelocity.length;
            if(velocityMagnitude < double.Epsilon) {
                return new SpatialVectorDouble(new double[]{0,0,0});
            }
            else {
                return relativeVelocity.normalized().scale(0.5 * mass * velocityMagnitude * velocityMagnitude);
            }
        }

        public void addProximityDetector(ProximityDetector detector) {
            // make sure the bounding volumes are up to date
            // PERFORMANCE< we recalc the bounding volumes for bounding volume "dirty" objects to save some cycles >
            transformOnlyDirtyObjects();
            recalcBoundingVolumesForDirtyObjects();

            // do this to add all objects and particles which overlap
            IEnumerable<ProximityDetector> listOfOneProximityDetector = new ProximityDetector[] { detector };
            ProximityHelper.checkProximityDetectorsAndFireEventsForParticles(listOfOneProximityDetector, particles);
            ProximityHelper.checkProximityDetectorsAndFireEventsForObjects(listOfOneProximityDetector, physicsAndMeshPairs.Select(v => v.physicsComponent));

            privateProximityDetectors.Add(detector);
        }

        public void removeProximityDetector(ProximityDetector detector) {
            // TODO< fire EXIT event for all contained objects and particles >

            privateProximityDetectors.Remove(detector);
        }

        private IList<ProximityDetector> privateProximityDetectors = new List<ProximityDetector>();

        private RungeKutta4 rungeKutta4 = new RungeKutta4();
        public const double dt = 1.0 / 60.0;

        private ulong physicsComponentAndParticleIdCounter = 0;

        CelestialPhysics celestialPhysics = new CelestialPhysics();
    }

    internal class CollisionCallbackHelper {
        public IList<ICollisionHandler> collisionHandlers = new List<ICollisionHandler>();

        // normal is normalized
        public void beginContact(ref CollisionInformation collisionInformation) {
            foreach( ICollisionHandler iCollisionHandler in collisionHandlers ) {
                iCollisionHandler.beginContact(ref collisionInformation);
            }
        }

        // normal is normalized
        public void endContact(ref CollisionInformation collisionInformation) {
            foreach (ICollisionHandler iCollisionHandler in collisionHandlers) {
                iCollisionHandler.endContact(ref collisionInformation);
            }
        }

        public void instantaniousContact(ref CollisionInformation collisionInformation) {
            foreach (ICollisionHandler iCollisionHandler in collisionHandlers) {
                iCollisionHandler.instantaniousContact(ref collisionInformation);
            }
        }

        // for the case when no begin/end information is accesible or if the engine is configured this way that it doesn't take track of begin/end of a contact
        public void instantaniousParticleContact(ref CollisionInformation collisionInformation, out EnumParticleCollisionResponse particleResponse) {
            EnumParticleCollisionResponse? nonneutralParticleResponse = null;

            foreach (ICollisionHandler iCollisionHandler in collisionHandlers) {
                EnumParticleCollisionResponse calleeParticleResponse;
                iCollisionHandler.instantaniousParticleContact(ref collisionInformation, out calleeParticleResponse);
                if(calleeParticleResponse != EnumParticleCollisionResponse.NEUTRAL) {
                    nonneutralParticleResponse = calleeParticleResponse;
                }
            }

            particleResponse = nonneutralParticleResponse == null ? EnumParticleCollisionResponse.REFLECT : nonneutralParticleResponse.Value;
        }
    }

    internal struct ConvexHullRayIntersection {
        public static bool calcRayCollision(SpatialVectorDouble rayOrigin, SpatialVectorDouble rayDirection, SpatialVectorDouble[] convexHullPlanes, out SpatialVectorDouble? normal, out double tResult, out int? faceNumber) {
            int? fnormNum, bnormNum;

            RayPolyhedron.EnumReturnCode returnCode = RayPolyhedron.rayCvxPolyhedronInt(rayOrigin, rayDirection, double.PositiveInfinity, convexHullPlanes, out tResult, out normal, out fnormNum, out bnormNum);

            // function returns 4 component normal but we need only 3 components
            if( normal != null ) {
                normal = SpatialVectorUtilities.toVector3(normal.Value);
            }

            if (returnCode == RayPolyhedron.EnumReturnCode.FRONTFACE) {
                faceNumber = fnormNum;
                return true;
            }
            else if (returnCode == RayPolyhedron.EnumReturnCode.BACKFACE) {
                faceNumber = bnormNum;
                return true;
            }
            else {
                faceNumber = null;
                return false;
            }
        }

        public static bool checkRayCollision(SpatialVectorDouble rayOrigin, SpatialVectorDouble rayDirection, SpatialVectorDouble[] convexHullPlanes) {
            double tResult;
            SpatialVectorDouble? normal;
            int? faceNumber;

            return calcRayCollision(rayOrigin, rayDirection, convexHullPlanes, out normal, out tResult, out faceNumber);
        }
    }

    internal class CollisionDescriptor {
        public bool hit;

        public double rayDistance = double.PositiveInfinity;

        public SpatialVectorDouble faceNormal;

        public PhysicsComponentAndCollidersPair physicsComponentAndCollider; // of hit
        public uint faceIndex; // of hit
    }

    internal struct MeshRayIntersection {
        

        /* outdated, was implementing a universal (convex or concave) mesh vs ray check
         
        public static void checkMeshRayCollision(SpatialVectorDouble rayOrigin, SpatialVectorDouble rayDirection, PhysicsComponentAndCollidersPair physicsComponentAndMesh, CollisionDescriptor collisionDescriptor) {
            TransformedMeshComponent transformedMeshComponent = physicsComponentAndMesh.transformedMeshComponent;

            PlueckerCoordinate plueckerCoordinateOfRay = PlueckerCoordinate.createByVector(rayOrigin, rayDirection);

            for(int faceIndex = 0; faceIndex < transformedMeshComponent.meshComponent.mesh.faces.Length; faceIndex++) {
                if( !doesFaceIntersectRay(plueckerCoordinateOfRay, (uint)faceIndex, transformedMeshComponent) ) {
                    continue;
                }

                SpatialVectorDouble planeNormal;
                SpatialVectorDouble planeOrigin;
                {
                    uint
                        vertexIndex0 = physicsComponentAndMesh.transformedMeshComponent.meshComponent.mesh.faces[faceIndex].verticesIndices[0],
                        vertexIndex1 = physicsComponentAndMesh.transformedMeshComponent.meshComponent.mesh.faces[faceIndex].verticesIndices[1],
                        vertexIndex2 = physicsComponentAndMesh.transformedMeshComponent.meshComponent.mesh.faces[faceIndex].verticesIndices[2];

                    SpatialVectorDouble
                        p0 = physicsComponentAndMesh.transformedMeshComponent.transformedVertices[vertexIndex0],
                        p1 = physicsComponentAndMesh.transformedMeshComponent.transformedVertices[vertexIndex1],
                        p2 = physicsComponentAndMesh.transformedMeshComponent.transformedVertices[vertexIndex2];

                    SpatialVectorDouble
                        diff01 = p1 - p0,
                        diff02 = p2 - p0;

                    planeOrigin = p0;

                    planeNormal = SpatialVectorDouble.crossProduct(diff01, diff02);
                    // TODO< normalize >
                }

                // calculate distance of intersection to face
                double intersectionDistance = WhiteSphereEngine.geometry.Plane.calcD(planeOrigin, planeNormal, rayOrigin, rayDirection);

                // check if distance to plane is smaller than the last collision and bigger than 0.0
                if ( intersectionDistance < 0.0 ) {
                    continue;
                }

                if (collisionDescriptor.rayDistance < intersectionDistance) {
                    continue;
                }

                // store collision information
                collisionDescriptor.rayDistance = intersectionDistance;
                collisionDescriptor.faceIndex = (uint)faceIndex;
                collisionDescriptor.faceNormal = planeNormal;
                collisionDescriptor.physicsComponentAndMesh = physicsComponentAndMesh;
                collisionDescriptor.hit = true;
            }
        }
        */
        
        public static bool doesFaceIntersectRay(PlueckerCoordinate plueckerCoordinate, uint faceIndex, TransformedMeshComponent transformedMeshComponent) {
            uint[] verticesIndices = transformedMeshComponent.meshComponent.mesh.faces[(int)faceIndex].verticesIndices;

            for(int edgeIndex = 0; edgeIndex < verticesIndices.Length-1; edgeIndex++) {
                uint vertexIndex0 = verticesIndices[edgeIndex];
                uint vertexIndex1 = verticesIndices[edgeIndex+1];

                SpatialVectorDouble vertex0 = transformedMeshComponent.transformedVertices[vertexIndex0];
                SpatialVectorDouble vertex1 = transformedMeshComponent.transformedVertices[vertexIndex1];

                if(!checkPlueckerCoordinateAgainstEdge(plueckerCoordinate, vertex1, vertex0)) {
                    return false;
                }
            }

            // check last vertex to first
            {
                SpatialVectorDouble vertex0 = transformedMeshComponent.transformedVertices[verticesIndices.Length - 1];
                SpatialVectorDouble vertex1 = transformedMeshComponent.transformedVertices[0];

                if (!checkPlueckerCoordinateAgainstEdge(plueckerCoordinate, vertex1, vertex0)) {
                    return false;
                }
            }

            return true;
        }

        static bool checkPlueckerCoordinateAgainstEdge(PlueckerCoordinate plueckerCoordinate, SpatialVectorDouble vertex0, SpatialVectorDouble vertex1) {
            PlueckerCoordinate plueckerCoordinateOfEdge = PlueckerCoordinate.createByPandQ(vertex0, vertex1);
            return PlueckerCoordinate.checkCcw(plueckerCoordinate, plueckerCoordinateOfEdge);
        }
    }

    internal class ProximityHelper {
        public static void checkProximityDetectorsAndFireEventsForParticles(IEnumerable<ProximityDetector> proximityDetectors, IList<PhysicsComponent> particles) {
            // TODO< basically same code as checkProximityDetectorsAndFireEventsForObjects, but just with some other particle related method calls >
        }

        // NOTE< implementation just checks for AABB bounds and doesn't handle the sphere of an proximity detector >
        public static void checkProximityDetectorsAndFireEventsForObjects(IEnumerable<ProximityDetector> proximityDetectors, IEnumerable<PhysicsComponent> objects) {
            foreach( ProximityDetector iProximityDetector in proximityDetectors ) {
                // check and fire exit of objects

                IList<PhysicsComponent> removedElementsFromObjectsInside = new List<PhysicsComponent>();

                foreach( PhysicsComponent iCheckExitedObject in iProximityDetector.objectsInside ) {
                    bool exited = checkObjectExitingExitOfObject(iProximityDetector, iCheckExitedObject);
                    if( exited ) {
                        removedElementsFromObjectsInside.Add(iCheckExitedObject);

                        iProximityDetector.handleExitEventForObject(iCheckExitedObject);
                    }
                }

                // check for entered objects and fire
                foreach (PhysicsComponent iObject in objects) {
                    if( !KDop.checkIntersectAabb(iObject.boundingVolume.kdop, iProximityDetector.cachedKDop) ) {
                        continue;
                    }

                    if( iProximityDetector.objectsInside.Contains(iObject) ) {
                        continue;
                    }

                    // fire
                    iProximityDetector.handleEnterEventForObject(iObject);
                }

                // remove elements of list of objects inside which exited
                foreach (PhysicsComponent iExitedObject in removedElementsFromObjectsInside) {
                    iProximityDetector.objectsInside.Remove(iExitedObject);
                }

                // fire inside event for all objects still inside
                foreach(PhysicsComponent iObjectInside in iProximityDetector.objectsInside) {
                    iProximityDetector.handleInsideEventForObject(iObjectInside);
                }

                // add entered objects
                foreach (PhysicsComponent iObject in objects) {
                    if (!KDop.checkIntersectAabb(iObject.boundingVolume.kdop, iProximityDetector.cachedKDop)) {
                        continue;
                    }

                    if (iProximityDetector.objectsInside.Contains(iObject)) {
                        continue;
                    }

                    iProximityDetector.objectsInside.Add(iObject);
                }
            }
        }

        // checks if an object is exiting, the object must already been inside the proxymity
        static bool checkObjectExitingExitOfObject(ProximityDetector detector, PhysicsComponent object_) {
            return !KDop.checkIntersectAabb(object_.boundingVolume.kdop, detector.cachedKDop);
        }

        static bool checkParticleExitingExitOfObject(ProximityDetector detector, PhysicsComponent particle) {
            return !detector.cachedKDop.checkIntersectPosition(particle.position);
        }

        static bool checkObjectOverlap(ProximityDetector detector, PhysicsComponent object_) {
            return !KDop.checkIntersectAabb(object_.boundingVolume.kdop, detector.cachedKDop);
        }

        static bool checkParticleOverlap(ProximityDetector detector, PhysicsComponent particle) {
            return detector.cachedKDop.checkIntersectPosition(particle.position);
        }
    }

    internal class CelestialPhysics {
        public IList<CelestialObjectWithPosition> celestialObjects = new List<CelestialObjectWithPosition>();

        // calculates the gravitational forces and coresponding accelerations 
        public void calcForcesAndAccelerationsForPhysicsComponents(IEnumerable<PhysicsComponent> physicsComponents) {
            foreach (PhysicsComponent iPhysicsComponent in physicsComponents) {
                SpatialVectorDouble sumOfForce = new SpatialVectorDouble(new double[] { 0, 0, 0 });

                foreach (CelestialObjectWithPosition iCelestialObject in celestialObjects) {
                    SpatialVectorDouble extrapolatedPosition = iPhysicsComponent.rungeKutta4State.x + iPhysicsComponent.rungeKutta4State.v.scale(PhysicsEngine.dt);
                    SpatialVectorDouble difference = iCelestialObject.position - extrapolatedPosition;
                    SpatialVectorDouble direction = difference.normalized();
                    double distanceSquared = difference.lengthSquared;

                    double forceMagnitude = Orbit.calculateForceBetweenObjectsByDistance(iPhysicsComponent.mass, iCelestialObject.celestialObject.mass, distanceSquared);

                    sumOfForce += direction.scale(forceMagnitude);
                }

                SpatialVectorDouble acceleration = sumOfForce.scale(iPhysicsComponent.invMass);
                iPhysicsComponent.linearAcceleration += acceleration;
            }
        }
    }
}
