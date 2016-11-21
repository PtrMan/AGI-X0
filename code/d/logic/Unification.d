// implementation of unification algorithm after robinson
// algorithm is described in book "Logik for Informatiker" page ~132

// for "interactive" testing
void main() {
	import std.stdio;

	// example from the book "Logik für Informatiker"
	bool unifies;
	Element[] elements = [
		new Formula(-1, [  new Formula(0, [new Formula(1, [new Variable(2), new Variable(3)])]),  new Formula(1, [new Variable(1), new Formula(1, [new Terminal(0), new Terminal(1)])]) ]),
		new Formula(-1, [  new Formula(0, [new Variable(4)]),  new Formula(1, [new Formula(2, [new Terminal(0), new Variable(5)]), new Formula(1, [new Variable(2), new Variable(3)])])  ])
	];
	
	Substitution resultSubstitution = unification(ElementSet.make(elements), /*out*/unifies);
	writeln("unifies=", unifies);

	foreach( substitutionIterationElement; resultSubstitution.subsitutionElements ) {
		writeln(substitutionIterationElement[0].toString() ~ " <- " ~ substitutionIterationElement[1].toString());
	}
}









unittest {
	size_t[] position;
	Formula element = new Formula(0, [new Variable(0),new Variable(0)]);
	increment(position, element);
	assert(position == [0]);
	increment(position, element);
	assert(position == [1]);
}

unittest {
	size_t[] position;
	Formula element = new Formula(0, [new Formula(0, [new Variable(0)]),new Variable(0)]);
	increment(position, element);
	assert(position == [0]);
	increment(position, element);
	assert(position == [0, 0]);
	increment(position, element);
	
	assert(position == [1]);
}

private void increment(ref size_t[] position, Element element) {
	if( position.length == 0 ) {
		enforce(element.isFormula);
		position = [0];
		return;
	}

	if( elementAt(element, position).isFormula ) {
		position ~= 0;
		return;
	}

	while( position.length > 0 ) {
		position[$-1]++;

		Element rootElement;
		if( position.length == 1 ) {
			rootElement = element;
		}
		else {
			rootElement = elementAt(element, position[0..$-1]);
		}
		enforce(rootElement.isFormula);
		Formula rootElementAsFormula = cast(Formula)rootElement;

		enforce(position[$-1] <= rootElementAsFormula.children.length);
		
		if( rootElementAsFormula.children.length == position[$-1] ) {
			position = position[0..$-1]; // remove last element
			continue;
		}

		break;
	}
}

// TODO< check indices >
private Element elementAt(Element root, size_t[] position) {
	if( position.length == 0 ) {
		return root;
	}
	else {
		// enforce because it's fast
		enforce(root.isFormula);
		Formula rootAsFormula = cast(Formula)root;
		size_t firstPosition = position[0];
		return elementAt(rootAsFormula.children[firstPosition], position[1..$]);
	}
}


unittest {
	assert(isAtEnd(new Variable(0), []));
}

unittest {
	size_t[] position;
	Formula element = new Formula(0, [new Formula(0, [new Variable(0)]),new Variable(0)]);
	assert(!isAtEnd(element, []));
	assert(!isAtEnd(element, [0]));
	assert(!isAtEnd(element, [0, 0]));
	assert(isAtEnd(element, [1]));
}

unittest {
	assert(isAtEnd(new Formula(0, []), [])); // special case
	assert(!isAtEnd(new Formula(0, [new Formula(1, []), new Formula(1, [])]), [0]));
	assert(isAtEnd(new Formula(0, [new Formula(1, []), new Formula(1, [])]), [1]));
}

unittest {
	Formula testFormula = new Formula(-1, [  new Formula(0, [new Formula(1, [new Variable(2), new Variable(3)])]),  new Formula(1, [new Variable(1), new Formula(1, [new Terminal(0), new Terminal(1)])]) ]);

	assert(!isAtEnd(testFormula, [0,0,1]));
	assert(!isAtEnd(testFormula, [1]));
}

