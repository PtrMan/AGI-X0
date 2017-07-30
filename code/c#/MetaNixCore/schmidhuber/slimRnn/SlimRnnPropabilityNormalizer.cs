using System.Collections.Generic;

namespace MetaNix.schmidhuber.slimRnn {
    // used to normalize the propabilities for the search algorithm
    public static class SlimRnnPropabilityNormalizer {
        public static void normalize(IList<UniversalSlimRnnSearch.WeightWithPropability> weightWithPropabilities) {
            double propabilitySum = 0;

            foreach( var iWeightWithPropability in weightWithPropabilities ) {
                propabilitySum += iWeightWithPropability.propability;
            }

            foreach (var iWeightWithPropability in weightWithPropabilities) {
                iWeightWithPropability.propability = iWeightWithPropability.propability / propabilitySum;
            }
        }
    }
}
