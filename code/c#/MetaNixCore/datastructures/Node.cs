using System;
using System.Collections.Generic;

namespace MetaNix.datastructures {
    /**
     * 
     * 
     * Immutable Node datastructure.
     * The Datastructure can be used in an immutable mode (where all values are immutable) or in an mutable mode for efficiency reasons.
     * 
     * "B <<-- A" is interpreted as "A points at B"
     *                                               +- parent ---+
     *                                               V            |        
     *                    ValueNode |  <<--   | Immutable Node Referer |  <<--     | Node Referer Entry |
     *                                               ^            |     
     *                                               +- children -+     
     *                                                  0..n
     * 
     * Multiple "Node referer entry"s can point to the same "Immutable Node Referer" because the program can hold 
     * the same "Immutable Node Referer" from different points in the other datastructures.
     * Additionally an array of "Immutable Node Referer" doesn't have to be deep copied when just ne element is changed.
     * 
     * If an "Immutable Node Referer" is describing an array and the array is changed then a new "Immutable Node Referer"
     * has to be created which references the new content of the array. to reference the new array from the outside a new 
     * "Node Referer Entry" has to be created which points at the "Immutable Node Referer" which holds the new array.
     * 
     */

    // holds the value
    public sealed class ValueNode {
        public enum EnumMutability {
            MUTABLE,
            IMMUTABLE,
        }

        public static ValueNode makeAtomic(Variant value, EnumMutability mutability = EnumMutability.IMMUTABLE) {
            ValueNode result = new ValueNode(EnumType.VALUE, mutability);
            result.privateValue = value;
            return result;
        }

        public static ValueNode makeInstr(EnumType type, EnumMutability mutability = EnumMutability.IMMUTABLE) {
            Ensure.ensure(type != EnumType.STRING_DATATYPE && type != EnumType.VALUE); // real ensure and not just InterpretationException
            return new ValueNode(type, mutability);
        }

        public static ValueNode makeDatatype(string datatype, EnumMutability mutability = EnumMutability.IMMUTABLE) {
            ValueNode result = new ValueNode(EnumType.STRING_DATATYPE, mutability);
            result.privateValue = Variant.makeString(datatype);
            return result;
        }

        ValueNode(EnumType type, EnumMutability mutability) {
            privateType = type;
            privateMutability = mutability;
        }

        public enum EnumType {
            VALUE, // variant value

            STRING_DATATYPE, // native string payload is used to indicate the datatype of the parent branch, like [DATATYPE:string, 20, 20, 54, 63, 20, 20], can also be used to store the datatypes of functions etc

            INSTR_CMP_NEQ_INT,
            INSTR_CMP_EQ_INT, // compare equal and store in condition flag
            INSTR_CMP_G_INT,
            INSTR_CMP_GE_INT,

            INSTR_GOTO,

            INSTR_ADD_INT,
            INSTR_SUB_INT,
            INSTR_MUL_INT,

            INSTR_DEREF_INT, // follows the 1st argument which is a relative path to an int and puts the value into the register
        }

        public EnumMutability mutability {
            get {
                return privateMutability;
            }
        }

        public bool isMutable {
            get {
                return privateMutability == EnumMutability.MUTABLE;
            }
        }
        

        public Variant value {
            get {
                return privateValue;
            }
        }

        public long valueInt {
            get {
                return privateValue.valueInt;
            }
        }

        public string valueString {
            get {
                Ensure.ensure(privateType == EnumType.STRING_DATATYPE); // is only represented by the string if it's a datatype!
                return privateValue.valueString;
            }
        }

        public EnumType type {
            get {
                return privateType;
            }
        }

        EnumType privateType;
        Variant privateValue;
        EnumMutability privateMutability;
    }

    public sealed class ImmutableNodeReferer {
        public ValueNode referencedValueNode;
        
        public ImmutableNodeReferer interpretationResult; // is most of the time null
                                                          // is mutable too for an efficient interpretation
                                                          // PARALLELIZATION TODO< this has to be an array for an multithreaded interpretation with the # of threads because it will be changed by many threads at the same time >

