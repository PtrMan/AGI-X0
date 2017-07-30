using WhiteSphereEngine.math;

namespace WhiteSphereEngine.physics.rigidBody {
    // translation of https://github.com/PtrMan/SpaceSimCore/blob/master/include/physics/AttachedForce.h 
    public class AttachedForce {
        private AttachedForce() {}
        public AttachedForce(SpatialVectorDouble objectLocalPosition, SpatialVectorDouble localDirection) {
            this.objectLocalPosition = objectLocalPosition;
            this.localDirection = localDirection;
        }

        // TODO< can be float >
        public SpatialVectorDouble objectLocalPosition; // position where the force is applied
        public SpatialVectorDouble forceVectorInNewton {
            get {
                return localDirection.scale(forceInNewton);
            }
        }

        public SpatialVectorDouble localDirection; // must be normalized

        public double forceInNewton = 0;
    }
}
