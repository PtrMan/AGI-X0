using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
        
        public ImmutableArray<ImmutableNodeReferer> children;
        
        public static ImmutableNodeReferer makeBranch(IList<ImmutableNodeReferer> children = null) {
            return new ImmutableNodeReferer(children == null ? new List<ImmutableNodeReferer>() : children);
        }

        public static ImmutableNodeReferer makeNonbranch(ValueNode referencedValueNode) {
            ImmutableNodeReferer result = new ImmutableNodeReferer();
            result.referencedValueNode = referencedValueNode;
            return result;
        }

        private ImmutableNodeReferer() {
        }

        private ImmutableNodeReferer(IList<ImmutableNodeReferer> children) {
            var builder = ImmutableArray.CreateBuilder<ImmutableNodeReferer>();
            builder.AddRange(children);
            this.children = builder.ToImmutable();
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
                if (a.children.Count() != b.children.Count()) {
                    return false;
                }

                for (int i = 0; i < a.children.Count(); i++) {
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
    public sealed class NodeRefererEntryManipulationHelper {
        public static NodeRefererEntry makeImmutableArray(IList<Variant> values) {
            ImmutableNodeReferer entryNodeReferer = ImmutableNodeRefererManipulatorHelper.makeImmutableNodeRefererForArray(values);
            return new NodeRefererEntry(entryNodeReferer);
        }
        
        public static NodeRefererEntry arrayInsert(NodeRefererEntry refererEntry, int idx, ImmutableNodeReferer insert) {
            return arrayInsert(refererEntry, new List<Tuple<int, ImmutableNodeReferer>>{new Tuple<int, ImmutableNodeReferer>(idx, insert)} );
        }

        /** 
         * inserts the elements at the corresponding indices into the array referenced by "refererEntry"
         * and returns a new NodeRefererEntry which points at the "ImmutableNodeReferer" which desribes the result array
         * 
         * indices must be sorted!
         * 
         * \param refererEntry the target where the elements are inserted into
         * \param elementsWithIndices array of elements with indices, must be sorted by index
         */
        public static NodeRefererEntry arrayInsert(NodeRefererEntry refererEntry, IList<Tuple<int, ImmutableNodeReferer>> elementsWithIndices) {
            Ensure.ensure(refererEntry.entry.isBranch);

            ImmutableNodeReferer resultReferer = ImmutableNodeRefererManipulatorHelper.copy(refererEntry.entry);

            int? previousIdx = null;

            // we iterate from elements with high indices to low because we don't have to keep track of index changes

            foreach(var iElementWithIndex in elementsWithIndices.Reverse()) {
                int idx = iElementWithIndex.Item1;
                ImmutableNodeReferer nodeReferer = iElementWithIndex.Item2;

                Ensure.ensureHard(previousIdx.HasValue ? idx < previousIdx : true); // make sure the index is smaller than the previous one

                resultReferer.children = resultReferer.children.Insert(idx, nodeReferer);

                previousIdx = idx;
            }

            return new NodeRefererEntry(resultReferer);
        }

        public static NodeRefererEntry arrayClear(NodeRefererEntry refererEntry) {
            Ensure.ensure(refererEntry.entry.isBranch);

            ImmutableNodeReferer resultReferer = ImmutableNodeReferer.makeBranch(new List<ImmutableNodeReferer>());
            return new NodeRefererEntry(resultReferer);
        }
        
        public static NodeRefererEntry arrayRemove(NodeRefererEntry refererEntry, int index) {
            return arrayRemove(refererEntry, new List<int> { index });
        }

        /** 
         * inserts the elements at the corresponding indices into the array referenced by "refererEntry"
         * and returns a new NodeRefererEntry which points at the "ImmutableNodeReferer" which desribes the result array
         * 
         * indices must be sorted!
         * 
         * \param refererEntry the target where the elements are inserted into
         * \param indices array of indices, must be sorted by index
         */
        public static NodeRefererEntry arrayRemove(NodeRefererEntry refererEntry, IList<int> indices) {
            Ensure.ensure(refererEntry.entry.isBranch);

            ImmutableNodeReferer resultReferer = ImmutableNodeRefererManipulatorHelper.copy(refererEntry.entry);

            int? previousIdx = null;

            // we iterate from elements with high indices to low because we don't have to keep track of index changes
            foreach (int idx in indices.Reverse()) {
                Ensure.ensureHard(previousIdx.HasValue ? idx < previousIdx : true); // make sure the index is smaller than the previous one

                Ensure.ensure(idx < resultReferer.children.Count());
                resultReferer.children = resultReferer.children.RemoveAt(idx);

                previousIdx = idx;
            }

            return new NodeRefererEntry(resultReferer);
        }

        // TODO< array subarray, concatenate >
    }

    public sealed class ImmutableNodeRefererManipulatorHelper {
        public static ImmutableNodeReferer makeImmutableNodeRefererForArray(IList<Variant> values) {
            var resultChildren = new List<ImmutableNodeReferer>(new ImmutableNodeReferer[values.Count]);
            for (int i = 0; i < values.Count; i++) {
                resultChildren[i] = ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(values[i]));
            }

            return ImmutableNodeReferer.makeBranch(resultChildren);
        }


        public static ImmutableNodeReferer makeString(string value) {
            ImmutableNodeReferer result = ImmutableNodeReferer.makeBranch();
            result.children = result.children.Add(ImmutableNodeReferer.makeNonbranch(ValueNode.makeDatatype("string")));
            for (int i = 0; i < value.Length; i++) {
                result.children = result.children.Add(ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(Variant.makeInt(value[i]))));
            }

            return result;
        }

        // shallow copie of branch or just passthrough if element is nonbranch
        // doesn't touch parent
        public static ImmutableNodeReferer copy(ImmutableNodeReferer element) {
            if(!element.isBranch) {
                return element;
            }

            ImmutableNodeReferer result = ImmutableNodeReferer.makeBranch();

            // copy
            for( int i = 0; i < element.children.Count(); i++ ) {
                result.children = result.children.Add(element.children[i]);
            }

            return result;
        }
    }
}
