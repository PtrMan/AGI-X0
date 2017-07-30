using WhiteSphereEngine.math;
using WhiteSphereEngine.misc;

namespace WhiteSphereEngine.physics.rigidBody {
    // exposes some physics engine functionality public to be used in other parts of the game(engine)
    public class PhysicsEngineUtilities {
        public static SpatialVectorDouble calcAngularAccelerationOfRigidBodyForAppliedForce(ChangeCallbackCalculateInverse inertiaTensor, SpatialVectorDouble objectLocalPositionOfForce, SpatialVectorDouble objectLocalForce) {
            SpatialVectorDouble appliedTorque = Angular.calculateTorque(objectLocalPositionOfForce, objectLocalForce);
            SpatialVectorDouble angularAcceleration = Angular.calculateRotationalAcceleration(inertiaTensor, appliedTorque);

            return angularAcceleration;
        }
    }
}
