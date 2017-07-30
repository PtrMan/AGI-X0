namespace MetaNix.nars.config {
    // https://github.com/opennars/opennars/blob/1.6.5_devel17_RetrospectiveAnticipation/nars_core/nars/config/Parameters.java
    public static class Parameters {
        /** determines the internal precision used for TruthValue calculations.
         *  Change at your own risk
         */
        public const float TRUTH_EPSILON = 0.01f;

        public static bool DEBUG_BAG = true;
        public static bool DEBUG = true;

        /** 
           Cycles per duration.
           Past/future tense usage convention;
           How far away "past" and "future" is from "now", in cycles.         
           The range of "now" is [-DURATION/2, +DURATION/2];      */
        public static uint DURATION = 5;


        //FIELDS BELOW ARE BEING CONVERTED TO DYNAMIC, NO MORE STATIC: ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //
        //Pei comments: parameters will be separated into a dynamic group and a static group
        //              and the latter contains "personality parameters" that cannot be changed
        //              in the lifetime of the system, though different systems may take different
        //              values. For example, to change HORIZON dynamically will cause inconsistency 
        //              in evidence evaluation.



        /* ---------- default input values ---------- */
        /** Default expectation for confirmation. */
        public static float DEFAULT_CONFIRMATION_EXPECTATION = 0.6f;
        /** Default expectation for creation of concept. */
        public static float DEFAULT_CREATION_EXPECTATION = 0.66f; //0.66
                                                                              /** Default expectation for creation of concept for goals. */
        public static float DEFAULT_CREATION_EXPECTATION_GOAL = 0.6f; //0.66
                                                                                  /** Default confidence of input judgment. */
        public static float DEFAULT_JUDGMENT_CONFIDENCE = 0.9f;
        /** Default priority of input judgment */
        public static float DEFAULT_JUDGMENT_PRIORITY = 0.8f;
        /** Default durability of input judgment */
        public static float DEFAULT_JUDGMENT_DURABILITY = 0.5f; //was 0.8 in 1.5.5; 0.5 after
                                                                      /** Default priority of input question */
        public static float DEFAULT_QUESTION_PRIORITY = 0.9f;
        /** Default durability of input question */
        public static float DEFAULT_QUESTION_DURABILITY = 0.9f;

        /** Size of ConceptBag and level amount */
        public static uint CONCEPT_BAG_SIZE = 10; // 10 for testing
        public static uint CONCEPT_BAG_LEVELS = 1000;
        /** Size of TaskLinkBag */
        public static uint TASK_LINK_BAG_SIZE = 100;  //was 200 in new experiment
        public static uint TASK_LINK_BAG_LEVELS = 10;
        /** Size of TermLinkBag */
        public static uint TERM_LINK_BAG_SIZE = 100;  //was 1000 in new experiment
        public static uint TERM_LINK_BAG_LEVELS = 10;
        /** Size of Novel Task Buffer */
        public static uint NOVEL_TASK_BAG_SIZE = 100;
        public static uint NOVEL_TASK_BAG_LEVELS = 10;



        public static uint TERM_LINK_RECORD_LENGTH = 10;        /** Record-length for newly created TermLink's */

        /* ---------- logical parameters ---------- */
        /** Evidential Horizon, the amount of future evidence to be considered. 
         * Must be >=1.0, usually 1 .. 2
         */
        public static float HORIZON = 1;

        /** Level separation in LevelBag, one digit, for display (run-time adjustable) and management (fixed)
         */
        public static float BAG_THRESHOLD = 1.0f;

        /** The budget threshold rate for task to be accepted. */
        public static float BUDGET_THRESHOLD = 0.01f;

        public static float HAPPY_EVENT_HIGHER_THRESHOLD = 0.75f;
        public static float HAPPY_EVENT_LOWER_THRESHOLD = 0.25f;

        public static float DERIVATION_PRIORITY_LEAK = 0.4f; // https://groups.google.com/forum/#!topic/open-nars/y0XDrs2dTVs
        public static float DERIVATION_DURABILITY_LEAK = 0.4f; // https://groups.google.com/forum/#!topic/open-nars/y0XDrs2dTVs

        public static float COMPLEXITY_UNIT = 1.0f; // 1.0 - oo

        
        public static uint CONCEPT_BELIEFS_MAX = 28; /** Maximum number of beliefs kept in a Concept */
        public static uint CONCEPT_QUESTIONS_MAX = 5; /** Maximum number of questions kept in a Concept */

        /** Maximum TermLinks checked for novelty for each TaskLink in TermLinkBag */
        public static uint TERM_LINK_MAX_MATCHED = 10;

        /** (see its use in budgetfunctions iterative forgetting) */
        public static float FORGET_QUALITY_RELATIVE = 0.1f;


        public static uint TERMLINK_MAX_REASONED = 3; /** Maximum TermLinks used in reasoning for each Task in Concept */
        
        
        public static uint STAMP_BLOOMFILTERNUMBEROFBITS = 256;
        public static uint STAMP_NUMBEROFELEMENTS = 40;

        public static float SATISFACTION_TRESHOLD = 0.0f; // decision threshold is enough for now

        /** what this value represents was originally equal to the termlink record length (10), but we may want to adjust it or make it scaled according to duration since it has more to do with time than # of records.  it can probably be increased several times larger since each item should remain in the recording queue for longer than 1 cycle */
        public static uint NOVELTY_HORIZON = 10;


        public static float BUSY_EVENT_HIGHER_THRESHOLD = 0.9f;
        public static float BUSY_EVENT_LOWER_THRESHOLD = 0.1f;

        // NOTE SQUARE< this comment doesn't make any sense to me >
        // OpenNARS : temporary parameter for setting #threads to use, globally
        public static bool IMMEDIATE_ETERNALIZATION = true;

        public static uint MAXIMAL_TERM_COMPLEXITY = 50;

        public static uint TASKLINK_PER_CONTENT = 4; //eternal/event are also seen extra
    }
}
