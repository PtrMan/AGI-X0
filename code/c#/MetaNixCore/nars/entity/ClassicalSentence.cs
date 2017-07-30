
using MetaNix.nars.inference;

namespace MetaNix.nars.entity {
    // https://github.com/opennars/opennars/blob/1.6.5_devel17_TonyAnticipationStrategy/nars_core/nars/entity/Sentence.java
    /**
     * A Sentence is an abstract class, mainly containing a Term, a TruthValue, and
     * a Stamp.
     * 
     * It is used as the premises and conclusions of all inference rules.
     */
    public class ClassicalSentence {
        static ClassicalSentence makeJudgement(TermOrCompoundTermOrVariableReferer term, TruthValue truth) {
            ClassicalSentence result = new ClassicalSentence(EnumPunctation.JUDGMENT);
            result.term = term;
            result.truth = truth;
            return result;
        }
        
        public class MakeByTermPunctuationTruthStampNormalizeParameters {
            public TermOrCompoundTermOrVariableReferer term;
            public EnumPunctation punctation;
            public TruthValue truth;
            public Stamp stamp;
            public bool normalize = true;
        }

        // TODO< transfer all the nonsense of OpenNARS to here >
        public static ClassicalSentence makeByTermPunctuationTruthStampNormalize(MakeByTermPunctuationTruthStampNormalizeParameters parameters) {
            ClassicalSentence created = new ClassicalSentence(parameters.punctation);
            created.term = parameters.term;
            created.truth = parameters.truth;
            created.stamp = parameters.stamp;
            return created;
        }

        public ClassicalSentence(EnumPunctation punctation) {
            this.punctation = punctation;
        }

        public Stamp stamp; // Partial record of the derivation path


        public TermOrCompoundTermOrVariableReferer term; // the content of a sentence is a term

        public readonly EnumPunctation punctation;

        public TruthValue truth; // The truth value of Judgment, or desire value of Goal
        public bool producedByTemporalInduction = false;


        // TODO< Stamp stamp > // Partial record of the derivation path


        // called punctation for classical NARS reasons
        public enum EnumPunctation {
            JUDGMENT,
            QUESTION,
            GOAL,
            QUEST,
        }

        // TODO TODO TODO< overload comparision whic does roughtly the same as in openNARS >


        public bool isJudgment { get {
            return punctation == EnumPunctation.JUDGMENT;
        }}

        public bool isQuestion { get {
            return punctation == EnumPunctation.QUESTION;
        }}

        public bool isGoal { get {
            return punctation == EnumPunctation.GOAL;
        }}

        public bool isQuest { get {
            return punctation == EnumPunctation.QUEST;
        }}

        public bool equalsContent(ClassicalSentence s2) {
            return TermOrCompoundTermOrVariableReferer.isSameWithId(term, s2.term);
        }

        /**
         * Check whether the judgment is equivalent to another one
         * <p>
         * The two may have different keys
         *
         * \param that The other judgment
         * \return Whether the two are equivalent
         */
        public bool checkEquivalentTo(ClassicalSentence that) {
            /*
            if (Parameters.DEBUG) {
                if ((!term.equals(term)) || (punctuation != that.punctuation)) {
                    throw new RuntimeException("invalid comparison for Sentence.equivalentTo");
                }
            }*/
            bool isStampEqual = Stamp.checkEquals(
                stamp,
                that.stamp,
                Stamp.EnumCompareCreationTime.NO,
                Stamp.EnumCompareOccurrenceTime.YES,
                Stamp.EnumCompareEvidentialBaseTime.YES);
            return truth.checkEquals(that.truth) && isStampEqual;
        }

        /**
          * project a judgment to a difference occurrence time
          *
          * \param targetTime The time to be projected into
          * \param currentTime The current time as a reference
          * \return The projected belief
          */
        public ClassicalSentence projection(long targetTime, long currentTime) {

            TruthValue newTruth = projectionTruth(targetTime, currentTime);

            bool eternalizing = newTruth is EternalizedTruthValue;

            Stamp newStamp = eternalizing ? stamp.cloneWithNewOccurrenceTime(Stamp.ETERNAL) :
                                            stamp.cloneWithNewOccurrenceTime(targetTime);

            MakeByTermPunctuationTruthStampNormalizeParameters parameters = new MakeByTermPunctuationTruthStampNormalizeParameters();
            parameters.term = term;
            parameters.punctation = punctation;
            parameters.truth = newTruth;
            parameters.stamp = newStamp;
            parameters.normalize = false;
            return makeByTermPunctuationTruthStampNormalize(parameters);
        }


        public TruthValue projectionTruth(long targetTime, long currentTime) {
            TruthValue newTruth = null;

            if (!stamp.isEternal) {
                newTruth = TruthFunctions.eternalize(truth);
                if (targetTime != Stamp.ETERNAL) {
                    long occurrenceTime = stamp.occurrenceTime;
                    double factor = TruthFunctions.temporalProjection(occurrenceTime, targetTime, currentTime);
                    float projectedConfidence = (float)( factor * truth.confidence );
                    if (projectedConfidence > newTruth.confidence) {
                        newTruth = TruthValue.make(truth.frequency, projectedConfidence);
                    }
                }
            }

            if (newTruth == null) newTruth = truth.clone();

            return newTruth;
        }
        
    }
}
