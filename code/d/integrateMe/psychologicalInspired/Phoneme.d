import std.algorithm.searching : canFind;

// http://www.theschoolrun.com/what-is-a-phoneme
const string[][] phonemeTable = [
	["s"], 
	["t"], 
	["p"], 
	["n"], 
	["m"], 
	["a"], 
	["e"], 
	["i"], 
	["o"], 
	["g"], 
	["d"], 
	["c", "k"], 
	["r"], 
	["h"],
	["u"],
	["ai"],
	["ee"],
	["igh"],
	["b"],
	["f"],
	["l"],
	["j"],
	["v"],
	["ao"],
	["oo"],
	// ["oo"],
	["ar"],
	["w"],
	["x"],
	["y"],
	["z"],
	["qu"],
	["or"],
	["ur"],
	["ow"],
	["oi"],
	["ch"],
	["sh"],
	["th"],
	//[th]
	["ng"],
	["ear"],
	["air"],
	["ure"],
	["er"],
];

import std.stdio;

// finds the longest substring of a letter sequence in the table and translates it to the indices into the table
int[] convertToPhonemeIndices(string text) {
	// score is length
	static struct HitWithScore {
		uint score;
		uint index;

		static HitWithScore make(uint index, uint score) {
			HitWithScore result;
			result.score = score;
			result.index = index;
			return result;
		}

		// length is the score
		final @property uint length() {
			return score;
		}
	}

	HitWithScore[] innerFnFindHits(size_t startIndex) {
		HitWithScore[] hits;

		foreach( tableIndex, iterationTableEntry; phonemeTable ) {
			foreach( iterationPhoneme; iterationTableEntry ) {
				size_t length = iterationPhoneme.length;
				if( text.length - startIndex < length ) {
					continue;
				}
				string subtext = text[startIndex..startIndex+length];

				if( subtext == iterationPhoneme ) {
					hits ~= HitWithScore.make(tableIndex, iterationPhoneme.length);
				}
			}
		}

		return hits;
	}

	static HitWithScore innerFnHighestHit(HitWithScore[] hits) {
		HitWithScore result = hits[0];
		foreach( iHit; hits ) {
			if( iHit.score > result.score ) {
				result = iHit;
			}
		}
		return result;
	}

	int[] resultPhonemes;

	size_t i = 0;
	for(;;) {
		assert( i <= text.length );
		if( i == text.length ) {
			writeln("EXIT");
			break;
		}

		HitWithScore[] hits = innerFnFindHits(i);
		if( hits.length == 0 ) {
			// TODO< throw something >
			return resultPhonemes;
		}
		
		HitWithScore highestHit = innerFnHighestHit(hits);
		i += highestHit.length;
		resultPhonemes ~= highestHit.index;
	}

	return resultPhonemes;
}

void main() {
	import std.stdio;
	writeln(convertToPhonemeIndices("queer"));
}
