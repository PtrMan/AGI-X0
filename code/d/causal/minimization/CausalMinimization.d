module causal.minimization.CausalMinimization;

import std.array : array;
import std.algorithm.iteration : filter, map, sum;
import std.algorithm.searching : canFind;
import std.typecons : Tuple;
import std.exception : enforce;

// this algorithm tries to find valid insertion/swap operations and notes all done tries in a tree

// (1) find a potential element/neuron to swap or insert with another position, which is not jet in the tree
// (2) check if swap/insertion is possible/legal
// (3) do swap/insertion
// (4) calc energy
//     - if energy is lower then execute swap/insertion and create a new network/linearlization, flush tree, we have one with an lower energy
//     - if energy is equal add it to the tree as another possibility to do a swap
//     - if energy is higher add it to the tree as an reminder that the action is propably pointless

struct Decoration {
	uint overlapCounter; // used to find the elements of the overlaps
	int regionMarker; // used for marking elements in different regions, used for swapping elements
}

struct Element {
	Element*[] childrens;
	Element*[] up;

	size_t index;

	// decoration
	Decoration decoration;
}

private void resetOverlapCounters(Element*[] elements) {
	foreach( iterationElement; elements ) {
		iterationElement.decoration.overlapCounter = 0;
	}
}

// increments the overlap counter
private void incrementOverlapCounters(Element*[] elements) {
	foreach( iterationElement; elements ) {
		iterationElement.decoration.overlapCounter++;
	}
}


// elements from the sequence partioned into three groups
struct Replay {
	Element*[][3] elementsPartioned;

	final @property Element*[] elementsBeforeA() {
		return elementsPartioned[0];
	}

	final @property Element*[] elementsBetweenAAndB() {
		return elementsPartioned[1];
	}

	final @property Element*[] elementsAfterB() {
		return elementsPartioned[2];
	}
}



struct Linearized {
	Element*[] linearized;

	static Linearized *makePtr(Element*[] linearized) {
		Linearized *result = new Linearized;
		result.linearized = linearized;
		return result;
	}

	final void refreshIndices() {
		foreach( i, ie; linearized ) {
			ie.index = i;
		}
	}

	final double calcEnergy() {
		return linearized.map!(v => calcEnergyOfElement(v)).sum;
	}

	final Linearized *deepCopy() {
		Linearized *result = new Linearized;

		// copy
		Element*[Element*] copiedElements;
		foreach( iterationElement; linearized ) {
			copiedElements[iterationElement] = new Element;
			copiedElements[iterationElement].index = iterationElement.index;
			copiedElements[iterationElement].decoration = iterationElement.decoration;
		}

		result.linearized = cast(Element*[])copiedElements.values;

		// map elements to copied elements
		foreach( i, iterationElement; linearized ) {
			Element* resultElement = result.linearized[i];

			foreach( iterationChildren; iterationElement.childrens ) {
				enforce(iterationChildren in copiedElements); // because else the element is not in the linearized, which is not good and allowed
				resultElement.childrens ~= copiedElements[iterationChildren];
			}

			foreach( iterationUp; iterationElement.up ) {
				enforce(iterationUp in copiedElements); // because else the element is not in the linearized, which is not good and allowed
				resultElement.up ~= copiedElements[iterationUp];
			}
		}

		return result;
	}

	private static double calcEnergyOfElement(Element *element) {
		double result = 0;
		foreach( iChildren; element.childrens ) {
			int diff = cast(int)iChildren.index - cast(int)element.index;
			assert( diff >= 0 );
			result += diff;
		}
		return result;
	}


}

