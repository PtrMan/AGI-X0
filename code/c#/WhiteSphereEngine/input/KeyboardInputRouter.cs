using WhiteSphereEngine.subsystems.gui.input;

namespace WhiteSphereEngine.input {
    // decides if keyboard input is transmitted to GUI code orto the input remapper for control of the controlled spacevehicle
    public class KeyboardInputRouter {
        public enum EnumDestination {
            INPUTREMAPPER,
            GUI, // send to GUI
        }

        KeyboardInputRemapper inputRemapper;
        KeyboardEventRouterOfGui guiEventRouter;

        public EnumDestination destination = EnumDestination.INPUTREMAPPER;
        private KeyboardInputRemapper keyboardInputRemapper;

        private KeyboardInputRouter(){}
        public KeyboardInputRouter(KeyboardInputRemapper inputRemapper, KeyboardEventRouterOfGui guiEventRouter) {
            this.inputRemapper = inputRemapper;
            this.guiEventRouter = guiEventRouter;
        }
        
        public void routeKeyDown(int alphanumericKeyCode, KeyCodes.EnumKeyType keyType, KeyCodes.EnumKeyCode? keyCode) {
            if( destination == EnumDestination.INPUTREMAPPER ) {
                inputRemapper.remapKeyDown(alphanumericKeyCode, keyType, keyCode);
            }
            else {
                guiEventRouter.keyDown(alphanumericKeyCode, keyType, keyCode);
            }
        }

        public void routeKeyUp(int alphanumericKeyCode, KeyCodes.EnumKeyType keyType, KeyCodes.EnumKeyCode? keyCode) {
            if (destination == EnumDestination.INPUTREMAPPER) {
                inputRemapper.remapKeyUp(alphanumericKeyCode, keyType, keyCode);
            }
            else {
                guiEventRouter.keyUp(alphanumericKeyCode, keyType, keyCode);
            }
        }
    }
}
