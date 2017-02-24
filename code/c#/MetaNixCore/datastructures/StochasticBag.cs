using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix.datastructures {
    public class StochasticBag {
        public float getValueByUniformRandomVariable(float uniformDistributionValue) {
            return propabilities[getIndexByUniformRandomVariable(uniformDistributionValue)];
        }

        private int getIndexByUniformRandomVariable(float uniformDistributionValue) {
            Ensure.ensureHard(propabilities.Count > 0);

            float sum = 0.0f;

            for (int i =0;i< propabilities.Count;i++) {
                sum += propabilities[i];
                Ensure.ensureHard(sum <= 1.0f && sum >= 0.0f);
                if( sum > uniformDistributionValue) {
                    return i;
                }
            }

            return propabilities.Count-1;
        }

        public IList<float> propabilities = new List<float>(); // sum must be one
    }
}