// checks if it is at the last position bottom up
private bool isAtEnd(Element element, size_t[] currentPosition) {
	if( !element.isFormula ) {
		enforce(currentPosition.length == 0);
		return true;
	}

	// it is a formula
	Formula elementAsFormula = cast(Formula)element;

	if( elementAsFormula.children.length == 0 && currentPosition.length == 0 ) { // special case
		return true;
	}

	if( currentPosition.length == 0 ) { // points directly at the function
		return false;
	}

	size_t firstCurrentPosition = currentPosition[0];

	enforce( firstCurrentPosition <= elementAsFormula.children.length-1 );
	if( firstCurrentPosition == elementAsFormula.children.length-1 ) {
		return isAtEnd(elementAsFormula.children[firstCurrentPosition], currentPosition[1..$]);
	}

	return false;
}



// naive list implementation of set, works for unification because it doesn't have a lot of elements
struct ElementSet {
	private Element[] set;

	final bool contains(Element value) {
		foreach( iterationElement; set ) {
			if( value.equalsRecursivly(iterationElement) ) {
				return true;
			}
		}

		return false;
	}

	final size_t size() {
		return set.length;
	}

	final Element[] asList() {
		return set;
	}

	// elements can contain duplicates
	static ElementSet make(Element[] elements) {
		Element[] inSet;

		foreach( iterationElement; elements ) {
			if( !existsElementInElements(iterationElement, inSet) ) {
				inSet ~= iterationElement;
			}
		}

		ElementSet result;
		result.set = inSet;
		return result;
	}

	private static bool existsElementInElements(Element element, Element[] elements) {
		foreach( iterationElement; elements ) {
			if( element.equalsRecursivly(iterationElement) ) {
				return true;
			}
		}

		return false;
	}
}


import std.exception : enforce;
import std.typecons : Tuple;
import std.array : array;
import std.algorithm.iteration : map;
import std.string : join;

abstract class Element {
	enum EnumType {
		VARIABLE,
		FORMULA,
		TERMINAL,
	}

	EnumType type;

	final @property bool isVariable() {
		return type == EnumType.VARIABLE;
	}
	final @property bool isFormula() {
		return type == EnumType.FORMULA;
	}
	final @property bool isTerminal() {
		return type == EnumType.TERMINAL;
	}

	abstract Element deepCopy();
	abstract bool equalsRecursivly(Element other);
	abstract bool equalsFirst(Element other);
	abstract string toString();

	final this(EnumType type) {
		this.type = type;
	}
}

class Variable : Element {
	final this(int id) {
		super(Element.EnumType.VARIABLE);
		this.id = id;
	}

	override Element deepCopy() {
		return new Variable(id);
	}

	override bool equalsRecursivly(Element other) {
		return equalsInternal(other);
	}

	override bool equalsFirst(Element other) {
		return equalsInternal(other);
	}

	private final bool equalsInternal(Element other) {
		if( type != other.type ) {
			return false;
		}
		
		Variable otherAsVariable = cast(Variable)other;
		return id == otherAsVariable.id;
	}



	override string toString() {
		import std.conv : to;
		return "x" ~ to!string(id);
	}

	int id;
}

class Formula : Element {
	final this(int name, Element[] children) {
		super(Element.EnumType.FORMULA);
		this.children = children;
		this.name = name;
	}

	override Element deepCopy() {
		return new Formula(name, children.map!(v => v.deepCopy()).array);
	}

	override bool equalsRecursivly(Element other) {
		if( type != other.type ) {
			return false;
		}
		
		Formula otherAsFormula = cast(Formula)other;

		if( name != otherAsFormula.name ) {
			return false;
		}

		foreach( i; 0..children.length ) {
			if( !children[i].equalsRecursivly(otherAsFormula.children[i]) ) {
				return false;
			}
		}

		return true;
	}

	override bool equalsFirst(Element other) {
		if( type != other.type ) {
			return false;
		}
		
		Formula otherAsFormula = cast(Formula)other;

		if( name != otherAsFormula.name ) {
			return false;
		}

		return true;
	}


	override string toString() {
		import std.format : format;
		return "f%s(%s)".format(name, children.map!(v => v.toString()).join(","));
	}

	Element[] children;
	int name;
}

class Terminal : Element {
	final this(int value) {
		super(Element.EnumType.TERMINAL);
		this.value = value;
	}

	override Element deepCopy() {
		return new Terminal(value);
	}

	override bool equalsRecursivly(Element other) {
		return equalsInternal(other);
	}

	override bool equalsFirst(Element other) {
		return equalsInternal(other);
	}

