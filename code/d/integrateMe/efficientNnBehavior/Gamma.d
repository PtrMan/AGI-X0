
// code translated from https://github.com/PtrMan/ai2/blob/4d2d983e0090d42de3deb7140b606b30490d0fb4/ai2/Math/Gamma.cs
// code is from mrob.com/pub/ries/lanczos-gamma.html
// LICENSE UNKNOWN

// The following constants LG_g and LG_N are the "g" and "n" parameters
// for the table of coefficients that follows them; several alternative
// coefficients are available at mrob.com/pub/ries/lanczos-gamma.html
private static const double LG_g = 5.0; // Lanczos parameter "g"
private static const double[] LanczosConstants = [
    1.000000000190015,
    76.18009172947146,
    -86.50532032941677,
    24.01409824083091,
    -1.231739572450155,
    0.1208650973866179e-2,
    -0.5395239384953e-5
];

private static const double LNSQRT2PI = 0.91893853320467274178;

import std.math : PI, log, sin, exp;

// Compute the logarithm of the Gamma function using the Lanczos method.
double lanczosLnGamma(double z) {
    double sum;
    double baseValue;
    int i;

    if( z < 0.5 ) {
        // Use Euler's reflection formula:
        // Gamma(z) = Pi / [Sin[Pi*z] * Gamma[1-z]];
        return log(PI / sin(PI * z)) - lanczosLnGamma(1.0 - z);
    }
    
    z = z - 1.0;
    baseValue = z + LG_g + 0.5;  // Base of the Lanczos exponential
    sum = 0;

    // We start with the terms that have the smallest coefficients and largest
    // denominator.
    i = cast(int)LanczosConstants.length-1;
    for( ; i >= 1; i-- ) {
        sum += LanczosConstants[i] / (z + (cast(double)i));
    }
    sum += LanczosConstants[0];

    // This printf is just for debugging
    //printf("ls2p %7g  l(b^e) %7g   -b %7g  l(s) %7g\n", LNSQRT2PI, log(base)*(z+0.5), -base, log(sum));
    
    // Gamma[z] = Sqrt(2*Pi) * sum * base^[z + 0.5] / E^base
    return ((LNSQRT2PI + log(sum)) - baseValue) + log(baseValue)*(z+0.5);
}

// Compute the Gamma function, which is e to the power of ln_gamma.
double lanczosGamma(double z) {
    return exp(lanczosLnGamma(z));
}

// src http://web.science.mq.edu.au/~mjohnson/code/digamma.c
// LICENSE UNKNOWN
double diGamma(double x) {
    double result = 0, xx, xx2, xx4;

    assert(x > 0.0);
    
    for ( ; x < 7; ++x) {
        result -= 1/x;
    }
    
    x -= 1.0/2.0;
    xx = 1.0/x;
    xx2 = xx*xx;
    xx4 = xx2*xx2;
    result += log(x)+(1.0/24.0)*xx2-(7.0/960.0)*xx4+(31.0/8064.0)*xx4*xx2-(127.0/30720.0)*xx4*xx4;
    return result;
}
