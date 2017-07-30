namespace WhiteSphereEngine.ai.command {
    public enum EnumNonPreemptiveTaskState {
        FINISHEDSUCCESSFUL,
        INPROGRESS,
        ERROR,
    }

    // non preemptive task which has to give up the control to the caller after doing some work
    // see https://en.wikipedia.org/wiki/Cooperative_multitasking
    public interface INonPreemptiveTask {
        EnumNonPreemptiveTaskState process();
    }
}
