using System.Collections.Generic;

namespace WhiteSphereEngine.input {
    // remaps raw input events
    public class KeyboardInputRemapper {
        private KeyboardInputRemapper() {}

        public KeyboardInputRemapper(IKeyboardInputHandler keyboardInputHandler) {
            this.privateKeyboardInputHandler = keyboardInputHandler;

            // store default
            keyMapping['E'] = EnumRemappedKey.ENTITY_ACCELERATE;
            keyMapping['C'] = EnumRemappedKey.ENTITY_DEACCELERATE;
            keyMapping['A'] = EnumRemappedKey.ENTITY_YAWADD;
            keyMapping['D'] = EnumRemappedKey.ENTITY_YAWSUB;
            keyMapping['W'] = EnumRemappedKey.ENTITY_PITCHADD;
            keyMapping['S'] = EnumRemappedKey.ENTITY_PITCHSUB;
        }
        
        public void remapKeyDown(int alphanumericKeyCode, KeyCodes.EnumKeyType keyType, KeyCodes.EnumKeyCode? keyCode) {
            if( keyMapping.ContainsKey(alphanumericKeyCode) ) {
                privateKeyboardInputHandler.keyDown(keyMapping[alphanumericKeyCode]);
            }
        }

        public void remapKeyUp(int alphanumericKeyCode, KeyCodes.EnumKeyType keyType, KeyCodes.EnumKeyCode? keyCode) {
            if (keyMapping.ContainsKey(alphanumericKeyCode)) {
                privateKeyboardInputHandler.keyUp(keyMapping[alphanumericKeyCode]);
            }
        }

        public IDictionary<int, EnumRemappedKey> keyMapping = new Dictionary<int, EnumRemappedKey>();

        IKeyboardInputHandler privateKeyboardInputHandler;

        public IKeyboardInputHandler keyboardInputHandler {
            set {
                privateKeyboardInputHandler = value;
            }
        }
    }
}
