import std.stdio;

// paper
// "Sorting in linear expected time"
// [linear]

import std.algorithm.comparison : min, max;

private void findMinAndMax(Type)(Type[] arr, out Type min, out Type max) {
    assert(arr.length > 0);
    foreach( e; arr ) {
        min = .min(min, e);
        max = .max(max, e);
    }
}

// \param n : number of elements
private int calcM(int k, int n) {
    assert(n > 0);
    
    int m = (n - 1) / k;
    assert( 1 + m*k <= n);
    assert( n < 1 + (m + 1)*k);
    
    return m;
}

// as described in [linear] page 6
private int[] calcFrequencyCount(Type)(Type[] arr) {
    int n = arr.length; // n is number of elements

    int nIntervals;
    
    if( n < 1000 ) {
        nIntervals = 10;
    }
    else {
        nIntervals = n / 100;
    }
    
    float sampleConstant = nIntervals / (sampleMax - sampleMin);
    
    int freq[];
    freq.length = nintervals;
    foreach( i; 0..nintervals ) {
        freq[i] = 0;
    }
    
    for( int i = 0;; ) {
        int j = cast(int)((arr[i] - sampleMin)*sampleConstant);
        freq[j] = freq[j] + 1;
        i += k;
        
        if( i > n ) {
            break;
        }
    }

    freq[nIntervals - 1] = freq[nIntervals - 1] + freq[nIntervals];
    
    return freq;
}

// links the elements into the different bins
// see paper [linear]
void linkElements(int[] a, int sampleMin, int sampleMax, int sampleConstant,   float intervalMin[], float[] constant, float[] offset,  int extrabin, ref int[] link, out uint[] lhead) {
  lhead.length = 0;
  lhead.length = extrabin; // we assume that it is reseted to zero
  
  size_t n = a.length;
  
  foreach( i; 0..n ) {
    int j2;
    
    if( a[i] <= sampleMin ) {
      j2 = 0;
    }
    else if( a[i] >= sampleMax ) {
      j2 = extrabin;
    }
    else {
      int j1 = cast(int)((a[i] - sampleMin) * sampleConstant);
      j2 = cast(int)((a[i] - intervalMin[j1]) * constant[j1] + offset[j1] );
    }
    
    link[i] = lhead[j2];
    lhead[j2] = cast(int)i;
  }
}
