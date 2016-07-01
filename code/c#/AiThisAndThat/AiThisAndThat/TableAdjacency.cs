using System;
using System.Collections.Generic;


class TableAdjacency<Datatype> : IAdjacency<Datatype> {
    public void addSourceDestinationsUnion(uint sourceIndex, List<Tuple<uint, Datatype>> destinationIndicesWithData) {
        foreach(Tuple<uint, Datatype> iterationDestinationIndicesWithData in destinationIndicesWithData) {
            table[iterationDestinationIndicesWithData.Item1, sourceIndex] = iterationDestinationIndicesWithData.Item2;
        }
    }

    public void addDestinationSourceUnion(uint destinationIndex, List<Tuple<uint, Datatype>> sourceIndicesWithData) {
        foreach (Tuple<uint, Datatype> iterationSourceIndicesWithData in sourceIndicesWithData) {
            table[destinationIndex, iterationSourceIndicesWithData.Item1] = iterationSourceIndicesWithData.Item2;
        }
    }

    public void allocate(uint sourceIndices, uint destinationIndices) {
        table = new Datatype[destinationIndices, sourceIndices];
    }

    public List<Tuple<uint, Datatype>> getDestinationsBySource(uint sourceIndex) {
        List<Tuple<uint, Datatype>> result = new List<Tuple<uint, Datatype>>();

        uint destinationCount = getDestinationCount();
        
        for( uint destinationI = 0; destinationI < destinationCount; destinationI++) {
            if( table[destinationI, sourceIndex] != null ) {
                result.Add(new Tuple<uint, Datatype>(destinationI, table[destinationI, sourceIndex]));
            }
        }

        return result;
    }

    protected uint getSourceCount() {
        return (uint)table.GetLength(1);
    }

    protected uint getDestinationCount() {
        return (uint)table.GetLength(0);
    }
    


    // [destination, source]
    protected Datatype[,] table;
}

