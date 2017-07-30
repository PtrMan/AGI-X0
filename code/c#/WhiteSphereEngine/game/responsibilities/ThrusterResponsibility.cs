using System.Collections.Generic;
using System.Diagnostics;

using WhiteSphereEngine.math;
using WhiteSphereEngine.physics.rigidBody;

namespace WhiteSphereEngine.game.responsibilities {
    // maps physics object id's to thrusters of the object
    public class ThrusterResponsibility {
        public class ThrusterBinding {
            public AttachedForce attachedForce;

            public double maximalForce; // maximal force which the thruster can do

            public float relative = 0.0f; // relative force, used by control logic

            public string tag; // used to specify special thrusters, can be null

            public SpatialVectorDouble localDirection {
                get {
                    return attachedForce.localDirection;
                }
            }

            // REFACTOR ASK< holds too many not really related parameters, needs maybe an refactor if this gets too complicated. This attribute is a first oportunity to do so >
            public ThrusterAdditionalInformation additionalInformation;

            public struct ThrusterAdditionalInformation {
                // is the angular acceleration firing of the thruster is doing on the object where it is attached to
                public SpatialVectorDouble cachedAngularAccelerationOnObject;
            }
        }

        public IDictionary<ulong, IList<ThrusterBinding>> physicsObjectIdToThrusters = new Dictionary<ulong, IList<ThrusterBinding>>();

        public void recalcAngularAccelerationOfThrustersForObjectId(PhysicsEngine physicsEngine, ulong objectId) {
            Trace.Assert(physicsObjectIdToThrusters.ContainsKey(objectId));

            if( !physicsObjectIdToThrusters.ContainsKey(objectId) ) {
                // indicates a bug in some code because we expect this to be the case but it is not fatal

                // TODO< log >
                return;
            }

            IList<ThrusterBinding> thrusterBindings = physicsObjectIdToThrusters[objectId];
            foreach( ThrusterBinding iThrusterBinding in thrusterBindings ) {
                PhysicsComponent objectPhysicsComponent = physicsEngine.getObjectById(objectId);
                iThrusterBinding.additionalInformation.cachedAngularAccelerationOnObject = objectPhysicsComponent.calcAngularAccelerationOfRigidBodyForAppliedForce(iThrusterBinding.attachedForce.objectLocalPosition, iThrusterBinding.localDirection.scale(iThrusterBinding.maximalForce));
            }
        }
    }
}
