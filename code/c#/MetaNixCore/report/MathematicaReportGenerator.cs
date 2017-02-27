using System.Linq;
using System.Collections.Generic;

using MetaNix.resourceManagement.compute;

namespace MetaNix.report {
    public class MathematicaReportGenerator : HumanReadableReportGenerator {
        public MathematicaReportGenerator(ComputeContextResourceRecorder resourceRecorder) :base(resourceRecorder) {
            this.resourceRecorder = resourceRecorder;
        }

        public override IHumanReadableReport generate() {
            MathematicaReport report = new MathematicaReport();
            
            // TODO< rewite to !isEmpty
            if(resourceRecorder.records.Count != 0) {
                int numberOfRecords = resourceRecorder.records.Count;

                int numberOfTimesteps = resourceRecorder.records[0].recordedTaskInfosByName.Count;

                // find all unique names
                ISet<string> humanReadableUniqueNames = new HashSet<string>();
                foreach( var iRecord in resourceRecorder.records ) {
                    foreach( var iUniqueName in iRecord.recordedTaskInfosByName.Keys ) {
                        humanReadableUniqueNames.Add(iUniqueName);
                    }
                }



                // allocate descriptors by name
                IDictionary<string, PlotWithName> plotsByName = new Dictionary<string, PlotWithName>();
                foreach( string iUniqueName in humanReadableUniqueNames.AsEnumerable() ) {
                    plotsByName[iUniqueName] = new PlotWithName(iUniqueName);
                }

                // append all records
                foreach( var iRecord in resourceRecorder.records ) {
                    float normalizationFactor = normalizeBudget ? iRecord.sumOfbudgetPriorities.Value : 1.0f;

                    // we add first for all names a zero and set it to the value if it exists in iRecord

                    foreach ( string iUniqueName in humanReadableUniqueNames.AsEnumerable() ) {
                        plotsByName[iUniqueName].values.Add(0.0f);
                    }

                    foreach( string iUniqueName in iRecord.recordedTaskInfosByName.Keys ) {
                        Ensure.ensureHard(plotsByName.ContainsKey(iUniqueName));
                        plotsByName[iUniqueName].values[plotsByName[iUniqueName].values.Count - 1] = (float)iRecord.recordedTaskInfosByName[iUniqueName].budgetPriority / normalizationFactor;
                    }
                }
                
                report.createPrint("resource usage:");

                foreach( PlotWithName iPlot in plotsByName.Values ) {
                    report.pushPlot(iPlot.values, iPlot.name);
                }

                report.createPlot();
            }




            return report;
        }

        private class PlotWithName {
            public PlotWithName(string name) {
                this.name = name;
            }

            public string name;
            public IList<float> values = new List<float>();
        }

        public bool normalizeBudget = true; // is the priority of the tasks normalized before logging

        ComputeContextResourceRecorder resourceRecorder;
    }
}
