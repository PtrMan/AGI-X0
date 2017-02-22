using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaNix.dispatch;

namespace MetaNix.instrumentation {
    // searches the condidates of the next optimizations the MetaNix runtime could do
    class Statistics {
        TimingAndCountHiddenDispatchObserver timingAndCountingObserver;

        public Statistics(TimingAndCountHiddenDispatchObserver timingAndCountingObserver) {
            this.timingAndCountingObserver = timingAndCountingObserver;
        }

        public void doIt() {
            var instrumentations = timingAndCountingObserver.instrumentations;
            var orderedInstrumentations = instrumentations.OrderBy(v => v.calltimeSumInNs);

            var reportList = orderedInstrumentations.Select(v => String.Format("fnHId={0} call+ignored#={1}  sum time in ns={2}", v.functionId.value, v.callcounterPlusIgnored, v.calltimeSumInNs));
            string reportString = String.Join("\n", reportList);

            System.Console.WriteLine(reportString);
        }
    }
}
