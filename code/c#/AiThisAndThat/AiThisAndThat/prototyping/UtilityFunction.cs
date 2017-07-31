using System;
using System.Collections.Generic;

namespace AiThisAndThat.prototyping {
    // calculates the utility
    public interface IUtilityTreeElement {
        IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent);

        double propability {
            get;
        }
    }

    // just for testing
    // terminates the descision tree search
    class TestingTerminalUtilityTreeElement : IUtilityTreeElement {
        public TestingTerminalUtilityTreeElement(double propability) {
            this.privatePropability = propability;
        }

        public double propability => privatePropability;

        public IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent) {
            yield return new Tuple<int[], double>(path, propabilityOfParent*propability);
        }

        double privatePropability;
    }

    // just for testing
    class TestingTreeUtilityTreeElement : IUtilityTreeElement {
        public IList<IUtilityTreeElement> children;

        public double propability => 0.5;

        public IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent) {
            int idx = 0;
            foreach( var iChildren in children ) {
                int[] childrenPath = new int[path.Length+1];
                Array.Copy(path, childrenPath, path.Length);
                childrenPath[childrenPath.Length-1] = idx;
                
                var resultFromChildren = iChildren.calcUtility(childrenPath, propabilityOfParent*propability);
                foreach( var iResultFromChildren in resultFromChildren )   yield return iResultFromChildren;

                idx++;
            }
        }
    }
}
