package ptrman.agix0.Evolution;

import ptrman.agix0.Neuroids.Datastructures.GenerativeNeuroidNetworkDescriptor;

import java.util.List;

/**
 * Used to store the inheritage meta data in a directed acyclic graph.
 *
 * The
 */
public class Inheritage {
    public String resultOfOperationsOnParents; // description/name on the operation
    public List<String> resultOfOperationsOnParentsArguments = null;

    public int masterParentIndex = -1; // index of the parent which is the master

    public float rating;
    public GenerativeNeuroidNetworkDescriptor generativeNeuroidNetworkDescriptor = null; // can be null if the storage space should be saved
                                                                                         // can be recalculated on need from the chain of the global parents
}
