namespace MetaNix.instrumentation {
    public class CounterInstrumentation {
        public long count;

        public void increment() {
            count++;
        }

        public void reset() {
            count = 0;
        }
    }
}