	private final bool equalsInternal(Element other) {
		if( type != other.type ) {
			return false;
		}
		
		Terminal otherAsTerminal = cast(Terminal)other;
		return value == otherAsTerminal.value;
	}

	override string toString() {
		import std.conv : to;
		return to!string(value);
	}


	int value;
}


Substitution unification(ElementSet M, out bool unifies) {
	return unificationRobinson(M, /*out*/unifies);
}

struct Substitution {
	Tuple!(Variable, Element)[] subsitutionElements;

	bool isIdentity;

	static Substitution makeIdentity() {
		Substitution result;
		result.isIdentity = true;
		return result;
	}

	static Substitution makeNonIdentity() {
		Substitution result;
		result.isIdentity = true;
		return result;	
	}

	final Substitution opBinary(string op)(Substitution rhs) {
	    static if (op == "~") {
	    	Substitution result;
	    	result.subsitutionElements = subsitutionElements ~ rhs.subsitutionElements;
	    	return result;
	    }
    	else static assert(0, "Operator "~op~" not implemented");
	}

	final Substitution opOpAssign(string op)(Substitution rhs) {
	    static if (op == "~") {
	    	subsitutionElements ~= rhs.subsitutionElements;
	    	return this;
	    }
    	else static assert(0, "Operator "~op~" not implemented");
	}

	final bool existsVariable(Variable variable) {
		foreach( iterationSubsitutionElement; subsitutionElements ) {
			if( iterationSubsitutionElement[0].equalsRecursivly(variable) ) {
				return true;
			}
		}
		return false;
	}

	final void addWithoutSetCheck(Variable variable, Element element) {
		subsitutionElements ~= Tuple!(Variable, Element)(variable, element);
	}

	// applies subsitution to elements
	final void apply(ref ElementSet elements) {
		static void innerFnApplySubstitutionToElementRecursive(Tuple!(Variable, Element) substitution, ref Element element) {
			if( element.isVariable ) {
				if( element.equalsRecursivly(substitution[0]) ) {
					element = substitution[1];
				}
			}
			else if( element.isTerminal ) {
				return; // do nothing
			}
			else if( element.isFormula ) {
				Formula elementAsFormula = cast(Formula)element;

				foreach( ref iterationChildren; elementAsFormula.children ) {
					innerFnApplySubstitutionToElementRecursive(substitution, iterationChildren);
				}
			}
			else {
				// must never happen because it indicates a unhandled type
				throw new Exception("Internal error");
			}
		}

		Element[] newSetElements;

		foreach( element; elements.asList ) {
			Element workingElement = element;

			foreach( iterationSubstitution; subsitutionElements ) {
				innerFnApplySubstitutionToElementRecursive(iterationSubstitution, workingElement);
			}

			newSetElements ~= workingElement;
		}

		elements = ElementSet.make(newSetElements);
	}
}

