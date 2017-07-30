using System;
using MetaNix.nars.memory;
using System.Diagnostics;

namespace MetaNix.nars.entity {
    // see by https://github.com/opennars/opennars/blob/1.6.5_devel17_RetrospectiveAnticipation/nars_core/nars/entity/TermLink.java
    /**
     * A link between a compound term and a component term
     * <p>
     * A TermLink links the current Term to a target Term, which is 
     * either a component of, or compound made from, the current term.
     * <p>
     * Neither of the two terms contain variable shared with other terms.
     * <p>
     * The index value(s) indicates the location of the component in the compound.
     * <p>
     * This class is mainly used in inference.RuleTable to dispatch premises to inference rules
     */
    public class ClassicalTermLink : Item<ClassicalTermLink>, INamed<ClassicalTermLink> {
        public enum EnumType {
            /** At C, point to C; TaskLink only */
            SELF = 0,
            /** At (&&, A, C), point to C */
            COMPONENT = 1,
            /** At C, point to (&&, A, C) */
            COMPOUND = 2,
            /** At <C --> A>, point to C */
            COMPONENT_STATEMENT = 3,
            /** At C, point to <C --> A> */
            COMPOUND_STATEMENT = 4,
            /** At <(&&, C, B) ==> A>, point to C */
            COMPONENT_CONDITION = 5,
            /** At C, point to <(&&, C, B) ==> A> */
            COMPOUND_CONDITION = 6,
            /** At C, point to <(*, C, B) --> A>; TaskLink only */
            TRANSFORM = 8,
            /** At C, point to B, potentially without common subterm term */
            TEMPORAL = 9,
        }
        
        public TermOrCompoundTermOrVariableReferer target; /** The linked Term */
        public EnumType type; /** The type of link, one of the above */

        /** The index of the component in the component list of the compound, may have up to 4 levels */
        public uint[] index; // ushort?

        public override ClassicalTermLink name => this;

        public static ClassicalTermLink makeFromTemplate(TermOrCompoundTermOrVariableReferer term, ClassicalTermLink template, ClassicalBudgetValue budget) {
            return new ClassicalTermLink(term, template, budget);
        }

        /**
         * Constructor for TermLink template
         * <p>
         * called in CompoundTerm.prepareComponentLinks only
         * /param target Target Term
         * /param type Link type
         * /param indices Component indices in compound, may be 1 to 4
         */
        public ClassicalTermLink(TermOrCompoundTermOrVariableReferer target, EnumType type, params uint[] indices) : base(null) {
            this.target = target;
            this.type = type;
            Debug.Assert(((uint)type % 2) == 0); // template types all point to compound, though the target is component
            if (type == EnumType.COMPOUND_CONDITION) {  // the first index is 0 by default
                index = new uint[indices.Length + 1];
                // index[0] = 0; //first index is zero, but not necessary to set since index[] was just created

                Array.Copy(indices, 0, index, 1, indices.Length);
            }
            else {
                index = indices;
            }
            // OpenNARS  hash = init();
        }

        /**
         * Constructor to make actual TermLink from a template
         * <p>
         * called in Concept.buildTermLinks only
         * @param t Target Term
         * @param template TermLink template previously prepared
         * @param v Budget value of the link
         */
        private ClassicalTermLink(TermOrCompoundTermOrVariableReferer target, ClassicalTermLink template, ClassicalBudgetValue budget) : base(budget) {
            this.target = target;
            type = (template.target == target)
                    ? (template.type - 1) //// point to component
                    : template.type;
            index = template.index;
            // OpenNARS  hash = init();
        }

        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/entity/TermLink.java#L137
        public bool checkEqual(ClassicalTermLink other) {
            // triple commented is commented because it's not fully implemented

            if (other == this) return true;
            ///if (hashCode() != other.hashCode()) return false;

            if (other is ClassicalTermLink) {
                ClassicalTermLink t = (ClassicalTermLink)other;

                if (type != t.type) return false;
                if (!(t.index == index)) return false;

                TermOrCompoundTermOrVariableReferer tt = t.target;
                if (target == null) {
                    if (tt != null) return false;
                }
                else if (tt == null) {
                    if (target != null) return false;
                }
                else if (!TermOrCompoundTermOrVariableReferer.isSameWithId(target, t.target)) {
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}
