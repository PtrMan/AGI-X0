using System;
using System.Collections.Generic;

using MetaNix.datastructures;

namespace MetaNix {
    // remebers values of functioncalls
    // https://en.wikipedia.org/wiki/Memoization

    // currently not optimized for speed
    class MemoizationLookup {
        public class ParameterResultPair {
            public IList<ImmutableNodeReferer> parameters;
            public ImmutableNodeReferer result;

            public bool checkParametersEqual(IList<ImmutableNodeReferer> parameters) {
                if(parameters.Count != this.parameters.Count) {
                    return false;
                }

                for (int i=0;i<parameters.Count;i++) {
                    if( !ImmutableNodeReferer.checkEquality(parameters[i], this.parameters[i]) ) {
                        return false;
                    }
                }

                return true;
            }
        }

        public IList<ParameterResultPair> parameterResultPairs = new List<ParameterResultPair>();

        public void memoize(IList<ImmutableNodeReferer> parameters, ImmutableNodeReferer result) {
            ParameterResultPair createdPair = new ParameterResultPair();
            createdPair.parameters = parameters;
            createdPair.result = result;
            parameterResultPairs.Add(createdPair);
        }

        // returns null if it wasn't found
        public ImmutableNodeReferer tryLookup(IList<ImmutableNodeReferer> parameters) {
            foreach(ParameterResultPair iParameterResultPair in parameterResultPairs) {
                if( iParameterResultPair.checkParametersEqual(parameters) ) {
                    return iParameterResultPair.result;
                }
            }
            
            return null;
        }

    }
}
