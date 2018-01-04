

import std.stdio : writeln;
import std.string : splitLines;

void main() {
	// ripped out from https://github.com/opennars/opennars2/blob/2_0_1/src/nal/rules.clj
	string nal = """
		;;Equivalence and Implication Rules
          ;Similarity to Inheritance
          #R[(S --> P) (S <-> P) |- (S --> P) :post (:t/struct-int :p/belief) :pre (:question?)]
          ;Inheritance to Similarity
          #R[(S <-> P) (S --> P) |- (S <-> P) :post (:t/struct-abd :p/belief) :pre (:question?)]
	""";

	writeln(nal.splitLines());

	
}

