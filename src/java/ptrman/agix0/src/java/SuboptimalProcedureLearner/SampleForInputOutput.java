package ptrman.agix0.src.java.SuboptimalProcedureLearner;

import ptrman.Datastructures.Variadic;

import java.util.ArrayList;
import java.util.List;

/**
 * Is a "training"-example for the correct input/output pair of a function which should be modelled or is modelled
 *
 */
public class SampleForInputOutput {
    // we only have it implemented for integers for now
    // TODO< combined datatypes etc >
    public List<Variadic> input = new ArrayList<>();
    public List<Variadic> output = new ArrayList<>();
}
