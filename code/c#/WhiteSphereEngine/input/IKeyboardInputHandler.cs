namespace WhiteSphereEngine.input {
    public enum EnumRemappedKey {
        ENTITY_ACCELERATE,
        ENTITY_DEACCELERATE,
        ENTITY_PITCHADD,
        ENTITY_PITCHSUB,
        ENTITY_YAWADD,
        ENTITY_YAWSUB,
        ENTITY_ROLLADD,
        ENTITY_ROLLSUB,
    }

    public interface IKeyboardInputHandler {
        void keyDown(EnumRemappedKey remappedKey);
        void keyUp(EnumRemappedKey remappedKey);

        // for direct text input mode
        void keyDown(int keyCode);
        void keyUp(int keyCode);
    }
}