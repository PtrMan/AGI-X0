using WhiteSphereEngine.math;
using WhiteSphereEngine.misc;

namespace WhiteSphereEngine.physics.rigidBody {
    internal class Angular {
        // translated from https://github.com/PtrMan/SpaceSimCore/blob/e7fe1b9af722339c95037660b2c5d71b880deea7/include/physics/Angular.hpp

        // https://en.wikiversity.org/wiki/Physics_equations/Impulse,_momentum,_and_motion_about_a_fixed_axis
        // r is the distance from the orgin of the body where the force is applied
        public static SpatialVectorDouble calculateTorque(SpatialVectorDouble r, SpatialVectorDouble force) {
            return SpatialVectorDouble.crossProduct(r, force);
        }

        // calculate the angular acceleration of a torque (which is calculated with calculate torque)
        // used to calculate the angular acceleration of a body when a force pulls on it

        // derivation of formula:
        // https://en.wikiversity.org/wiki/Physics_equations/Impulse,_momentum,_and_motion_about_a_fixed_axis
        // tau = I * alpha  | * I^(-1)
        // alpha = tau * (I^(-1))
        public static SpatialVectorDouble calculateRotationalAcceleration(ChangeCallbackCalculateInverse inertiaTensorAndInverse, SpatialVectorDouble torque) {
            return new SpatialVectorDouble(inertiaTensorAndInverse.getInverse() * torque.asMatrix);
        }
    }
}
