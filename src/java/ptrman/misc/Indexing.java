package ptrman.misc;

public class Indexing {
    /** calculate the index inside a triangle strip

    */
    /*
     examples

     0
     result 0

     0
     xx
     result 0xx

     0
     xx
     000
     result 0xx000

     ...
     */
    public static Tuple<Integer, Integer> getIndicesOfTriangle(final int index) {
        int yIndex = 0;
        int width = 1;
        int remainingIndex = index;

        for(;;) {
            if( remainingIndex < width ) {
                return new Tuple<>(remainingIndex, yIndex);
            }

            remainingIndex -= width;
            yIndex++;
            width++;
        }
    }

    public static int calculateMaxIndexOfTriangle(final int sidelength) {
        return (sidelength*sidelength - sidelength) / 2;
    }
}
