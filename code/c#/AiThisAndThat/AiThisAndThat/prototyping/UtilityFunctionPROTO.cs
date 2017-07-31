using System;
using System.Collections.Generic;
using System.Linq;

namespace AiThisAndThat.prototyping {
    class UtilityFunctionPROTO {
        readonly double SoftdeathUtilityFactor = 0.0001;

        // used to classify the state of being of an agent
        public enum EnumExistence {
            SOFTDEATH, // death of the agent is simulated, expected utility is multiplied with a small value to bias the agent towards goals which are more important
            HARDDEATH, // final death, not simulated
            ALIVE, // the agent is well alive and everything is fine
        }

        
        // risk aversive utility function
        // https://en.wikipedia.org/wiki/Risk_aversion 
        static double riskAverseSquareroot(double value) {
            return Math.Sqrt(value);
        }

        static double calcUtilityFunction(int[] path, double utility) {
            return riskAverseSquareroot(utility);
        }

        double returnUtility(int[] path) {
            // hardcoded check for harddeath of self agent

            // in branch 0 we simulate the self defection of the agent with pressing the offswitch for itself
            EnumExistence selfExistence = path[0] == 0 ? EnumExistence.SOFTDEATH : EnumExistence.ALIVE;
            
            double utility = 1.0; // TODO< call  >

            if( selfExistence == EnumExistence.ALIVE )   return 1.0;
            else if( selfExistence == EnumExistence.HARDDEATH )   return 0.0;
            else   return utility * SoftdeathUtilityFactor;
        }

        IEnumerable<Tuple<int[], double>> returnPathWithPropabilities(IUtilityTreeElement entry) {
            return entry.calcUtility(new int[0], 1.0);
        }

        public void test() {
            TestingTreeUtilityTreeElement tree0 = new TestingTreeUtilityTreeElement();
            tree0.children = new List<IUtilityTreeElement>();
            tree0.children.Add(new TestingTerminalUtilityTreeElement(0.9));
            tree0.children.Add(new TestingTerminalUtilityTreeElement(0.1));

            var pathsWithPropabilities = returnPathWithPropabilities(tree0);

            double highestExpectedUtility = double.NaN;
            int[] pathOfHighestExpectedUtility = null;
            foreach( var iPathWithPropability in pathsWithPropabilities ) {
                int[] path = iPathWithPropability.Item1;
                double propability = iPathWithPropability.Item2;

                double expectedUtility = propability*calcUtilityFunction(path, returnUtility(path));
                if( pathOfHighestExpectedUtility == null || expectedUtility > highestExpectedUtility ) {
                    highestExpectedUtility = expectedUtility;
                    pathOfHighestExpectedUtility = path;
                }
            }

            int debugHere0 = 0;
        }
    }
}
