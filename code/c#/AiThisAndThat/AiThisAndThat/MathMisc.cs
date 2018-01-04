namespace AiThisAndThat {
    public static class MathMisc {
        // see https://codingforspeed.com/using-faster-exponential-approximation/
        public static float expFast10(float v) {
            const int precision = 10;

            v = (float)1 + v / (1 << precision);
            for(int i = 0; i < precision; i++)   v = v*v;
            return v;
        }
    }
}
