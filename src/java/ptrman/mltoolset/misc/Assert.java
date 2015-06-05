package ptrman.mltoolset.misc;

public class Assert {
    public static void Assert(boolean condition, String message) {
        if( !condition ) {
            throw new RuntimeException("ASSERT : " + message);
        }
    }
}
