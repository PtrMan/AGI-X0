using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiThisAndThat {
    public static class PropabilisticHelper {
        // calculates the expectation of/from a propability
        // book "Propability Theory: The logic of science"    page 92
        public static double propabilityToEvidence(double propability) {
            return Math.Log10(propability/(1-propability));
        }

        // book "Propability Theory: The logic of science"    page 92
        public static double oddsToPropability(double odds) {
            return odds / (1 + odds);
        }
    }

    public class ProgramLaptop {
        // 1/2
        // 2/3
        // 3/5

        
        
            /*
        static void Main(string[] args) {
            double o1 = PropabilisticHelper.oddsToPropability(1.0);
            double o2 = PropabilisticHelper.oddsToPropability(2.0);
            double o5 = PropabilisticHelper.oddsToPropability(5.0);
            double o10 = PropabilisticHelper.oddsToPropability(10.0);
            double o100 = PropabilisticHelper.oddsToPropability(100.0);

            
            int debugHere5 = 1;
        }*/
    }
}
