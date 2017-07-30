using WhiteSphereEngine.math;

namespace WhiteSphereEngine.physics.rigidBody {
    public struct RayHitDescriptor {
        public SpatialVectorDouble hitPosition;
        public SpatialVectorDouble hitNormal;
        public PhysicsComponentAndCollidersPair hitPhysicsComponentAndCollider;
    }
}
