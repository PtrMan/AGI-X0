using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reinforcementLearning.successStoryAlgorithm {
    interface ICheckpointAction<Type> {
        void undoAllPolicyModifications(Type data);
    }
}
