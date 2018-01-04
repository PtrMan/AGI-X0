module NDimensionalHypersphere;

import std.math : sqrt, PI;

import misced.IntegerExponentiation;
import Gamma;

// volume of n dimensional sphere
// seee https://en.wikipedia.org/wiki/N-sphere#Volume_and_surface_area
double volumeNBall(uint n) {
	return lanczosGamma(n/2 + 3.0/2.0) / (sqrt(PI) * lanczosGamma(n/2 + 1));
}

double volumeHypersphere(uint n, double radius) {
	return volumeNBall(n) * powNaive(radius, n);
}