private Substitution unificationRobinson(ElementSet M, out bool unifies) {
	unifies = false;

	Substitution substitution = Substitution.makeIdentity();

	size_t[] firstPositionWhereDifference;

	// returns the substitution if it exists
	bool innerFnExistsCorrectingUnificationForCurrentPosition(out Substitution substitutionForCorrection) {
		substitutionForCorrection = Substitution.makeNonIdentity();

		// \param element is not a variable
		void innerFnAddUnificationIfPossible(Variable variable, Element element, out bool exists) {
			exists = false;

			if( substitutionForCorrection.existsVariable(variable) ) { // if the variable already exists for an substitution we can't substitute again
				exists = true;
				return;
			}
			substitutionForCorrection.addWithoutSetCheck(variable, element);
		}

		Element[] MselectedElements = M.asList.map!(v => elementAt(v, firstPositionWhereDifference)).array;

		// iterate over all elements and check if it could substitute/unify with the other elements
		foreach( i, MselectedElementsIteration; MselectedElements ) {
			foreach( j, otherMselectedElementsIteration; MselectedElements ) {
				if( i <= j ) {
					continue;
				}

				// case if both are no variables and of the same type
				if( !MselectedElementsIteration.isVariable && !otherMselectedElementsIteration.isVariable && MselectedElementsIteration.type == otherMselectedElementsIteration.type ) {
					if( !MselectedElementsIteration.equalsRecursivly(otherMselectedElementsIteration) ) {
						return false;
					}
				}
				else if( MselectedElementsIteration.isVariable && !otherMselectedElementsIteration.isVariable ) {
					bool exists;
					innerFnAddUnificationIfPossible(cast(Variable)MselectedElementsIteration, otherMselectedElementsIteration, /*out*/exists);
					if( exists ) {
						return false;
					}
				}
				else if( !MselectedElementsIteration.isVariable && otherMselectedElementsIteration.isVariable ) {
					bool exists;
					innerFnAddUnificationIfPossible(cast(Variable)otherMselectedElementsIteration, MselectedElementsIteration, /*out*/exists);
					if( exists ) {
						return false;
					}
				}
				else if( MselectedElementsIteration.isVariable && otherMselectedElementsIteration.isVariable ) { // case if both are variables
					if( !MselectedElementsIteration.equalsRecursivly(otherMselectedElementsIteration) ) {
						return false;
					}
				}
				else { // in this case both sides are not variables and of different types, can't unify
					return false;
				}
			}
		}

		return true;
	}

	for(;;) {
		enforce(M.size >= 1);
		if( M.size == 1 ) {
			unifies = true;
			return substitution;
		}

		bool differenceFound;
		firstPositionWhereDifference = findFirstPositionWhereElementsAreDifferent(M.asList, /*out*/differenceFound);
		enforce(differenceFound); // hard enforce, must not be assert

		Substitution substitutionForCorrection;
		bool existsCorrectingUnification = innerFnExistsCorrectingUnificationForCurrentPosition(/*out*/substitutionForCorrection);
		if( !existsCorrectingUnification ) {
			unifies = false;
			substitution.isIdentity = false;
			return substitution;
		}

		// apply substitution to M and append to substitution
		substitutionForCorrection.apply(M);
		substitution ~= substitutionForCorrection;
	}
}


unittest {
	Element[] elements = [
		new Formula(-1, [  new Formula(0, [new Formula(1, [new Variable(2), new Variable(3)])]),  new Formula(1, [new Variable(1), new Formula(1, [new Terminal(0), new Terminal(1)])]) ]),
		new Formula(-1, [  new Formula(0, [new Formula(1, [new Variable(2), new Variable(3)])]),  new Formula(1, [new Formula(2, [new Terminal(0), new Variable(5)]), new Formula(1, [new Variable(2), new Variable(3)])])  ])
	];
	bool found;
	size_t[] resultPosition = findFirstPositionWhereElementsAreDifferent(elements, found);
	assert(found);
	assert(resultPosition == [1, 0]);
}

private size_t[] findFirstPositionWhereElementsAreDifferent(Element[] elements, out bool found) {
	// make a deep copy of all elements because we manipulate them
	return findFirstPositionWhereElementsAreDifferentInternal(elements.map!(v => v.deepCopy()).array, /*out*/found);
}

private size_t[] findFirstPositionWhereElementsAreDifferentInternal(Element[] formulas, out bool found) {
	static bool innerFnRecursivelyEqualFirstAt(Element a, Element b, size_t[] position) {
		return elementAt(a, position).equalsFirst( elementAt(b, position) );
	}

	bool innerFnAllRecursivlyEqualFirstAt(size_t[] position) {
		foreach( i; 0..formulas.length-1 ) {
			Element a = formulas[i];
			Element b = formulas[i+1];
			if( !innerFnRecursivelyEqualFirstAt(a, b, position) ) {
				return false;
			}
		}

		return true;
	}

	found = false;

	size_t[] currentPosition = [];

	for(;;) {
		// we check for the end after checking for a possible difference because the difference could be at the end

		if( innerFnAllRecursivlyEqualFirstAt(currentPosition) ) {
			// do nothing
		}
		else {
			found = true;
			return currentPosition;
		}

		if( isAtEnd(formulas[0], currentPosition) ) {
			return [];
		}

		increment(currentPosition, formulas[0]); // we only use the first formula to increment the position because the formulas are up to the current position identical
	}
}



// helper
private Type[] exceptLast(Type)(Type[] arr) {
	return arr[0..$-1];
}

unittest {
	assert(exceptLast([0, 1]) == [0]);
}

private Type[] exceptFirst(Type)(Type[] arr) {
	return arr[1..$];
}

unittest {
	assert(exceptFirst([0, 1]) == [1]);
}
