namespace WhiteSphereEngine.input {
    public class KeyCodes {
        public enum EnumKeyCode {
            BACKSLASH,

            // directional keys
            DIRECTION_LEFT,
            DIRECTION_RIGHT,
            DIRECTION_UP,
            DIRECTION_DOWN,
        }

        public enum EnumKeyType {
            ALPHANUMERIC, // A-Z 0-9, special signs
            CONTROL, // left,right,up,down
        }
    }
}
