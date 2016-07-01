using System;
using System.Collections.Generic;
//using System.Linq;

// calculates an adjacency matrix for all nodes, fills in all sources a path can come from
class Tracer<Datatype> {
    public static TableAdjacency<CausalAttribute> traceAll(TableAdjacency<CausalAttribute> inputTable, List<uint> entrySourceIndices) {
        TableAdjacency<CausalAttribute> resultTable; // TODO = inputTable.clone();

        // TODO
        /*
        foreach( uint entrySourceIndex in entrySourceIndices) {
            List<Tuple<uint, CausalAttribute>> sourcePath = new List<Tuple<uint, CausalAttribute>> {new Tuple<uint, CausalAttribute>(0, entrySourceIndex) };

            traceRecursive(sourcePath, resultTable, inputTable);
        }
        */

        return null;//resultTable;
    }

    private static void traceRecursive(List<Tuple<uint, CausalAttribute>> sourcePath, TableAdjacency<CausalAttribute> resultTable, TableAdjacency<CausalAttribute> inputTable) {
        uint currentSourceIndex = sourcePath[sourcePath.Count - 1].Item1;

        resultTable.addDestinationSourceUnion(currentSourceIndex, sourcePath);

        foreach(Tuple<uint, CausalAttribute> iterationDestination in inputTable.getDestinationsBySource(currentSourceIndex)) {
            List<Tuple<uint, CausalAttribute>> sourcePathForCallee = copyTupleList(sourcePath);
            
            // ASK< is adding the sourcedepth here correct? >
            sourcePathForCallee.Add(new Tuple<uint, CausalAttribute>(iterationDestination.Item1+1, iterationDestination.Item2));

            traceRecursive(sourcePathForCallee, resultTable, inputTable);
        }
    }

    private static List<Tuple<uint, CausalAttribute>> copyTupleList(List<Tuple<uint, CausalAttribute>> source) {
        List<Tuple<uint, CausalAttribute>> result = new List<Tuple<uint, CausalAttribute>>();
        foreach(Tuple<uint, CausalAttribute> iteration in source) {
            result.Add(new Tuple<uint, CausalAttribute>(iteration.Item1, iteration.Item2));
        }
        return result;
    }
}