// returns the linearized form
/* uncommented because outdated, because the algorithm to find the children is slightly wrong 
private Linearized findRandomSwapPairAndDoSwap(Element*[] elements, Replay *replay) {
	static Element*[] innerFnFilterByOverlapCounter(Element[] *arr, uint comparisionOverlapCounter) {
		return bChildrenSet.filter(v => v.decoration.overlapCounter == comparisionOverlapCounter).array;
	}

	Tuple!(Element*, Element*) swapPair = getRandomSwapPair(elements);
	Element *a = swapPair[0];
	Element *b = swapPair[1];

	Element*[] aChildrenSet = calcSetOfChildrenInclusive(a);
	Element*[] bChildrenSet = calcSetOfChildrenInclusive(b);
	
	// figure out which elements overlap
	// if elements overlap we can't just add them to the result array (twice)
	incrementOverlapCounters(aChildrenSet);
	incrementOverlapCounters(bChildrenSet);


	// build the linearized version
	Element*[] resultLinearized;
	resultLinearized ~= replay.elementsBeforeA();
	resultLinearized ~= innerFnFilterByOverlapCounter(bChildrenSet, 1);
	resultLinearized ~= replay.elementsBetweenAAndB();
	resultLinearized ~= innerFnFilterByOverlapCounter(aChildrenSet, 1);
	resultLinearized ~= innerFnFilterByOverlapCounter(aChildrenSet, 2); // add overlap
	resultLinearized ~= replay.elementsAfterB();
	return Linearized.make(resultLinearized);
}
*/


// a swap is possible if
// (a) the two elements do have a common "up" element
// if elements are swapped their childrens have to be swapped too

// returns a random 
// throws something if no tuple can be found, should not happen because the dag should be tested for this condition before the algorithm is invoked
private Tuple!(Element*, Element*) getRandomSwapPair(Element*[] elements) {
	Element*[] possibleElements = elements.dup;

	for(;;) {
		if( possibleElements.length == 0 ) {
			// should never happen
			throw new Exception("internal error");
		}

		Element *candidateRoot = removeAndReturnRandom(possibleElements);

		Element*[] setOfChildrens = calcSetOfChildren(candidateRoot);
		if( setOfChildrens.length < 2 ) {
			continue;
		}

		Element *a = removeAndReturnRandom(setOfChildrens);
		Element *b = removeAndReturnRandom(setOfChildrens);
		return Tuple!(Element*, Element*)(a, b);
	}
}

private Element*[] calcSetOfChildrenInclusive(Element *element) {
	return calcSetOfReachableElements(element, [], false);
}

private Element*[] calcSetOfChildren(Element *element) {
	return calcSetOfReachableElements(element, [], false, false);
}


// functions for calculating the children and elements which point at the children, all guarded by the guardElement
// means that the connections aren't tracked bejond the guard element
private Element*[] calcSetOfReachableElements(Element *element, Element*[] guardElements, bool checkUp = true, bool inclusive = true) {
	bool[Element*] set;
	calcSetOfReachableElementsIntern(element, set, guardElements, checkUp, inclusive);
	return cast(Element*[])set.keys;
}

private void calcSetOfReachableElementsIntern(Element *element, ref bool[Element*] set, Element*[] guardElements, bool checkUp = true, bool inclusive = true) {
	if( guardElements.canFind(element) ) {
		return;
	}

	if( inclusive ) {
		set[element] = true;
	}

	foreach( iteractionChildren; element.childrens ) {
		calcSetOfReachableElementsIntern(iteractionChildren, set, guardElements);
	}

	if( checkUp ) {
		foreach( iteractionChildren; element.up ) {
			calcSetOfReachableElementsIntern(iteractionChildren, set, guardElements);
		}
	}
}


// region helpers

private void regionMarkerReset(Element*[] elements) {
	regionMarkerSet(elements, -1);
}

private void regionMarkerSet(Element*[] elements, int regionMarker) {
	foreach( iterationElement; elements ) {
		iterationElement.decoration.regionMarker = regionMarker;
	}
}






import std.algorithm.mutation : remove;

Type removeAndReturn(Type)(ref Type[] arr, size_t index) {
	Type result = arr[index];
	arr = arr.remove(index);
	return result;
}

Type removeAndReturnRandom(Type)(ref Type[] arr) {
	return removeAndReturn(arr, rand(arr.length));
}

// dummy
extern size_t rand(size_t);



struct Range {
	uint[] sortedIndices;

	static Range makeFromUnsorted(uint[] unsortedIndices) {
		Range result;
		result.sortedIndices = unsortedIndices.sort;
		return result;
	}
}

struct Action {
	enum EnumType {
		NONE, // only allowed for the ROOT
		SWAP,
		//INSERT//  commented because not jet supported
	}
	EnumType type;

	Range swapPartA, swapPartB;
}

struct TreeElement {
	Linearized *linearized;

