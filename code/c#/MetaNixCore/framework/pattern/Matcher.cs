using System.Collections.Generic;
using System.Diagnostics;

namespace AiThisAndThat.patternMatching {
    public class MatchArguments<DecorationType> where DecorationType : IDecoration<DecorationType> {
	    public Pattern<DecorationType> templatePattern, matchingPattern;
	    public bool bidirectional;
    }

    public static class Matcher<DecorationType> where DecorationType : IDecoration<DecorationType> {
        // tries to match the 'matchingPattern' to the 'templatePattern'
        // 'matches' doesn't get reseted on entry, this simplifies iterated(recursive) matching
        public static bool match(MatchArguments<DecorationType> arguments, IDictionary</*variableId*/ulong, Pattern<DecorationType>> matches, out bool isSame) {
	        bool innerFnMatchVariable(Pattern<DecorationType> variable, Pattern<DecorationType> toMatch) {
		        // it is possible that the variable is already in the dictionary
		        // if this is the case compare the uniqueId to identify a missmatch

		        if(
                    matches.ContainsKey(variable.uniqueId) &&
			        !Pattern<DecorationType>.deepCompare(toMatch, matches[variable.uniqueId])
		        ) {
			        return false;
		        }
                
		        matches[variable.uniqueId] = toMatch;

		        return true;
	        }

	        isSame = arguments.templatePattern.uniqueId == arguments.matchingPattern.uniqueId; // OPTIMIZATION
	        
		    if( isSame ) {
			    return true;
		    }
		    else if( // OPTIMIZATION< we can do this because uniqueId == variableId
                isSame &&
                arguments.templatePattern.@is(Pattern<DecorationType>.EnumType.VARIABLE) &&
                arguments.matchingPattern.@is(Pattern<DecorationType>.EnumType.VARIABLE)
            ) { 

			    Debug.Assert(arguments.templatePattern.variableId == arguments.matchingPattern.variableId); // verify
			    return true;
		    }
	        

            if( arguments.templatePattern.@is(Pattern<DecorationType>.EnumType.DECORATEDVALUE) &&
                arguments.matchingPattern.@is(Pattern<DecorationType>.EnumType.DECORATEDVALUE)
            ) {

                bool isEqual = arguments.templatePattern.decoration.checkEqualValue(arguments.matchingPattern.decoration);
                return isEqual;
            }
		    else if(
			    arguments.templatePattern.@is(Pattern<DecorationType>.EnumType.SYMBOL) &&
			    arguments.matchingPattern.@is(Pattern<DecorationType>.EnumType.SYMBOL)
		    ) {

			    return Pattern<DecorationType>.isSameSymbol(arguments.templatePattern.symbol, arguments.matchingPattern.symbol);
		    }
		    else if( arguments.templatePattern.@is(Pattern<DecorationType>.EnumType.VARIABLE) ) {
			    return innerFnMatchVariable(arguments.templatePattern, arguments.matchingPattern);
		    }
		    // we match in the other direction too if the flag is set
		    else if( arguments.bidirectional && arguments.matchingPattern.@is(Pattern<DecorationType>.EnumType.VARIABLE) ) {
			    return innerFnMatchVariable(arguments.matchingPattern, arguments.templatePattern);
		    }
		    else if( arguments.templatePattern.isBranch && arguments.matchingPattern.isBranch ) { // recursivly match
			    if( arguments.templatePattern.referenced.Length != arguments.matchingPattern.referenced.Length ) { // OPTIMIZATION
				    return false;
			    }

			    int numberOfChildren = arguments.templatePattern.referenced.Length;
			    for( int i = 0; i < numberOfChildren; i++ ) {
				    bool calledIsSame;
				    MatchArguments<DecorationType> calledArguments = new MatchArguments<DecorationType>();
				    calledArguments.templatePattern = arguments.templatePattern.referenced[i];
				    calledArguments.matchingPattern = arguments.matchingPattern.referenced[i];
				    calledArguments.bidirectional = arguments.bidirectional;

				    if( !match(calledArguments, matches, out calledIsSame) ) {
					    return false;
				    }
			    }

			    return true;
		    }
		    else {
			    // if all possibilities failed we return a missmatch
			    return false;
		    }
	        
        }
    }
}
