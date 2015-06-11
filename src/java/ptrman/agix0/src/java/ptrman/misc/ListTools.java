package ptrman.misc;

import java.util.List;

public class ListTools {
    public static boolean isListTheSameInt(List<Integer> a, List<Integer> b) {
        int i;
        if( a.size() != b.size() ) {
            return false;
        }
         
        for (i = 0;i < a.size();i++) {
            if( a.get(i) != b.get(i) ) {
                return false;
            }
        }
        return true;
    }
}

