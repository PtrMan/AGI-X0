module cognitive.EpisodicBuffer;

import std.algorithm.sorting : sort;
import std.algorithm.iteration : filter;
import std.array : array;

import HtmLike;
import SemanticPointer;
import linopterixed.types.Bigint;


// after [workingmemoryEpisodicBuffer]

// * holds semantic pointers to temporal concepts
// * has limited capacity [workingmemoryEpisodicBuffer], so AIKR
// * binding is solved with temporal synchrony [mitencySync]

// is controled by the mysterious "central executive", which is in our system a mix of handcrafted and evolved/enumerated components

static float minSalienceTillRemoval = 0.1f;
static uint capacity = 5000;
static float salienceDecay = 0.5f;

static float salienceRefreshByFeatures = 1.2f;
static uint minimumFeatureOverlapForRefresh = 3;

struct EpisodicBuffer {
	EpisodicBufferElement*[] elements;

	// adds a slience to all elements which fire enough for the fuature encoding
	// usually called from WorkingMemoryModel
	final void refreshElementsWithFeatures(Bigint!WIDTHOFHTMLIKECELLIN64BITWORDS feature, float refreshSalience = salienceRefreshByFeatures) {
		foreach( iterationElement; elements ) {
			if( iterationElement.featureEncoding.numberOfMatchingBits(feature) >= minimumFeatureOverlapForRefresh ) {
				iterationElement.salience += refreshSalience;
			}
		}
	}

	final void addElement(EpisodicBufferElement* element) {
		elements ~= element;
	}

	private void tick() {
		//decay(); uncommented because decay is controlled by the "central executive"
		removeBelowMinSalience();
		limitElements();
	}



	/*  uncommented because decay is controlled by the "central executive"
	private final void decay() {
		foreach( ref t; elements ) {
			t.salience *= salienceDecay;
		}
	}
	*/


	private final void removeBelowMinSalience() {
		elements = elements.filter!(v => v.salience > minSalienceTillRemoval).array;
	}

	private final void limitElements() {
		if( elements.length > capacity ) {
			elements.sort!("a.salience > b.salience");
			elements.length = capacity;
		}
	}
}

struct EpisodicBufferElement {
	// we use a HTM like representation because we have to adress it somehow, we do this based on learned features.
	// The htm like cells are used for matching aginst some (learned) features
	HtmLikeCell!WIDTHOFHTMLIKECELLIN64BITWORDS featureEncoding;

	float salience; // used for throwing not so important elments out after AIKR

	SemanticPointer *semanticPointer; // semantic pointer into temporal-LTM 

	final void decay() {
		multiplySalience(salienceDecay);
	}

	final void multiplySalience(float mul) {
		salience *= mul;
	}

	final void addSalience(float add) {
		salience += add;
	}
}

static const uint WIDTHOFHTMLIKECELLIN64BITWORDS = 16;