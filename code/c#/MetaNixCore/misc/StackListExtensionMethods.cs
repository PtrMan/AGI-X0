using System.Collections.Generic;

namespace MetaNix.misc {
    public static class ListStackExtensionMethods {
        public static void push<T>(this List<T> list, T element) {
            list.Add(element);
        }

        public static T pop<T>(this List<T> list) {
            if( list.Count == 0 ) {
                throw new System.Exception("Can't pop from empty stack!");
            }

            T result = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return result;
        }
    }
}
