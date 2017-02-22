using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix.symbolic {
    public interface IStringAccessor<Type> {
        Type at(int idx);
        int length();
    }

    public class StringManipulation {

        // see pseudocode https://en.wikipedia.org/wiki/Knuth%E2%80%93Morris%E2%80%93Pratt_algorithm#Description_of_pseudocode_for_the_search_algorithm
        // \param T is the precomputed lookup table for the search string
        public static int kmpSearch(IStringAccessor<int> S, IStringAccessor<int> W, int[] T, out bool found) {
            found = false;

            int m = 0; // the beginning of the current match in S
            int i = 0; // the position of the current character in W

            while (m + i < S.length()) {
                if (W.at(i) == S.at(m + i)) {
                    if (i == W.length() - 1) {
                        found = true;
                        return m;
                    }
                    i++;
                }
                else {
                    if (T[i] > -1) {
                        m = m + i - T[i]; i = T[i];
                    }
                    else {
                        m++;
                        i = 0;
                    }
                }
            }

            // if we reach here, we have serched all of S unsuccessfully
            return -1;
        }

        // see pseudocode https://en.wikipedia.org/wiki/Knuth%E2%80%93Morris%E2%80%93Pratt_algorithm#Description_of_pseudocode_for_the_table-building_algorithm
        // \param W the word to be analyzed
        // \param T the table to be filled
        public static void kmpTable(IStringAccessor<int> W, ref int[] T) {
            if (W.length() != T.Length) {
                throw new Exception();
            }

            int pos = 2; // the current position we are computing in T
            int cnd = 0; // the zero-based index in W of the next character of the current candidate substring

            // the first few values are fixed but different fro what the algorithm might suggest
            T[0] = -1;
            T[1] = 0;

            while (pos < W.length()) {
                // first case: the substring continues
                if (W.at(pos - 1) == W.at(cnd)) {
                    T[pos] = cnd + 1;
                    cnd++;
                    pos++;
                }
                // second case: it doesn't, but we can fall back)
                else if (cnd > 0) {
                    cnd = T[cnd];
                }

                // third case: we have run out of candidates. Note cnd = 0
                else {
                    T[pos] = 0;
                    pos++;
                }
            }
        }
        
    }
    
}
