using System.Collections.Generic;

namespace MetaNix.schmidhuber.powerplay {
    // used to build task from a enumerable description
    public interface ITaskBuilder {
        ITask buildFromVector(IList<uint> descriptionInstructions);
    }

    public enum EnumSimulationStepResult {
        NOTFINISHED, // simulation can do another iteration
        FINISHED,
        ERROR, // some kind of (not hard) internal error
    }

    public interface ITask {
        // does an iteration of some internal simulation
        void simulationIteration(float[] input, ref float[] onlineOutput, out EnumSimulationStepResult simulationStepResult);

        // resets the state of the task, can for example be a robot position or the position of objects or a retina
        void resetState();

        // the length of onlineOutput of simulationIteration()
        uint lengthOfOnlineOutput {
            get;
        }

        // called at the end of the run of a task
        bool wasTaskSolved {
            get;
        }
    }
}
