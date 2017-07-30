namespace WhiteSphereEngine.ai.command {
    // NonPreemptive implementation of the command pattern from the "Gang of four"
    // invokers and commands are preemptive which means that they process a fixed amount of work each time they are called.
    // This is necessary because we have to process a lot of invokers/commands at the same time and they may live longer than just one gameloop frame.


    // 
    // we abstract this to carry on the NonPreemptive semantics, this is not strictly necessary
    //public interface IInvoker : INonPreemptiveTask {
    //}
    
    public interface ICommand : INonPreemptiveTask {
        // is called before the first call to process() takes place to setup and prepare variables and state
        void beginExecution();
    }
}