        public IList<ImmutableNodeReferer> children;
        public ImmutableNodeReferer parent; // parent of the tree

        /* uncommented because not used 
        public ImmutableNodeReferer getChildrenAt(int idx) {
            Ensure.ensureHard(children != null); // hard ensure because soft ensure can be done extrnally
                                                 // we don't rely on the C#VM to throw an exeption on array access because the exception would be too general
            return children[idx];
        }*/

        public static ImmutableNodeReferer makeBranch(IList<ImmutableNodeReferer> children = null, ImmutableNodeReferer parent = null) {
            ImmutableNodeReferer result = new ImmutableNodeReferer();
            result.children = children == null ? new List<ImmutableNodeReferer>() : children; // small trick because function parameter must be compiletime
            result.parent = parent;
            return result;
        }

        public static ImmutableNodeReferer makeNonbranch(ValueNode referencedValueNode, ImmutableNodeReferer parent = null) {
            ImmutableNodeReferer result = new ImmutableNodeReferer();
            result.referencedValueNode = referencedValueNode;
            result.parent = parent;
            return result;
        }

        private ImmutableNodeReferer() {

        }

        public bool isBranch {
            get {
                return children != null;
            }
        }

        //////////
        // delegation to value for referers without children

        public Variant value {
            get {
                Ensure.ensure(children == null);
                return referencedValueNode.value;
            }
        }

        public ValueNode.EnumType type {
            get {
                Ensure.ensure(children == null);
                return referencedValueNode.type;
            }
        }

        public long valueInt {
            get {
                Ensure.ensure(children == null);
                return referencedValueNode.valueInt;
            }
        }

        public string valueString {
            get {
                Ensure.ensure(children == null);
                return referencedValueNode.valueString;
            }
        }

        ////////////
        // static helper functions

        internal static bool checkEquality(ImmutableNodeReferer a, ImmutableNodeReferer b) {
            if( a.isBranch != b.isBranch ) {
                return false;
            }
            
            if (a.isBranch) {
                if (a.children.Count != b.children.Count) {
                    return false;
                }

                for (int i = 0; i < a.children.Count; i++) {
                    if (!checkEquality(a.children[i], b.children[i])) {
                        return false;
                    }
                }
            }
            else {
                if (a.type != b.type) {
                    return false;
                }

                if (!a.value.checkEquality(b.value)) {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class NodeRefererEntry {
        public ImmutableNodeReferer entry;

        public NodeRefererEntry(ImmutableNodeReferer entry) {
            this.entry = entry;
        }
    }

    
    // class to simplify the manipulation of the node datastructure
    sealed class NodeRefererEntryManipulationHelper {
        public static NodeRefererEntry makeImmutableArray(IList<Variant> values) {
            ImmutableNodeReferer entryNodeReferer = ImmutableNodeRefererManipulatorHelper.makeImmutableNodeRefererForArray(values);
            return new NodeRefererEntry(entryNodeReferer);
        }

        

        // TODO< add elements to array, remove, clear array >
    }

    sealed class ImmutableNodeRefererManipulatorHelper {
        public static ImmutableNodeReferer makeImmutableNodeRefererForArray(IList<Variant> values, ImmutableNodeReferer parent = null) {
            ImmutableNodeReferer result = ImmutableNodeReferer.makeBranch(new List<ImmutableNodeReferer>(values.Count), parent);

            for (int i = 0; i < values.Count; i++) {
                result.children[i] = ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(values[i]));
            }

            return result;
        }


        public static ImmutableNodeReferer makeString(string value, ImmutableNodeReferer parent = null) {
            ImmutableNodeReferer result = ImmutableNodeReferer.makeBranch(new List<ImmutableNodeReferer>(new ImmutableNodeReferer[1 + value.Length]), parent);

            result.children[0] = ImmutableNodeReferer.makeNonbranch(ValueNode.makeDatatype("string"));
            for (int i = 0; i < value.Length; i++) {
                result.children[1+i] = ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(Variant.makeInt(value[i])));
            }

            return result;
        }
    }
}
