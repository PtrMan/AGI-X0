using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaNix.report {
    public class MathematicaReport : IHumanReadableReport {
        IList<Tuple<IList<float>,string>> plotStack = new List<Tuple<IList<float>, string>>();

        string reportString;

        public string getContentAsString() {
            string result = reportString;
            reportString = "";
            return result;
        }

        // for testing public
        public void pushPlot(IList<float> list, string labeling) {
            plotStack.Add(new Tuple<IList<float>, string>(list, labeling));
        }

        // for testing public
        public void createPlot() {
            string formated = String.Format("ListLinePlot[{{{0}}},PlotLegends->{{{1}}},DataRange -> {{0, {2}}},ClippingStyle -> Red]\n", String.Join(",", plotStack.Select(iPlot => "{" + String.Join(",", iPlot.Item1.Select(v => v.ToString("0.########", System.Globalization.CultureInfo.InvariantCulture))) + "}")), String.Join(",", plotStack.Select(iPlot => String.Format("\"{0}\"", iPlot.Item2))), plotStack[0].Item1.Count-1);
            reportString += formated;

            plotStack.Clear();
        }

        public void createPrint(string text) {
            reportString += String.Format("Print[\"{0}\"]\n", text);
        }
    }
}