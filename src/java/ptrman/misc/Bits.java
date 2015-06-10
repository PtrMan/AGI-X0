package ptrman.misc;

public class Bits {
    public static int countBitsSlow(int value) {
        int bits = 0;

        for( int i = 0; i < 32; i++ ) {
            if( (value & (1 << i)) != 0 ) {
                bits++;
            }
        }

        return bits;
    }
}