	final @property double energy() {
		return linearized.calcEnergy();
	}

	Action action; // action done to the parent

	static TreeElement *makeRoot(Linearized *linearized) {
		TreeElement *result = new TreeElement;
		result.linearized = linearized;
		result.action.type = Action.EnumType.NONE;
		return result;
	}

	static TreeElement *make(Linearized *linearized, Action action) {
		TreeElement *result = new TreeElement;
		result.linearized = linearized;
		result.action = action;
		return result;
	}

	TreeElement*[] children;
}

struct Tree {
	TreeElement *root;
}

private uint[] getIndices(Element*[] elements) {
	return elements.map!(v => v.index).array;
}

struct Algorithm {
	/* Tree in which we store all done actions to avoid to do the same action multiple times.
	 * another purpose is to estimate the best actions, which are actions which lead to the same energy.
	 */
	Tree tree;

	private final void resetTree(TreeElement *newRoot) {
		tree.root = newRoot;
	}

	private final void resetTreeBecauseEnergyWasMinimized(Linearized *linearized) {
		// TODO< print debug message >
		resetTree(TreeElement.makeRoot(linearized));
	}



	final void iteration(TreeElement *parentTreeElement) {
		Linearized *workingLinearized = parentTreeElement.linearized.deepCopy();

		workingLinearized.refreshIndices(); // * because in the last iteration we have overwritten them, this is an inefficiency

		// * select elements to switch, mark and store into Replay
		Element*[] elementsToSwapFromA;
		Element*[] elementsToSwapFromB;
		{
			Tuple!(Element*, Element*) tupleOfElementsToSwitch = getRandomSwapPair(workingLinearized.linearized);
			Element* elementA = tupleOfElementsToSwitch[0];
			Element* elementB = tupleOfElementsToSwitch[1];
			elementsToSwapFromA = calcSetOfReachableElements(elementA, [elementA, elementB]);
			elementsToSwapFromB = calcSetOfReachableElements(elementB, [elementA, elementB]);
		}

		// - store the done action
		Action doneAction;
		doneAction.swapPartA = Range.makeFromUnsorted(getIndices(elementsToSwapFromA));
		doneAction.swapPartB = Range.makeFromUnsorted(getIndices(elementsToSwapFromB));
		doneAction.type = Action.EnumType.SWAP;

		// * do switch, linearlize
		regionMarkerReset(workingLinearized.linearized);
		regionMarkerSet(elementsToSwapFromA, 1);
		regionMarkerSet(elementsToSwapFromB, 2);

		Replay *replay = createReplay(workingLinearized);
		Linearized *candidateSolution = doSwap(workingLinearized.linearized, replay, elementsToSwapFromA, elementsToSwapFromB);

		// * calc energy
		double energyOfCandidateSolution = candidateSolution.calcEnergy();
		
		// * compare energy with energy of tree, if energy is lower we have a new root and throw old tree away
		//    - if energy is equal or higher we store it in the tree
		if( candidateSolution.calcEnergy() < parentTreeElement.energy ) {
			resetTreeBecauseEnergyWasMinimized(candidateSolution);
		}
		else { // if energy is equal or greater
			parentTreeElement.children ~= TreeElement.make(candidateSolution, doneAction);
		}
	}

