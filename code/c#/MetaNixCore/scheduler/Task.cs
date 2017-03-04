namespace MetaNix.scheduler {
    public enum EnumTaskStates {
        FINISHED, // this will be returned if the task did termintate itself
        RUNNING, // this will be returned if the task should be restarted in this frame
        WAITNEXTFRAME // this will be returned if this task should be executed again in the next frame
    }

    public interface ITask {
        // TODO< get required absolute time ?? >

        // \param softTimelimitInSeconds how many seconds should the task run ideally
        void processTask(Scheduler scheduler, double softTimelimitInSeconds, out EnumTaskStates taskState);
    }
}
