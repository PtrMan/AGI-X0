using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using PatternWithDecoration = MetaNix.framework.pattern.Pattern<MetaNix.framework.pattern.Decoration>;
using MetaNix.framework.pattern.withDecoration;

namespace MetaNix.framework.pattern {
    
    public class Decoration : IDecoration<Decoration> {
	    public enum EnumType {
		    VARIABLETEMPLATEMATCHING, // list of templates which can be matched against variables
		    SEQUENCE, // sequences can be interpreted linearly
		    TUPLE2, // used for matching, pair of template and code (sequence) which will be executed if it matches
            EXEC, // used to execute a basic instruction
            STRING, // made out of integers
            VALUE, // 
            LOOP, // loops child Pattern indefinitly
	    }

        public object value; // must be null if it is not a value
        
	    public EnumType type;

        public Decoration deepCopy() {
            Decoration copied = new Decoration();
            copied.type = type;
            copied.value = value; // we don't deep copy the value for now
            return copied;
        }

        public bool checkEqualValue(Decoration other) {
            if( type != other.type )   return false;
            return value.Equals(other.value);
        }
    }

    
    public class InterpretationResult {
	    public enum EnumResultType {
		    OK,
		    INTERRUPTED, // interpretation was interrupted because of some mismatch, is not a fault
		    ERRORVARIABLENOTFOUND, // a variable was not found while matching, hard error
	    }

	    public EnumResultType resultType;

	    public bool isInterruptedOrError { get {
		    return resultType != EnumResultType.OK;
	    } }
    }

    public class  InterpretationContext {
	    public IDictionary</*UniqueIdType*/ulong, PatternWithDecoration> valueByVariable = new Dictionary</*UniqueIdType*/ulong, PatternWithDecoration>();

        public IDictionary<string, FunctionTypeDelegate> functionTable = new Dictionary<string, FunctionTypeDelegate>();

        // /param callerSite from where was the function called
        public delegate void FunctionTypeDelegate(InterpretationContext context, IList<PatternWithDecoration> arguments, PatternWithDecoration callerSite);

    }

    public class Interpreter {
        
        private static void interpretSequence(PatternWithDecoration patternWithDecoration, InterpretationContext context, InterpretationResult interpretationResult) {
	        Debug.Assert(patternWithDecoration.decoration.type == Decoration.EnumType.SEQUENCE); // just debug-assert because it has to be hardly checked by the dispatching logic

	        vmAssert(patternWithDecoration.isBranch, true, "Must be branch!");

	        for( int i = 0; i < patternWithDecoration.referenced.Length; i++ ) {
		        interpretationDispatch(patternWithDecoration.referenced[i], context, interpretationResult);
		        if( interpretationResult.isInterruptedOrError ) {
			        return;
		        }
	        }
        }

        private static void interpretVariableTemplateMatching(
            PatternWithDecoration patternWithDecoration,
            InterpretationContext context,
            InterpretationResult interpretationResult,
            bool multiMatch = false
        ) {
	        Debug.Assert(patternWithDecoration.decoration.type == Decoration.EnumType.VARIABLETEMPLATEMATCHING); // just debug-assert because it has to be hardly checked by the dispatching logic
	        
            vmAssert(patternWithDecoration.isBranch, true, "Must be branch!");
            vmAssert(patternWithDecoration.referenced.Length >= 1, false, "Must have a pattern to match");

	        PatternWithDecoration patterToMatch = patternWithDecoration.referenced[0];

            // we resolve the variable if it is a variable
            if( patterToMatch.@is(PatternWithDecoration.EnumType.VARIABLE) ) {
                ulong variableIdToResolve = patterToMatch.variableId;
                patterToMatch = context.valueByVariable[variableIdToResolve];
            }

	        for( int i = 1; i < patternWithDecoration.referenced.Length; i++ ) {
		        vmAssert(patternWithDecoration.referenced[i].decoration.type == Decoration.EnumType.TUPLE2, true, "Must be tuple2");
		        Debug.Assert(patternWithDecoration.referenced[i].type == PatternWithDecoration.EnumType.BRANCH); // tuples must be branches, just assert because else it is a bug in the creation of tuple2
		        Debug.Assert(patternWithDecoration.referenced[i].referenced.Length == 2); // just assert because else it is a bug in the creation of tuple2
		

		        PatternWithDecoration templatePattern = patternWithDecoration.referenced[i].referenced[0];
		        PatternWithDecoration actionPattern = patternWithDecoration.referenced[i].referenced[1];

		        MatchArguments<Decoration> arguments = new MatchArguments<Decoration>();
	

		        IDictionary</*UniqueIdType*/ulong, PatternWithDecoration> matches = new Dictionary</*UniqueIdType*/ulong, PatternWithDecoration>(context.valueByVariable);// let it know the current variable assignments
		        bool isSame;

		        arguments.bidirectional = false;
		        arguments.templatePattern = templatePattern;
		        arguments.matchingPattern = patterToMatch;
		        bool isMatching = Matcher<Decoration>.match(arguments, matches, out isSame);

		        if( !isMatching )  continue;
		        // we are here if it does match

		        // excute body
		        interpretationDispatch(actionPattern, context, interpretationResult);

                if( !multiMatch )   return; // we return because we don't need to match the other possibilities
	        }
        }

