using System.Collections.Generic;

namespace MetaNix.misc {
    public static class MyListExtensions {
        public static void limitSize<E>(this List<E> list, ulong maxLength) {
            if( list.Count > (int)maxLength ) {
                list.RemoveRange((int)maxLength - 1, list.Count - (int)maxLength);
            }
        }
        
        public static bool isEmpty<T>(this ICollection<T> collection) {
            return collection.Count == 0;
        }
    }
}
