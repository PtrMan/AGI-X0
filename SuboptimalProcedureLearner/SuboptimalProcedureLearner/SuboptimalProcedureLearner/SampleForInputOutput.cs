using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    /**
     * Is a "training"-example for the correct input/output pair of a function which should be modelled or is modelled
     * 
     */
    class SampleForInputOutput
    {
        // we only have it implemented for integers for now
        // TODO< combined datatypes etc >

        public List<Variadic> input;
        public List<Variadic> output;
    }
}
