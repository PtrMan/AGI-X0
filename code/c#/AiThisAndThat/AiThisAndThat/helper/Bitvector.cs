using System.Diagnostics;
using System.Collections.Generic;

class SlowBitvector {
    public List<bool> vector;

    public static bool equals(SlowBitvector a, SlowBitvector b) {
        if (a.vector.Count != b.vector.Count) {
            return false;
        }

        for (int i = 0; i < a.vector.Count; i++) {
            if (a.vector[i] != b.vector[i]) {
                return false;
            }
        }

        return true;
    }

    public SlowBitvector subvector(uint startIndex, uint endIndex) {
        Debug.Assert(endIndex >= startIndex, "Negative length is not valid!");

        SlowBitvector result = new SlowBitvector();
        result.vector = new List<bool>(new bool[endIndex - startIndex]);

        int iResult = 0;
        for (int i = (int)startIndex; i < (int)endIndex; i++) {
            result.vector[iResult] = vector[i];
            iResult++;
        }

        return result;
    }

    public static bool operator ==(SlowBitvector a, SlowBitvector b) {
        if (System.Object.ReferenceEquals(a, b)) {
            return true;
        }

        // If one is null, but not both, return false.
        //if (((object)a == null) || ((object)b == null)) {
        //    return false;
        //}

        // Return true if the fields match:
        if( a.vector.Count != b.vector.Count ) {
            return false;
        }

        for( int i = 0; i < a.vector.Count; i++ ) {
            if( a.vector[i] != b.vector[i] ) {
                return false;
            }
        }

        return true;
    }

    public static bool operator !=(SlowBitvector a, SlowBitvector b) {
        return !(a == b);
    }


}

