package ptrman.agix0.src.java.Neuroids.Datastructures;

/**
 *
 */
public class NeuronDescriptor {
    public boolean isEnabled;

    // input(s) -> threshold detector -> CA / state graph -> output
    public enum EnumInternalType {
        UNIFORM, // no internal state
        CA // internal CA is used as state
    }

    public EnumInternalType internalType = EnumInternalType.UNIFORM;

    public float firingThreshold = 0.5f;

    public int firingLatency;

    // internal CellularAutomata
    public int stateCaWriteinOffset = 0;
    public int stateCaReadoutOffset = 1;
    public boolean[] stateCaStartState;
    public int[] stateCaRuleIndices; // indices of the rules used for each bit
    public boolean stateCaWraparound = false;

    public NeuronDescriptor getClone() {
        NeuronDescriptor clone = new NeuronDescriptor();
        clone.isEnabled = isEnabled;
        clone.internalType = internalType;
        clone.firingThreshold = firingThreshold;
        clone.firingLatency = firingLatency;
        clone.stateCaWriteinOffset = stateCaWriteinOffset;
        clone.stateCaReadoutOffset = stateCaReadoutOffset;
        clone.stateCaStartState = stateCaStartState;

        if( stateCaRuleIndices != null ) {
            clone.stateCaRuleIndices = new int[stateCaRuleIndices.length];
            for( int i = 0; i < stateCaRuleIndices.length; i++ ) {
                clone.stateCaRuleIndices[i] = stateCaRuleIndices[i];
            }
        }
        clone.stateCaWraparound = stateCaWraparound;

        return clone;
    }
}