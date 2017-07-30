using System;
using System.Diagnostics;

using TemporaryDerivedCompoundWithDecoration = MetaNix.nars.derivation.TemporaryDerivedCompoundType<MetaNix.nars.derivation.TemporaryDerivedCompoundDecoration>;
using TemporaryDerivedCompound = MetaNix.nars.derivation.TemporaryDerivedCompoundType<int>; 

namespace MetaNix.nars.derivation {
    // not possible in C#
    ///alias TemporaryDerivedCompoundType!TemporaryDerivedCompoundDecoration TemporaryDerivedCompoundWithDecoration;

    public class TemporaryDerivedCompoundDecoration {
        public enum EnumPayloadType {
            NONE,
            COMPOUNDINDEX,
            REFERER,
        }

        public EnumPayloadType payloadType {
            get;
            private set;
        }

        public CompoundIndex compoundIndex {
            get {
                Trace.Assert(payloadType == EnumPayloadType.COMPOUNDINDEX);
                return privateCompoundIndex;
            }
            set {
                payloadType = EnumPayloadType.COMPOUNDINDEX;
                privateCompoundIndex = value;
            }
        }

        public TermOrCompoundTermOrVariableReferer referer {
            get {
                Trace.Assert(payloadType == EnumPayloadType.REFERER);
                return privateReferer;
            }
            set {
                payloadType = EnumPayloadType.COMPOUNDINDEX;
                privateReferer = value;
            }
        }
        
        CompoundIndex privateCompoundIndex;
        TermOrCompoundTermOrVariableReferer privateReferer;

        // returns the refered payload as an referer
        public TermOrCompoundTermOrVariableReferer returnReferer(CompoundAndTermContext compoundAndTermContext) {
            Trace.Assert(payloadType != EnumPayloadType.NONE, "Must not be None because it must have a value to be referenced!"); // if it is NONE we can't return the referer

            if( payloadType == EnumPayloadType.REFERER ) {
                return privateReferer;
            }
            else {
                Debug.Assert(payloadType == EnumPayloadType.COMPOUNDINDEX);
                return compoundAndTermContext.accessCompoundByIndex(compoundIndex).thisTermReferer;
            }
        }

        public static TemporaryDerivedCompoundWithDecoration makeRecursive(TemporaryDerivedCompound temporaryDerivedCompound) {
            switch (temporaryDerivedCompound.type) {
			case TemporaryDerivedCompound.EnumType.COMPOUND: return TemporaryDerivedCompoundWithDecoration.makeBinaryCompound(temporaryDerivedCompound.flagsOfCopula, makeRecursive(temporaryDerivedCompound.leftChildren), makeRecursive(temporaryDerivedCompound.rightChildren));
			case TemporaryDerivedCompound.EnumType.LEAF: return TemporaryDerivedCompoundWithDecoration.makeLeaf(temporaryDerivedCompound.termReferer);
			case TemporaryDerivedCompound.EnumType.INDEPENDENTVARIABLE: return TemporaryDerivedCompoundWithDecoration.makeReferenceIndependentVariable(temporaryDerivedCompound.independentVariableId);
			case TemporaryDerivedCompound.EnumType.DEPENDENTVARIABLE: return TemporaryDerivedCompoundWithDecoration.makeReferenceDependentVariable(temporaryDerivedCompound.dependentVariableId);
            }

            throw new Exception("Internal error - shouldn't be reachable!");
        }
    }

    // not possible in C#
    // int is dummy
    //alias TemporaryDerivedCompoundType!int TemporaryDerivedCompound;

    public class TemporaryDerivedCompoundType<Type> {
	    public enum EnumType {
            COMPOUND,
            LEAF,
            INDEPENDENTVARIABLE,
            DEPENDENTVARIABLE,
        }

