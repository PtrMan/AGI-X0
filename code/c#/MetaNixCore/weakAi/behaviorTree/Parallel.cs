using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaNix.weakAi.behaviorTree {
    // http://guineashots.com/2014/08/10/an-introduction-to-behavior-trees-part-2/
    class Parallel : Task {
        public override Task clone() {
            Parallel cloned = new Parallel();
            cloned.maxCounterFailure = maxCounterFailure;
            cloned.maxCounterSuccess = maxCounterSuccess;
            foreach (Task iterationChildren in children) {
                cloned.children.Add(iterationChildren.clone());
            }
            return cloned;
        }

        public override void reset() {
        }

        public override EnumReturn run(EntityContext context) {
            throw new NotImplementedException();

            uint counterSuccess = 0;
            uint counterFailure = 0;

            foreach(Task iterationChildren in children) {
                EnumReturn result = iterationChildren.run(context);
                if( result == EnumReturn.SUCCESS ) {
                    counterSuccess++;
                }
                else if( result == EnumReturn.FAILURE ) {
                    counterFailure++;
                }
            }

            if(counterSuccess >= maxCounterSuccess ) {
                return EnumReturn.SUCCESS;
            }
            else if( counterFailure >= maxCounterFailure ) {
                return EnumReturn.FAILURE;
            }

            return EnumReturn.RUNNING;
        }

        public uint maxCounterSuccess, maxCounterFailure;

        public IList<Task> children = new List<Task>();
    }
}
