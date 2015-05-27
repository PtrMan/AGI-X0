package SuboptimalProcedureLearner.Operators;

/**
     * 
     * see drawing
     * 
     * operator which converts a bitmap image (given in as a parameter as a array of arrays of bools) to vectors
     * (represented as a graph)
     * the coordinate are after whole pixels (1 element equals one pixel)
     * 
     * TODO
     * split this up into two operators?
     * 
     */
public class OperatorVectorGraphicVectorizeSimple {
}
// uncommented because currently unnecessary and untested
/*
public class OperatorVectorGraphicVectorizeSimple  extends Operator 
{
    public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices) throws Exception {
        IntrospectWiringInfo result = new IntrospectWiringInfo();
        result = new IntrospectWiringInfo();
        result.type = IntrospectWiringInfo.EnumType.NOWIRING;
        return result;
    }

    public OperatorInstance createOperatorInstance() throws Exception ;

    public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances) throws Exception ;

    public void initializeOperatorInstance(OperatorInstance instance) throws Exception {
    }

    public ExecutionResult executeSingleStep(OperatorInstance instance) throws Exception ;

    public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result) throws Exception ;

    public void operationCleanup(OperatorInstance instance) throws Exception {
    }

    public boolean isScaffold() throws Exception {
        return false;
    }

    public String getShortName() throws Exception {
        return "OperatorVectorGraphicVectorizeSimple";
    }

    private static class VectorPart   
    {
        public Vector2<float> a = new Vector2<float>();
        public Vector2<float> b = new Vector2<float>();
    }

    private static Variadic checkArrayAndVectorize(Variadic array) throws Exception {
        List<VectorPart> vectorParts = new List<VectorPart>();
        Variadic resultGraph = new Variadic();
        // TODO check array for being an array, containing arrays of bool which have the same length
        vectorParts = vectorizeBitmap(array);
        resultGraph = convertVectorPartsToGraph(vectorParts);
        return resultGraph;
    }

    **
             * 
             * the array must be checked that it is a array
             * the array must be checked that the width of the bitmap is consistent
             * the values must be checked that they are boolean
             * 
             *
    private static List<VectorPart> vectorizeBitmap(Variadic array) throws Exception {
        List<VectorPart> resultVectorParts = new List<VectorPart>();
        int arrayYI = new int();
        int arraySizeX = new int();
        resultVectorParts = new List<VectorPart>();
        System.Diagnostics.Debug.Assert(array.valueArray.Count >= 3);
        arraySizeX = array.valueArray[0].valueArray.Count;
        for (arrayYI = 1;arrayYI < array.valueArray.Count - 1;arrayYI++)
        {
            int arrayXI = new int();
            List<Variadic> arrayYM1 = array.valueArray[arrayYI - 1].valueArray;
            List<Variadic> arrayYP0 = array.valueArray[arrayYI].valueArray;
            List<Variadic> arrayYP1 = array.valueArray[arrayYI + 1].valueArray;
            for (arrayXI = 1;arrayXI < arrayYP0.Count - 1;arrayXI++)
            {
                // we let catch all indexing errors and so on by the runtime
                if (!arrayYP0[arrayXI].valueBool)
                {
                    continue;
                }
                 
                // if we are here the middle pixel is true
                int deltaX = new int();
                int deltaY = new int();
                for (deltaY = -1;deltaY < 1;deltaY++)
                {
                    for (deltaX = -1;deltaX < 1;deltaX++)
                    {
                        if (deltaX == 0 && deltaY == 0)
                        {
                            continue;
                        }
                         
                        // else we are here
                        if (array.valueArray[arrayYI + deltaY].valueArray[arrayXI + deltaX].valueBool)
                        {
                            resultVectorParts.Add(getVectorpartForPositionPlusDelta(arrayXI,arrayYI,deltaX,deltaY));
                        }
                         
                    }
                }
            }
        }
        return resultVectorParts;
    }

    private static VectorPart getVectorpartForPositionPlusDelta(int positionX, int positionY, int deltaX, int deltaY) throws Exception {
        VectorPart resultVectorPart;
        resultVectorPart = new VectorPart();
        resultVectorPart.a = new Vector2<float>(positionX, positionY);
        resultVectorPart.b = new Vector2<float>(positionX + deltaX, positionY + deltaY);
        return resultVectorPart;
    }

    **
             * converts the vector graphic vectorParts (only one edge)
             * to a variadic graph, which is compatible with the rest of the system
             * 
             *
    private static Variadic convertVectorPartsToGraph(List<VectorPart> vectorParts) throws Exception {
        Variadic resultGraph = new Variadic();
        int vertexCounter = new int();
        resultGraph = new Variadic(Variadic.EnumType.ARRAY);
        resultGraph.valueArray.Add(new Variadic(Variadic.EnumType.ARRAY));
        resultGraph.valueArray.Add(new Variadic(Variadic.EnumType.ARRAY));
        resultGraph.valueArray.Add(new Variadic(Variadic.EnumType.ARRAY));
        List<Variadic> arrayWithEdges = resultGraph.valueArray[0].valueArray;
        List<Variadic> arrayWithVertices = resultGraph.valueArray[1].valueArray;
        vertexCounter = 0;
        for (Object __dummyForeachVar0 : vectorParts)
        {
            VectorPart iterationVectorPart = (VectorPart)__dummyForeachVar0;
            List<Variadic> createdEdgeArray = new List<Variadic>();
            Variadic createdEdgeVariadic = new Variadic();
            Variadic createdVertexA = new Variadic();
            Variadic createdVertexB = new Variadic();
            // add the edge
            createdEdgeArray = new List<Variadic>();
            createdEdgeArray.Add(new Variadic(Variadic.EnumType.INT));
            createdEdgeArray.Add(new Variadic(Variadic.EnumType.INT));
            createdEdgeArray[0].valueInt = vertexCounter;
            createdEdgeArray[0].valueInt = vertexCounter + 1;
            createdEdgeVariadic = new Variadic(Variadic.EnumType.ARRAY);
            createdEdgeVariadic.valueArray = createdEdgeArray;
            arrayWithEdges.Add(createdEdgeVariadic);
            // add the vertices
            createdVertexA = new Variadic(Variadic.EnumType.ARRAY);
            createdVertexA.valueArray.Add(new Variadic(Variadic.EnumType.FLOAT));
            createdVertexA.valueArray.Add(new Variadic(Variadic.EnumType.FLOAT));
            createdVertexA.valueArray[0].valueFloat = iterationVectorPart.a.x;
            createdVertexA.valueArray[1].valueFloat = iterationVectorPart.a.y;
            createdVertexB = new Variadic(Variadic.EnumType.ARRAY);
            createdVertexB.valueArray.Add(new Variadic(Variadic.EnumType.FLOAT));
            createdVertexB.valueArray.Add(new Variadic(Variadic.EnumType.FLOAT));
            createdVertexB.valueArray[0].valueFloat = iterationVectorPart.b.x;
            createdVertexB.valueArray[1].valueFloat = iterationVectorPart.b.y;
            arrayWithVertices.Add(createdVertexA);
            arrayWithVertices.Add(createdVertexB);
            vertexCounter += 2;
        }
        return resultGraph;
    }

}

*/