	private static Replay *createReplay(Linearized *workingLinearized) {
		enum EnumMode {
			BEFOREA,
			INA,
			AFTERABEFOREB,
			INB,
			AFTERB,
		}

		/* uncommented because dead code
		static void innerFnCheck(Linearized *workingLinearized) {
			with(EnumMode) {
				EnumMode mode = BEFOREA;

				foreach( ie; workingLinearized.linearized ) {
					if( mode == BEFOREA ) {
						if( ie.decoration.regionMarker == -1 ) {
						}
						else if( ie.decoration.regionMarker == 1 ) {
							mode = INA;
						}
						else if( ie.decoration.regionMarker == 2 ) {
							mode = INB;
						}
					}
					else if( mode == INA ) {
						if( ie.decoration.regionMarker == -1 ) {
							mode = AFTERABEFOREB;
						}
						else if( ie.decoration.regionMarker == 1 ) {
							// stay in mode
						}
						else if( ie.decoration.regionMarker == 2 ) {
							mode = INB;
						}
					}
					else if( mode == AFTERABEFOREB ) {
						if( ie.decoration.regionMarker == -1 ) {
							// stay in the mode
						}
						else if( ie.decoration.regionMarker == 1 ) {
							throw new Exception("");
						}
						else if( ie.decoration.regionMarker == 2 ) {
							mode = INB;
						}
					}
					else if( mode == INB ) {
						if( ie.decoration.regionMarker == -1 ) {
							mode = AFTERB;
						}
						else if( ie.decoration.regionMarker == 1 ) {
							throw new Exception("");
						}
						else if( ie.decoration.regionMarker == 2 ) {
							// stay in the mode
						}
					}
					else if( mode == AFTERB ) {
						if( ie.decoration.regionMarker != -1 ) {
							throw new Exception("");
						}
					}
				}
			}
		}
		*/

		Replay *innerFnMakeReplayAndCheck() {
			Replay *resultReplay = new Replay;

			Element*[][3] elementsPartioned;

			with(EnumMode) {
				EnumMode mode = BEFOREA;

				foreach( ie; workingLinearized.linearized ) {
					if( mode == BEFOREA ) {
						if( ie.decoration.regionMarker == -1 ) {
						}
						else if( ie.decoration.regionMarker == 1 ) {
							mode = INA;
						}
						else if( ie.decoration.regionMarker == 2 ) {
							mode = INB;
						}
					}
					else if( mode == INA ) {
						if( ie.decoration.regionMarker == -1 ) {
							mode = AFTERABEFOREB;
						}
						else if( ie.decoration.regionMarker == 1 ) {
							// stay in mode
						}
						else if( ie.decoration.regionMarker == 2 ) {
							mode = INB;
						}
					}
					else if( mode == AFTERABEFOREB ) {
						if( ie.decoration.regionMarker == -1 ) {
							// stay in the mode
						}
						else if( ie.decoration.regionMarker == 1 ) {
							throw new Exception("");
						}
						else if( ie.decoration.regionMarker == 2 ) {
							mode = INB;
						}
					}
					else if( mode == INB ) {
						if( ie.decoration.regionMarker == -1 ) {
							mode = AFTERB;
						}
						else if( ie.decoration.regionMarker == 1 ) {
							throw new Exception("");
						}
						else if( ie.decoration.regionMarker == 2 ) {
							// stay in the mode
						}
					}
					else if( mode == AFTERB ) {
						if( ie.decoration.regionMarker != -1 ) {
							throw new Exception("");
						}
					}

					// add to result
					if( mode == BEFOREA ) {
						elementsPartioned[0] ~= ie;
					}
					else if( mode == AFTERABEFOREB ) {
						elementsPartioned[1] ~= ie;
					}
					else if( mode == AFTERB ) {
						elementsPartioned[2] ~= ie;
					}
				}
			}

			return resultReplay;
		}

		// before creation we have to check if the order is xxxxxxAxxxBxxxx
		//innerFnCheck(workingLinearized);
		return innerFnMakeReplayAndCheck();
	}

	// TODO< calculate correct order of A and B with the other algorithm(s)
	private static Linearized *doSwap(Element*[] elements, Replay *replay, Element*[] elementsOfA, Element*[] elementsOfB) {
		static Element*[] innerFnFilterByOverlapCounter(Element*[] arr, uint comparisionOverlapCounter) {
			return arr.filter!(v => v.decoration.overlapCounter == comparisionOverlapCounter).array;
		}
		
		// figure out which elements overlap
		// if elements overlap we can't just add them to the result array (twice)
		incrementOverlapCounters(elementsOfA);
		incrementOverlapCounters(elementsOfB);


		// build the linearized version
		Element*[] resultLinearized;
		resultLinearized ~= replay.elementsBeforeA();
		resultLinearized ~= innerFnFilterByOverlapCounter(elementsOfB, 1);
		resultLinearized ~= replay.elementsBetweenAAndB();
		resultLinearized ~= innerFnFilterByOverlapCounter(elementsOfA, 1);
		resultLinearized ~= innerFnFilterByOverlapCounter(elementsOfA, 2); // add overlap
		resultLinearized ~= replay.elementsAfterB();
		return Linearized.makePtr(resultLinearized);
	}
}
