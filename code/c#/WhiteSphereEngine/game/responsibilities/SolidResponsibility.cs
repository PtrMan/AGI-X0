using System.Collections.Generic;

using System.Diagnostics;

using WhiteSphereEngine.math;
using WhiteSphereEngine.physics.rigidBody;
using WhiteSphereEngine.physics.microTimestepSimulation;

namespace WhiteSphereEngine.game.responsibilities {
    // used to keep track of solids bound to physics objects  and fracturing the solids.
    // Fracturing solids can be used for explosions for various effects like rockets, grenades, proximity bombs, etc.
    public class SolidResponsibility {
        public PhysicsObjectIdToSolidClusterMapping mapping;
        public PhysicsEngine physicsEngine;

        public List<FracturedParticle> fractureSolidClusterAndRemoveFromMappingByPhysicsObject(PhysicsComponent physicsObject) {
            List<FracturedParticle> fracturedParticles = new List<FracturedParticle>();

            ulong physicsObjectId = physicsObject.id;

            Trace.Assert(mapping.physicsObjectIdToSolidCluster.ContainsKey(physicsObject.id));
            if( !mapping.physicsObjectIdToSolidCluster.ContainsKey(physicsObject.id) ) {
                return new List<FracturedParticle>(); // we should never get here, but it's not fatal
            }
            
            SolidCluster physicsComponentOfMappedObject = mapping.physicsObjectIdToSolidCluster[physicsObject.id];
            
            // fracture all solids
            foreach(SolidCluster.SolidWithPositionAndRotation iSolidWithPositionAndRotation in physicsComponentOfMappedObject.solids) {
                fracturedParticles.AddRange(fractureSolidWithPositionAndRotation(physicsObject.position, physicsObject.rotation, iSolidWithPositionAndRotation));
            }


            mapping.physicsObjectIdToSolidCluster.Remove(physicsObject.id);

            return fracturedParticles;
        }

        IList<FracturedParticle> fractureSolidWithPositionAndRotation(SpatialVectorDouble objectGlobalPosition, Quaternion objectRotation, SolidCluster.SolidWithPositionAndRotation solidWithPositionAndRotation) {
            uint roughtlyNumberOfFracturedElements = 64;
            
            IList<FracturedParticle> fracturedParticles = SimpleFracturing.fractureSolid(solidWithPositionAndRotation.solid, roughtlyNumberOfFracturedElements);

            // transform positions from local to global
            foreach(FracturedParticle iFracturedParticle in fracturedParticles) {
                Matrix localToGlobalTranslation = MatrixUtilities.calcLocalToGlobalTranslationMatrix(objectGlobalPosition);
                Matrix localToGlobalRotation = QuaternionUtilities.convToRotationMatrix4(objectRotation);

                Matrix localToGlobal =
                    (localToGlobalTranslation * localToGlobalRotation) *
                    MatrixUtilities.calcLocalToGlobalRotationAndTranslationMatrix(solidWithPositionAndRotation.localPosition, solidWithPositionAndRotation.localRotation);

                iFracturedParticle.relativePosition = SpatialVectorUtilities.toVector3(new SpatialVectorDouble(localToGlobal* SpatialVectorUtilities.toVector4(iFracturedParticle.relativePosition).asMatrix));
            }

            return fracturedParticles;
        }
    }
}