        public static TemporaryDerivedCompoundType<Type> makeBinaryCompound(FlagsOfCopula flagsOfCopula, TemporaryDerivedCompoundType<Type> leftChildren, TemporaryDerivedCompoundType<Type> rightChildren) {
            TemporaryDerivedCompoundType<Type> result = new TemporaryDerivedCompoundType<Type>();
            result.decoration = Activator.CreateInstance<Type>();
            result.flagsOfCopula = flagsOfCopula;
            result.leftChildren = leftChildren;
            result.rightChildren = rightChildren;
            result.type = EnumType.COMPOUND;
            return result;
        }

        public static TemporaryDerivedCompoundType<Type> makeLeaf(TermOrCompoundTermOrVariableReferer termReferer) {
            TemporaryDerivedCompoundType<Type> result = new TemporaryDerivedCompoundType<Type>();
            result.decoration = Activator.CreateInstance<Type>();
            result.protectedTermReferer = termReferer;
            result.type = EnumType.LEAF;
            return result;
        }

        public static TemporaryDerivedCompoundType<Type> makeReferenceIndependentVariable(uint id) {
            TemporaryDerivedCompoundType<Type> result = new TemporaryDerivedCompoundType<Type>();
            result.protectedVariableId = id;
            result.type = EnumType.INDEPENDENTVARIABLE;
            return result;
        }

        public static TemporaryDerivedCompoundType<Type> makeReferenceDependentVariable(uint id) {
            TemporaryDerivedCompoundType<Type> result = new TemporaryDerivedCompoundType<Type>();
            result.protectedVariableId = id;
            result.type = EnumType.DEPENDENTVARIABLE;
            return result;
        }

        public string debugToStringRecursivly(CompoundAndTermContext compoundAndTermContext) {
            if (isLeaf) {
                return String.Format("<LEAF={0}>", compoundAndTermContext.getDebugStringByTermReferer(termReferer));
            }
            else if (isIndependentVariable) {
                return String.Format("<$VAR:{0}>", independentVariableId);
            }
            else if (isDependentVariable) {
                return String.Format("<#VAR:{0}>", dependentVariableId);
            }
            else {
                // TODO< implement for nonbinary >
                return String.Format("<COMPOUND={0} {1} {2}>", leftChildren.debugToStringRecursivly(compoundAndTermContext), flagsOfCopula.convToHumanString(), rightChildren.debugToStringRecursivly(compoundAndTermContext));
            }
        }


        public uint calcComplexityRecursive(CompoundAndTermContext compoundAndTermContext) {
            if (isLeaf) {
                return compoundAndTermContext.getTermComplexityOfAndByTermReferer(termReferer);
            }
            else if (isIndependentVariable) {
                return Compound.COMPLEXITYINDEPENDENTVARIABLE;
            }
            else if (isDependentVariable) {
                return Compound.COMPLEXITYDEPENDENTVARIABLE;
            }
            else {
                uint complexity = flagsOfCopula.getComplexityOfFlagsOfCopula();

                // TODO< implement for nonbinary >
                complexity += leftChildren.calcComplexityRecursive(compoundAndTermContext);
                complexity += rightChildren.calcComplexityRecursive(compoundAndTermContext);
                return complexity;
            }
        }


        public TemporaryDerivedCompoundType<Type> getCompoundByIndex(ulong index) {
            // TODO< ensure type is COMPOUND for debug build >
            // TODO< ensure index is in range for debug build >

            // TODO< rewrite code to access array >
            if (index == 0) {
                return leftChildren;
            }
            else if (index == 1) {
                return rightChildren;
            }
            else {
                throw new Exception("invalid index");
            }
        }

        public ulong getCompoundLength() {
            // TODO< ensure type is COMPOUND for debug build >

            // TODO< rewrite code to access array >
            return 2;
        }

        public EnumType type;
        public FlagsOfCopula flagsOfCopula; // TODO< accessor >
        public TemporaryDerivedCompoundType<Type> leftChildren; // TODO< accessor >
        public TemporaryDerivedCompoundType<Type> rightChildren; // TODO< accessor >

