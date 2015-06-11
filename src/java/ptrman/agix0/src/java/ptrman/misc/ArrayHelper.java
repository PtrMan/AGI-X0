package ptrman.misc;

import java.util.ArrayList;
import java.util.List;

public class ArrayHelper {
    public static<Type> List<Type> asList2(Type a, Type b) {
        List<Type> result = new ArrayList<>();

        result.add(a);
        result.add(b);

        return result;
    }
}
