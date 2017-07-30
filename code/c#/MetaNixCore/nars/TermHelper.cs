using System.Collections.Generic;
using System.Diagnostics;

using MetaNix.nars.entity;

namespace MetaNix.nars {
    static class TermHelper {
        public static IList<ClassicalTermLink> prepareComponentLinks(CompoundAndTermContext compoundAndTermContext, TermOrCompoundTermOrVariableReferer term, ClassicalTermLink.EnumType type) {
            // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/language/Terms.java#L404

            IList<ClassicalTermLink> componentLinks = new List<ClassicalTermLink>();

            // doesn't go down a level because the Concept already does this for us

            Compound compoundOfTerm = compoundAndTermContext.translateTermOfCompoundToCompound(term);

            for( int componentIndex = 0; componentIndex < compoundOfTerm.getComponentLength(compoundAndTermContext); componentIndex++ ) {
                RefererOrInterval refererOfInterval = compoundOfTerm.getComponentByIndex(compoundAndTermContext, (uint)componentIndex);
                Debug.Assert(!refererOfInterval.isInterval);
                TermOrCompoundTermOrVariableReferer refered = refererOfInterval.referer;

                componentLinks.Add(new ClassicalTermLink(refered, type, (uint)componentIndex));
            }

            // TODO< other link types >

            return componentLinks;
        }
    }
}