        public TermOrCompoundTermOrVariableReferer termReferer { get {
            Debug.Assert(type == EnumType.LEAF);
		    return protectedTermReferer;
	    } }

        public uint independentVariableId { get {
            Debug.Assert(type == EnumType.INDEPENDENTVARIABLE);
		    return protectedVariableId;
	    } }

        public uint dependentVariableId { get {
            Debug.Assert(type == EnumType.DEPENDENTVARIABLE);
		    return protectedVariableId;
	    } }

        public bool isIndependentVariable { get {
		    return type == EnumType.INDEPENDENTVARIABLE;
	    } }

	    public bool isDependentVariable { get {
		    return type == EnumType.DEPENDENTVARIABLE;
	    } }

	    public bool isVariable { get {
		    return isIndependentVariable || isDependentVariable;
	    } }

	    public bool isLeaf { get {
		    return type == EnumType.LEAF;
	    } }

	    public bool isCompound { get {
		    return type == EnumType.COMPOUND;
	    } }


	    public uint termComplexity;

        protected TermOrCompoundTermOrVariableReferer protectedTermReferer;

        protected uint protectedVariableId;

        public Type decoration;
    }

    public class DeriverUtilities {
        // utilities used by the autogenerated deriver

        public static TemporaryDerivedCompound genBinary(FlagsOfCopula flagsOfCopula, TemporaryDerivedCompound left, TemporaryDerivedCompound right) {
            return TemporaryDerivedCompound.makeBinaryCompound(flagsOfCopula, left, right);
        }

        public static TemporaryDerivedCompound genBinary(FlagsOfCopula flagsOfCopula, TemporaryDerivedCompound left, TermOrCompoundTermOrVariableReferer termRefererRight) {
            return TemporaryDerivedCompound.makeBinaryCompound(flagsOfCopula, left, TemporaryDerivedCompound.makeLeaf(termRefererRight));
        }

        public static TemporaryDerivedCompound genBinary(FlagsOfCopula flagsOfCopula, TermOrCompoundTermOrVariableReferer termRefererLeft, TemporaryDerivedCompound right) {
            return TemporaryDerivedCompound.makeBinaryCompound(flagsOfCopula, TemporaryDerivedCompound.makeLeaf(termRefererLeft), right);
        }

        public static TemporaryDerivedCompound genBinary(FlagsOfCopula flagsOfCopula, TermOrCompoundTermOrVariableReferer termRefererLeft, TermOrCompoundTermOrVariableReferer termRefererRight) {
            return TemporaryDerivedCompound.makeBinaryCompound(flagsOfCopula, TemporaryDerivedCompound.makeLeaf(termRefererLeft), TemporaryDerivedCompound.makeLeaf(termRefererRight));
        }

        public static TemporaryDerivedCompound makeReferenceIndependentVariable(uint id) {
            return TemporaryDerivedCompound.makeReferenceIndependentVariable(id);
        }

        public static TemporaryDerivedCompound makeReferenceDependentVariable(uint id) {
            return TemporaryDerivedCompound.makeReferenceDependentVariable(id);
        }

    }



    public class TemporaryDerivedTerm {
        public TemporaryDerivedCompound derivedCompound;
        public RuleTable.EnumTruthFunction truthfunction;
        
        public static TemporaryDerivedTerm genTerm(TemporaryDerivedCompound derivedCompound, RuleTable.EnumTruthFunction truthfunction) {
            TemporaryDerivedTerm result = new TemporaryDerivedTerm();
            result.derivedCompound = derivedCompound;
            result.truthfunction = truthfunction;
            return result;
        }
    }


    // used to carry the derivation result which holds aleady the compound and the truth function and truthvalue
    // TODO< move this to own file/other file >
    public class TemporaryDerivedCompoundWithDecorationAndTruth {
        public TemporaryDerivedCompoundWithDecoration derivedCompoundWithDecoration;
        public RuleTable.EnumTruthFunction truthfunction;
        public TruthValue truth; // cached calculated truth
    }
}
