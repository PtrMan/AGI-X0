namespace MetaNix.weakAi.behaviorTree.decoration {
    // ripped and and translated to C# from my gameengine
    /**
     * Repeats until a number of repetitions got done or until the child failed or a combination of both conditions.
     */
    public class Repeater : Task {
        public Task children;

        public int counterResetValue = -1;
        public int remainingCounter = 0;
        public bool repeatUntilFail = false;

        public override Task.EnumReturn run(EntityContext context) {
            Task.EnumReturn calleeReturn = children.run(context);

            if (calleeReturn == Task.EnumReturn.FAILURE && repeatUntilFail) {
                return Task.EnumReturn.SUCCESS;
            }

            if (calleeReturn != Task.EnumReturn.RUNNING && !runsInfinitly) {
                remainingCounter--;

                if (remainingCounter <= 0) {
                    return calleeReturn;
                }
            }

            return EnumReturn.FAILURE;
        }

        /** \brief resets the variables to its defaults
         *
         */
        public override void reset() {
            remainingCounter = counterResetValue;
        }

        /** \brief is cloning the object on which it got called
         *
         * \return ...
         */
        public override Task clone() {
            Repeater clone = new Repeater();
            clone.children = children.clone();
            clone.counterResetValue = counterResetValue;
            clone.remainingCounter = remainingCounter;
            clone.repeatUntilFail = repeatUntilFail;
            return clone;
        }

        public bool runsInfinitly {
            get {
                return counterResetValue == -1;
            }
        }
    }
}