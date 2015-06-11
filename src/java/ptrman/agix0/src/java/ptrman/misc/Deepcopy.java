package ptrman.misc;

import java.util.ArrayList;
import java.util.List;

public class Deepcopy {
    public static<Type> List<Type> deepCopyList(List<Type> input) {
        List<Type> result = new ArrayList<>();
        for( Type __dummyForeachVar0 : input ) {
            result.add(__dummyForeachVar0);
        }
        return result;
    }
}

