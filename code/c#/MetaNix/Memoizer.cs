using System;
using System.Collections.Generic;


namespace MetaNix {
    // remebers values of functioncalls
    // https://en.wikipedia.org/wiki/Memoization

    // currently not optimized for speed
    class MemoizationLookup {
        public class ParameterResultPair {
            public IList<Node> parameters;
            public Node result;

            public bool checkParametersEqual(IList<Node> parameters) {
                if(parameters.Count != this.parameters.Count) {
                    return false;
                }

                for (int i=0;i<parameters.Count;i++) {
                    if( !NodeHelper.checkEquality(parameters[i], this.parameters[i]) ) {
                        return false;
                    }
                }

                return true;
            }
        }

        public IList<ParameterResultPair> parameterResultPairs = new List<ParameterResultPair>();

        public void memoize(IList<Node> parameters, Node result) {
            ParameterResultPair createdPair = new ParameterResultPair();
            createdPair.parameters = parameters;
            createdPair.result = result;
            parameterResultPairs.Add(createdPair);
        }

        // returns null if it wasn't found
        public Node tryLookup(IList<Node> parameters) {
            foreach(ParameterResultPair iParameterResultPair in parameterResultPairs) {
                if( iParameterResultPair.checkParametersEqual(parameters) ) {
                    return iParameterResultPair.result;
                }
            }
            
            return null;
        }

    }
}
