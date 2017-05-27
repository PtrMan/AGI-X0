using System.Collections.Generic;

namespace MetaNix.weakAi.behaviorTree {
    // ripped and and translated to C# from my gameengine
    public class Sequence : Task {
        public IList<Task> children = new List<Task>();

        public bool infinite {
            set {
                privateInfinite = value;
            }
        }
        
        public override Task.EnumReturn run(EntityContext context) {
            if (children.Count == 0) {
                // uncommented because its old code, and we want to still have the information errorMessage = "this.Childrens does have a length of 0!";
                // uncommented because its old code, and we want to still have the information errorDepth = 0;
                return Task.EnumReturn.FAILURE;
            }

            for (;;) {
                if (currentIndex >= children.Count) {
                    currentIndex = 0;

                    if (!privateInfinite) {
                        return Task.EnumReturn.SUCCESS;
                    }
                }

                Task.EnumReturn calleeReturn = children[(int)currentIndex].run(context);
                switch (calleeReturn) {
				case Task.EnumReturn.SUCCESS: break;
				case Task.EnumReturn.FAILURE: return Task.EnumReturn.FAILURE;
				case Task.EnumReturn.RUNNING: return Task.EnumReturn.RUNNING;
                }

                currentIndex++;
            }
        }

        public override void reset() {
            currentIndex = 0;

            foreach (Task iterationChildren in children ) {
                iterationChildren.reset();
            }
        }

        public override Task clone() {
            Sequence clonedSequence = new Sequence();
            clonedSequence.currentIndex = currentIndex;
            clonedSequence.privateInfinite = privateInfinite;

            foreach (Task iterationChildren in children ) {
                clonedSequence.children.Add( iterationChildren.clone() );
            }

            return clonedSequence;
        }

        private uint currentIndex = 0;
        private bool privateInfinite = false;
    }
}
