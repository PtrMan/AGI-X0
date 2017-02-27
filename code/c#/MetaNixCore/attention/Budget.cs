using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix.attention {
    public class Budget {
        public double priority; // current priority



        public double boostFactor = 1.0; // used to push the priority
                                   // can be used to give important task more resources

        public void boost() {
            priority *= boostFactor;
        }


        public double decayExponent = 0.0; //indirectly influences the decay factor by e^(decayExponent*deltaTimeInSeconds)

        public double getDecayFactorByDeltaInSeconds(double timeDeltaInSeconds) {
            Ensure.ensureHard(decayExponent <= 0.0);
            return Math.Exp(decayExponent*timeDeltaInSeconds);
        }

        public void recalcPriorityByDeltaInSeconds(double timeDeltaInSeconds) {
            priority *= getDecayFactorByDeltaInSeconds(timeDeltaInSeconds);
        }

        public static Budget makeByPriorityAndDecayExponent(double priority, double decayExponent) {
            Budget budget = new Budget();
            budget.priority = priority;
            budget.decayExponent = decayExponent;
            return budget;
        }
    }
}
