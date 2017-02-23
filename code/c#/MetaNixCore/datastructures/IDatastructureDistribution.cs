using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix.datastructures {
    // used to select the element index over an distribution
    // is an unification for nars-bag and stochastic selection mechanisms of arrays
    interface IDatastructureDistribution {
        // \param uniformDistributionValue [0..1)
        int getIndexByUniformRandomVariable(float uniformDistributionValue, int numberOfElements);
    }
}
