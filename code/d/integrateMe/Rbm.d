// see http://blog.echen.me/2011/07/18/introduction-to-restricted-boltzmann-machines/
// for tutorial of Restricted boltzmann machine

// x is actually bool[] but it's faster when we convert it just once
private float calcActivationEnergyIntern(float *weights, float *x, size_t length) {
	float sum = 0;
	foreach( i; 0..length ) {
		sum += (weights[i]*x[i]);
	}
	return sum;
}

private float calcActivationEnergy(float[] weights, float[] x) {
  assert(weights.length == x.length);
	return calcActivationEnergyIntern(&weights[0], &x[0], weights.length);
}



import std.math : exp;

private float logisticFunction(float x) {
	return 1.0f / (1.0f + exp(-x));
}

extern float rand();

/*
private bool calcActivation(float[] weights, bool[] x) {
	float energy = calcActivationEnergy(weights, x);
	float propability = logisticFunction(energy);
	return rand() < propability; 
}
*/


private void update(float[][] weights, float learningRate, bool[][] positive, bool[][] negative) {
	size_t iLength = weights.length;
	size_t jLength = weights[0].length;

	foreach( i; 0..iLength ) foreach( j; 0..jLength ) {
		weights[i][j] += (learningRate*(bool2Float(positive[i][j]) - negative[i][j]));
	}
}

private float bool2Float(bool x) {
	return x ? 1.0f : 0.0f;
}
