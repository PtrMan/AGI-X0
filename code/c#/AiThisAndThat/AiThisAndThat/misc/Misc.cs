namespace AiThisAndThat.misc {
    public static class Misc {
        public static float[] deepCopy(this float[] arr) {
            float[] result = new float[arr.Length];
            for(int i = 0; i < arr.Length; i++) result[i] = arr[i];
            return result;
        }

        public static double[] deepCopy(this double[] arr) {
            double[] result = new double[arr.Length];
            for(int i = 0; i < arr.Length; i++) result[i] = arr[i];
            return result;
        }

        public static int[] deepCopy(this int[] arr) {
            int[] result = new int[arr.Length];
            for(int i = 0; i < arr.Length; i++) result[i] = arr[i];
            return result;
        }

        public static uint[] deepCopy(this uint[] arr) {
            uint[] result = new uint[arr.Length];
            for(int i = 0; i < arr.Length; i++) result[i] = arr[i];
            return result;
        }

        public static string[] deepCopy(this string[] arr) {
            string[] result = new string[arr.Length];
            for(int i = 0; i < arr.Length; i++) result[i] = arr[i];
            return result;
        }
    }
}
