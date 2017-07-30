using System.Collections.Generic;

using WhiteSphereEngine.entity;
using WhiteSphereEngine.game.responsibilities;
using WhiteSphereEngine.gameLogic;
using WhiteSphereEngine.physics.rigidBody;

namespace WhiteSphereEngine.game.entityComponents {
    // connects the input of the controller to the thurst and attitude control
    public class VehicleControllerComponent : IComponent {
        private VehicleControllerComponent() {} // disable standard ctor
        public VehicleControllerComponent(AttitudeAndAccelerationControlResponsibility attitudeAndAccelerationControlResponsibility) {
            this.attitudeAndAccelerationControlResponsibility = attitudeAndAccelerationControlResponsibility;
        }
        
        public bool requiresUpdate => true;
        public void update(Entity entity) {
            if( controller == null ) {
                return; // it's fine if no controller is attached
            }
            
            PhysicsComponent objectOfEntity = entity.getSingleComponentsByType<PhysicsComponent>();
            attitudeAndAccelerationControlResponsibility.controlSolve(objectOfEntity, controller.inputRoll, controller.inputPitch, controller.inputYaw);
            attitudeAndAccelerationControlResponsibility.controlNamedPair(entity.getSingleComponentsByType<PhysicsComponent>(), "accelerate", controller.inputAcceleration);
        }

        public void entry(Entity parentEntity) {
            // do nothing
        }

        public void @event(string type, IDictionary<string, object> parameters) {
            // do nothing
        }

        // public so the controller can be exchanged on the fly
        public EntityController controller;

        AttitudeAndAccelerationControlResponsibility attitudeAndAccelerationControlResponsibility;
    }
}
