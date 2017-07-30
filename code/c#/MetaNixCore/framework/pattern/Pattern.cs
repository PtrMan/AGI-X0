using System;
using System.Diagnostics;

namespace AiThisAndThat.patternMatching {
    public class Pattern<DecorationType> where DecorationType : IDecoration<DecorationType> {
	    public DecorationType decoration;

	    public enum EnumType {
            DECORATEDVALUE,
		    SYMBOL,
		    VARIABLE,

		    BRANCH, // a branch in the tree
	    }

	    public EnumType type;

        public Pattern<DecorationType> parent; // can be null if it is the root

	    //union {
		public ulong symbol;
		public Pattern<DecorationType>[] referenced;
	    //}

        public Pattern<DecorationType> deepCopy() {
            Pattern<DecorationType> copied = new Pattern<DecorationType>();
            copied.decoration = decoration.deepCopy();
            copied.type = type;
            // we don't set the parent of copied because it is not valid, is has to be set by the caller
            copied.symbol = symbol;
            if( referenced != null ) {
                copied.referenced = new Pattern<DecorationType>[referenced.Length];
                for( int i = 0; i < referenced.Length; i++ ) {
                    copied.referenced[i] = referenced[i].deepCopy();
                }
            }

            return copied;
        }

        // does deep compare which may be (to) expensive
        public static bool deepCompare(Pattern<DecorationType> rhs, Pattern<DecorationType> lhs) {
            // optimization
            if( rhs.uniqueId == lhs.uniqueId )   return true;
            
            if( rhs.type != lhs.type )   return false;
            
            if( rhs.type == EnumType.BRANCH ) {
                if( rhs.referenced.Length != lhs.referenced.Length )   return false;
                for(int i = 0; i < rhs.referenced.Length; i++ )
                    if( rhs.referenced[i] != lhs.referenced[i] )   return false;
                return true;
            }
            else if( rhs.type == EnumType.SYMBOL ) {
                return rhs.symbol == lhs.symbol;
            }
            else {
                Debug.Assert(rhs.type == EnumType.VARIABLE);
                return rhs.variableId == lhs.variableId;
            }
        }
        

        public bool isBranch { get {
		    return type == EnumType.BRANCH;
	    } }
  
	    public bool @is(EnumType other) {
		    return type == other;
	    }

	    /* commented because not needed
	    final @property bool isLeaf() pure {
		    return !isBranch;
	    }
	    */

	    // used for fast matching
	    // VARIABLE must have the same variableId
	    public ulong uniqueId { get {
		    return privateUniqueId;
	    } }

	    public ulong variableId { get {
		    return privateUniqueId; // the variableId is equal to the unique id
	    } }

	    private ulong privateUniqueId;

        public static Pattern<DecorationType> makeDecoratedValue(ulong uniqueId) {
		    Pattern<DecorationType> result = Activator.CreateInstance<Pattern<DecorationType>>();
		    result.type = EnumType.DECORATEDVALUE;
		    result.privateUniqueId = uniqueId;
		    return result;
	    }
	    
	    public static Pattern<DecorationType> makeSymbol(ulong symbol, ulong uniqueId) {
		    Pattern<DecorationType> result = Activator.CreateInstance<Pattern<DecorationType>>();
		    result.type = EnumType.SYMBOL;
		    result.symbol = symbol;
		    result.privateUniqueId = uniqueId;
		    return result;
	    }

	    public static Pattern<DecorationType> makeVariable(ulong uniqueId) {
		    Pattern<DecorationType> result = Activator.CreateInstance<Pattern<DecorationType>>();
		    result.type = EnumType.VARIABLE;
		    result.privateUniqueId = uniqueId;
		    return result;
	    }

	    public static Pattern<DecorationType> makeBranch(ulong uniqueId, Pattern<DecorationType>[] children = null) {
            if( children == null )   children = new Pattern<DecorationType>[0];
            
		    Pattern<DecorationType> result = Activator.CreateInstance<Pattern<DecorationType>>();
		    result.type = EnumType.BRANCH;
		    result.referenced = children;
		    result.privateUniqueId = uniqueId;
		    return result;
	    }


        // defines the sematic for the symbol comparision
        public static bool isSameSymbol(ulong a, ulong b) {
	        return a == b;
        }
    }
}
