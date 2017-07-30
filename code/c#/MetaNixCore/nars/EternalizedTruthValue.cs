namespace MetaNix.nars {
    // Truth indicates it was the result of eternalization
    public class EternalizedTruthValue : TruthValue {
        public static EternalizedTruthValue make(float frequency, float confidence) {
            EternalizedTruthValue result = new EternalizedTruthValue();
            result.frequency = frequency;
            result.confidence = confidence;
            return result;
        }
    }
}
