using System.Linq;

namespace MetaNix.misc {
    public static class ArrayUtilities {
        public static void fill<T>(ref T[] array, T value) {
            array = array.Select(i => value).ToArray();
        }
    }
}
