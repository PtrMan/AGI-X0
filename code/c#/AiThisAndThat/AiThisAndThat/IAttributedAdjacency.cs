using System;
using System.Collections.Generic;

interface IAdjacency<Datatype> {
    void allocate(uint sourceIndices, uint destinationIndices);

    List<Tuple<uint, Datatype>> getDestinationsBySource(uint sourceIndex);

    void addSourceDestinationsUnion(uint sourceIndex, List<Tuple<uint, Datatype>> destinationIndicesWithData);
    void addDestinationSourceUnion(uint destinationIndex, List<Tuple<uint, Datatype>> sourceIndicesWithData);
}

