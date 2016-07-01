using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CompactIntegerVectorModifier {
    // works for power2 and nonpower2, is slower for power2 than the optimized version
    public static void incrementGeneric(CompactIntegerVector integerVector, uint maxValue) {
        for( uint currentIndex = 0;; currentIndex++) {
            uint element = integerVector.getElement(currentIndex);
            uint newElement = element + 1;
            
            if( newElement == maxValue ) {
                integerVector.setElement(currentIndex, 0);
            }
            else {
                integerVector.setElement(currentIndex, newElement);
                return;
            }
        }
    }

    public static void incrementPower2(CompactIntegerVector integerVector) {
        // we exploit the fact that the value automatically carries the value
        ulong oldValue = integerVector.vector[0]++;
  
        // TODO< handling of overflow >

    }
}

