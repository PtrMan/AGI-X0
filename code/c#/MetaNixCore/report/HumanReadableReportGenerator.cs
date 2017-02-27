using MetaNix.resourceManagement.compute;

namespace MetaNix.report {
    abstract public class HumanReadableReportGenerator {
        public HumanReadableReportGenerator(ComputeContextResourceRecorder resourceRecorder) {
            this.resourceRecorder = resourceRecorder;
        }

        public abstract IHumanReadableReport generate();

        ComputeContextResourceRecorder resourceRecorder;
    }
}
