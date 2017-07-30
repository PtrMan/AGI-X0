namespace WhiteSphereEngine.math.control {
    // PID controller
    public class Pid {
        public class Configuration {
            public double proportial, integral, derivative;
        }

        public static Pid makeByTargetAndConfiguration(double targetValue, Configuration configuration) {
            Pid created = new Pid();
            created.targetValue = targetValue;
            created.configuration = configuration;
            return created;
        }

        public double targetValue;
        public Configuration configuration;

        public double runningIntegral = 0;
        
        public void reset(double value) {
            runningIntegral = 0;
        }
        
        public double step(double value, double dt) {
            double currentDerivative;
            return step(value, dt, out currentDerivative);
        }

        public double step(double value, double dt, out double currentDerivative) {
            double currentError = targetValue - value;
            currentDerivative = (currentError / dt);
            
            runningIntegral += (currentError * dt);
            
            return
                currentError * configuration.proportial + // P
                runningIntegral * configuration.integral + // I
                currentDerivative * configuration.derivative; // D
        }
    }
}
