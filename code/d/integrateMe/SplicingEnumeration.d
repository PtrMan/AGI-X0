// generates all possible "splice" patterns
// we splice from 3 sources, from the 'a' instructions, from the 'b' instructions and from the instructions in the current code ('x')

/*
small example of the enumeration:

....
a...
aa..
aaa.
aaaa

	mask x...

	x...
	x...
	xa..
	xaa.
	xaaa

	mask xx..

	xx..
	xx..
	xx..
	xxa.
	xxaa

	mask xxx.

	xxx.
	xxx.
	xxx.
	xxx.
	xxxa

	

	mask .x..

	.x..
	ax..
	ax..
	axa.
	axaa	

	mask .xx.

	.xx.
	axx.
	axx.
	axx.
	axxa	

	mask .xxx

	.xxx
	axxx
	axxx
	axxx
	axxx	

	and so on

b...
b...
ba..
baa.
baaa

and so on...

bb..
bb..
bb..
bba.
bbaa

and so on...

*/

import std.algorithm.comparison : min;

struct EnumerateSplicing {
	static struct Pattern {
		int[] elements; // 0 : a, 1 : b, 2 : x, -1 : free 

		final void reset(uint width) {
			elements.length = width;
			fill(0, width, -1);
		}

		final void fill(uint startIndex, uint count, int value) {
			foreach(i;startIndex..min(startIndex+count,elements.length)) {
				elements[i] = value;
			}
		}
	}

	bool[Pattern] patternSet;

	Pattern[] enumerated;

	final void enumerate(uint width) {
		enumerateA(width);
	}

	private final void enumerateA(uint width) {
		foreach( currentWidth; 0..width+1 ) {
			Pattern aPattern;
			aPattern.reset(width);
			aPattern.fill(0, currentWidth, 0);

			enumerateB(aPattern, width);
		}
	}

	private final void enumerateB(Pattern inputPattern, uint width) {
		foreach( currentWidth; 0..width+1 ) {
			inputPattern.fill(0, currentWidth, 1);

			enumerateX(inputPattern, width);
		}
	}

	private final void enumerateX(Pattern inputPattern, uint width) {
		uint xWidth = 5; // width of x elements

		foreach( currentI; 0..width ) {
			foreach( currentWidth; 0..min(xWidth, width+1) ) {
				inputPattern.fill(0, currentWidth, 2);
				addToPatternsIfNew(inputPattern);
			}
		}
	}

	private final void addToPatternsIfNew(Pattern pattern) {
		if( !(pattern in patternSet) ) {
			patternSet[pattern] = true;
			enumerated ~= pattern;
		}
	}
}

void main() {
	EnumerateSplicing enumerate;
	enumerate.enumerate(14);

	import std.stdio;
	writeln("pattern #=", enumerate.enumerated.length);

}
