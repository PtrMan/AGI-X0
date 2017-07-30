using WhiteSphereEngine.input;

namespace WhiteSphereEngine.gameLogic {
    // translates keyboard input to commands of the controlled entity
    class PlayerShipControlInputHandler : IKeyboardInputHandler {
        private PlayerShipControlInputHandler() {}
        public PlayerShipControlInputHandler(EntityController entityController) {
            this.entityController = entityController;
        }

        public void keyDown(EnumRemappedKey remappedKey) {
            if( remappedKey == EnumRemappedKey.ENTITY_ACCELERATE ) {
                entityController.inputAcceleration = 1.0f;
            }
            else if( remappedKey == EnumRemappedKey.ENTITY_DEACCELERATE ) {
                entityController.inputAcceleration = -1.0f;
            }
            else if( remappedKey == EnumRemappedKey.ENTITY_PITCHADD ) {
                entityController.inputPitch = 1.0f;
            }
            else if (remappedKey == EnumRemappedKey.ENTITY_PITCHSUB) {
                entityController.inputPitch = -1.0f;
            }
            else if (remappedKey == EnumRemappedKey.ENTITY_YAWADD) {
                entityController.inputYaw = 1.0f;
            }
            else if (remappedKey == EnumRemappedKey.ENTITY_YAWSUB) {
                entityController.inputYaw = -1.0f;
            }
        }

        public void keyUp(EnumRemappedKey remappedKey) {
            if (remappedKey == EnumRemappedKey.ENTITY_ACCELERATE || remappedKey == EnumRemappedKey.ENTITY_DEACCELERATE) {
                entityController.inputAcceleration = 0.0f;
            }
            else if( remappedKey == EnumRemappedKey.ENTITY_PITCHADD || remappedKey == EnumRemappedKey.ENTITY_PITCHSUB ) {
                entityController.inputPitch = 0.0f;
            }
            else if( remappedKey == EnumRemappedKey.ENTITY_YAWADD || remappedKey == EnumRemappedKey.ENTITY_YAWSUB ) {
                entityController.inputYaw = 0.0f;
            }
        }

        public void keyDown(int keyCode) {
        }

        public void keyUp(int keyCode) {
        }

        private EntityController entityController;
    }
}