        static void interpretExecution(PatternWithDecoration patternWithDecoration, InterpretationContext context, InterpretationResult interpretationResult) {
            Trace.Assert(patternWithDecoration.decoration != null, "Must have decoration"); // hard Trace-assert because it is a deep bug if this is violated
            Debug.Assert(patternWithDecoration.decoration.type == Decoration.EnumType.EXEC);

            vmAssert(patternWithDecoration.isBranch, true, "Must be branch!");
            vmAssert(patternWithDecoration.referenced.Length >= 1, true, "Not sufficient many parameters!");
            
            Pattern<Decoration> calledName = patternWithDecoration.referenced[0];
            string calledNameAsString = StringHelper.convertPatternToString(calledName);

            int numberOfArguments = patternWithDecoration.referenced.Length - 1;
            Pattern<Decoration>[] arguments = patternWithDecoration.referenced.Skip(1).Take(numberOfArguments).ToArray(); // TODO OPTIMZIATION< might be to slow >

            // dispatch
            vmAssert(context.functionTable.ContainsKey(calledNameAsString), false, "Name " + calledNameAsString + " not found in function table!");
            context.functionTable[calledNameAsString](context, arguments, patternWithDecoration);
        }

        static void interpretLoop(PatternWithDecoration patternWithDecoration, InterpretationContext context, InterpretationResult interpretationResult) {
            vmAssert(patternWithDecoration.isBranch, true, "Must be branch!");
            vmAssert(patternWithDecoration.referenced.Length == 1, true, "Must have only one parameter");

            for(;;) {
                interpretationDispatch(patternWithDecoration.referenced[0], context, interpretationResult);

                if( interpretationResult.isInterruptedOrError ) {
			        return;
		        }
            }
        }
        
        
        
        public static void interpretationDispatch(PatternWithDecoration patternWithDecoration, InterpretationContext context, InterpretationResult interpretationResult) {
	        vmAssert(patternWithDecoration.decoration != null, true, "Must have decoration"); // hard Trace-assert because it is a deep bug if this is violated
            
            if( patternWithDecoration.decoration.type == Decoration.EnumType.SEQUENCE ) {
		        interpretSequence(patternWithDecoration, context, interpretationResult);
	        }
	        else if( patternWithDecoration.decoration.type == Decoration.EnumType.VARIABLETEMPLATEMATCHING ) {
		        interpretVariableTemplateMatching(patternWithDecoration, context, interpretationResult);
	        }
            else if( patternWithDecoration.decoration.type == Decoration.EnumType.EXEC ) {
                interpretExecution(patternWithDecoration, context, interpretationResult);
            }
            else if( patternWithDecoration.decoration.type == Decoration.EnumType.LOOP ) {
                interpretLoop(patternWithDecoration, context, interpretationResult);
            }
            else {
                vmAssert(false, true, "Invalid interpretation type");
            }
        }

        
        // check for value, used to reflect errors to the system itself so it can react to faulty code and or fix it 
        public static void vmAssert(bool value, bool critical, string humanReadableError) {
            // TODO< throw own exception >
            if( !value ) throw new Exception("VM Assert failed: " + humanReadableError);
        }


        // helper
        // TODO< move to helper class >
        public static long retriveLong(Pattern<Decoration> pattern) {
            Trace.Assert(pattern.decoration != null, "Must have decoration"); // hard Trace-assert because it is a deep bug if this is violated
            vmAssert(pattern.decoration.type == Decoration.EnumType.VALUE, false, "Must be value");
            vmAssert(pattern.decoration.value is long, false, "Must be long!");
            return (long)pattern.decoration.value;
        }
    }
}
