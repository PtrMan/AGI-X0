using System.Collections.Generic;

using MetaNix.nars.entity;

using static MetaNix.nars.control.DerivationContext;

namespace MetaNix.nars.config {
    /**
     * NAR Parameters which can be changed during runtime.
     */
    public class RuntimeParameters {
        
        /** Silent threshold for task reporting, in [0, 100]. 
         *  Noise level = 100 - silence level; noise 0 = always silent, noise 100 = never silent
         */
        public volatile int noiseLevel = 100;

        /** 
           Cycles per duration.
           Past/future tense usage convention;
           How far away "past" and "future" is from "now", in cycles.         
           The range of "now" is [-DURATION/2, +DURATION/2];      */
        public ClassicalDuration duration = new ClassicalDuration(Parameters.DURATION);

        /** Concept decay rate in ConceptBag, in [1, 99].  originally: CONCEPT_FORGETTING_CYCLE 
         *  How many cycles it takes an item to decay completely to a threshold value (ex: 0.1).
         *  Lower means faster rate of decay.
         */
        public double conceptForgetDurations = 2.0;

        /** TermLink decay rate in TermLinkBag, in [1, 99]. originally: TERM_LINK_FORGETTING_CYCLE */
        public double termLinkForgetDurations = 10.0;

        /** TaskLink decay rate in TaskLinkBag, in [1, 99]. originally: TASK_LINK_FORGETTING_CYCLE */
        public double taskLinkForgetDurations = 4.0;

        /** Sequence bag forget durations **/
        public double sequenceForgetDurations = 4.0;

        /** novel task bag forget duration **/
        public double novelTaskForgetDurations = 2.0;


        /** Minimum expectation for a desire value. 
         *  the range of "now" is [-DURATION, DURATION]; */
        public double decisionThreshold = 0.51;


        //    //let NARS use NARS+ ideas (counting etc.)
        //    public final AtomicBoolean experimentalNarsPlus = new AtomicBoolean();
        //
        //    //let NARS use NAL9 operators to perceive its own mental actions
        //    public final AtomicBoolean internalExperience = new AtomicBoolean();

        //these two are AND-coupled:
        //when a concept is important and exceeds a syntactic complexity, let NARS name it: 
        //public final AtomicInteger abbreviationMinComplexity = new AtomicInteger();
        //public final AtomicDouble abbreviationMinQuality = new AtomicDouble();



        public IList<DerivationFilter> defaultDerivationFilters = new List<DerivationFilter>();
        public IList<DerivationFilter> getDerivationFilters() {
            return defaultDerivationFilters;
        }
    }
}
