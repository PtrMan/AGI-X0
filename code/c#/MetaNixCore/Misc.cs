using System.Collections.Generic;

namespace MetaNix {
    namespace misc {

        public sealed class SetHelpers {
            public static ISet<uint> toSet(uint[] arr) {
                ISet<uint> result = new HashSet<uint>();
                foreach (uint iArr in arr) {
                    result.Add(iArr);
                }
                return result;
            }

            public static ISet<uint> toSet(IList<uint> arr) {
                ISet<uint> result = new HashSet<uint>();
                foreach (uint iArr in arr) {
                    result.Add(iArr);
                }
                return result;
            }

            public static ISet<uint> subtract(ISet<uint> a, ISet<uint> b) {
                ISet<uint> result = new HashSet<uint>();
                foreach (uint ia in a) {
                    if (!b.Contains(ia)) {
                        result.Add(ia);
                    }
                }
                return result;
            }

            public static ISet<uint> union(ISet<uint> a, ISet<uint> b) {
                ISet<uint> result = new HashSet<uint>();
                foreach (uint ai in a) {
                    result.Add(ai);
                }

                foreach (uint bi in b) {
                    result.Add(bi);
                }
                return result;
            }

        }


        public static class Extensions {
            public static bool isEmpty<Type>(this IList<Type> list) {
                return list.Count == 0;
            }
        }
    }

    class Misc {
        // used for parsing
        static public bool isLetter(char text) {
            return text.ToString().ToLower().IndexOfAny(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }) != -1;
        }
    }
    


}
