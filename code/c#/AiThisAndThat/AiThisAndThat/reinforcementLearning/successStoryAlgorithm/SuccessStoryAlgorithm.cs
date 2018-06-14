using System;
using System.Collections.Generic;


namespace reinforcementLearning.successStoryAlgorithm {
    /**
     * "success story algorithm" as described in the paper "Sequential Decision Making Based on Direct Search" from Schmidhuber
     * 
     */
    class SuccessStoryAlgorithm<Type> {
        public SuccessStoryAlgorithm(ICheckpointAction<Type> checkpointAction) {
            this.checkpointAction = checkpointAction;
        }

        public void invokeCheckpoint(Checkpoint<Type> currentCheckpoint, TimeAndReward currentTimeAndReward) {
            bool successStoryCriterionSatisfiedForLastPair;
            while( !(successStoryCriterionSatisfiedForLastPair = checkSuccessStoryCriterionForLastPair(currentTimeAndReward))) {
                // undo all changes made since last recent checkpoint and remove checkpoint

                int lastIndex = checkpoints.Count - 1;
                undoAllPolicyModificationsSinceCheckpoint(checkpoints[lastIndex]);
                checkpoints.RemoveAt(lastIndex);
            }

            checkpoints.Add(currentCheckpoint);
        }

        private void undoAllPolicyModificationsSinceCheckpoint(Checkpoint<Type> checkpoint) {
            checkpointAction.undoAllPolicyModifications(checkpoint.data);
        }

        private bool checkSuccessStoryCriterionForLastPair(TimeAndReward currentTimeAndReward) {
            if (checkpoints.Count < 2) {
                return true;
            } 

            int index = checkpoints.Count - 1;
            bool isCriterionValidForThisPair = isCriterionValidForPair(checkpoints[index-1], checkpoints[index], currentTimeAndReward);
            return isCriterionValidForThisPair;
        }

        private static bool isCriterionValidForPair(Checkpoint<Type> a, Checkpoint<Type> b, TimeAndReward currentTimeAndReward) {
            double
                ratioForB = calcRatio(b, currentTimeAndReward),
                ratioForA = calcRatio(a, currentTimeAndReward);

            return ratioForB > ratioForA;
        }

        private static double calcRatio(Checkpoint<Type> checkpoint, TimeAndReward currentTimeAndReward) {
            return (currentTimeAndReward.reward - checkpoint.timeAndReward.reward) / (currentTimeAndReward.time - checkpoint.timeAndReward.time);
        }

        private ICheckpointAction<Type> checkpointAction;

        private List<Checkpoint<Type>> checkpoints = new List<Checkpoint<Type>>();
    }
}
