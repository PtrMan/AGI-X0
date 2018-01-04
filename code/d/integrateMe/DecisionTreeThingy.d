import linopterixed.LogarithmHelper;

struct DecisionTreeElement {
	double utilityLogarithm;
	double propabilityLogarithm;

	final @property double payoffLogarithm() {
		return utilityLogarithm;
	}

	DecisionTreeElement*[] children;
	DecisionTreeElement *parent; // can be null

	enum EnumType {
		ACTION,
		EVENT,
	}

	EnumType type;
}

// \param outNodeValues logarithmic
// \param outPropabilities logarithmic
private void collectNodeValueAndPropabilityOfChildren(DecisionTreeElement *treeElement, ref double[] outNodeValues, ref double[] outPropabilities, out size_t usedLength) {
	usedLength = treeElement.children.length;
	foreach( i, iterationChildren; treeElement.children ) {
		outNodeValues[i] = iterationChildren.payoffLogarithm;
		outPropabilities[i] = iterationChildren.propabilityLogarithm;
	}
}

private void mulPropsWithPayoff(double[] propabilities, double[] payoffs, ref double[] results) {
  foreach( i; 0..propabilities.length ) {
    results[i] = propabilities[i] + payoffs[i];
  }
}

void calcUtilityLogarithm(DecisionTreeElement *treeElement, double[] propabilities, double[] payoffs, ref double[] results) {
	size_t usedLength;
	collectNodeValueAndPropabilityOfChildren(treeElement, payoffs, propabilities, usedLength);
	mulPropsWithPayoff(propabilities, payoffs, results);

	treeElement.parent.utilityLogarithm = logarithmicSum(results[0..usedLength]);
}
