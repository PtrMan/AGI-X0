using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CompactIntegerVector {
    public CompactIntegerVector(uint length, uint bitsPerElement) {
        this.length = length;
        this.bitsPerElement = bitsPerElement;
        this.cachedMask = (1ul << (int)bitsPerElement) - 1;
        this.cachedElementsPerVectorElement = 64 / bitsPerElement;

        vector = new ulong[MyMath.divRoundUp(length * bitsPerElement, 64)];
    }

    public uint getElement(uint index) {
        uint vectorIndex = index / cachedElementsPerVectorElement;
        uint indexInVectorElement = index % cachedElementsPerVectorElement;

        ulong shiftedContent = protectedVector[vectorIndex] >> (int)(indexInVectorElement * bitsPerElement);
        ulong maskedContent = shiftedContent & cachedMask;
        return (uint)maskedContent;
    }

    public void setElement(uint index, uint value) {
        uint vectorIndex = index / cachedElementsPerVectorElement;
        uint indexInVectorElement = index % cachedElementsPerVectorElement;
        
        ulong mask = cachedMask << (int)(indexInVectorElement * bitsPerElement);
        protectedVector[vectorIndex] = ((~mask) & protectedVector[vectorIndex]) | (value << (int)(indexInVectorElement * bitsPerElement));
    }

    public ulong[] vector {
        get { return protectedVector; }
        set { protectedVector = value; }
    }

    protected ulong[] protectedVector;
    protected uint length;
    protected uint bitsPerElement;

    protected uint cachedElementsPerVectorElement;
    protected ulong cachedMask;
}

