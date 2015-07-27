package ptrman.FargGeneral;

public abstract class Codelet {
    public class RunResult {
        public boolean putback;
        
        public RunResult(boolean putback) {
            this.putback = putback;
        }
    }
    
    public abstract RunResult run();
}
