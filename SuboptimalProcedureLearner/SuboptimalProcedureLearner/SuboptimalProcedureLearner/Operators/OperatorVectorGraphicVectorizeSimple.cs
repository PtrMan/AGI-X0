using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner.Operators
{
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
    class OperatorVectorGraphicVectorizeSimple : Operator
    {
        override public IntrospectWiringInfo introspectWiring(OperatorInstance instance, int[] inputIndices)
        {
            IntrospectWiringInfo result;

            result = new IntrospectWiringInfo();
            result.type = IntrospectWiringInfo.EnumType.NOWIRING;

            return result;
        }

        override public OperatorInstance createOperatorInstance();

        override public void setParameterOperatorInstances(OperatorInstance instance, List<OperatorInstance> parameterInstances);

        override public void initializeOperatorInstance(OperatorInstance instance)
        {
        }


        override public ExecutionResult executeSingleStep(OperatorInstance instance);

        
        override public void feedOperatorInstanceResult(OperatorInstance instance, Variadic result);



        override public void operationCleanup(OperatorInstance instance)
        {
        }

        override public bool isScaffold()
        {
            return false;
        }

        override public string getShortName()
        {
            return "OperatorVectorGraphicVectorizeSimple";
        }

        private class VectorPart
        {
            public Vector2<float> a;
            public Vector2<float> b;
        }

        private static Variadic checkArrayAndVectorize(Variadic array)
        {
            List<VectorPart> vectorParts;
            Variadic resultGraph;

            // TODO check array for being an array, containing arrays of bool which have the same length

            vectorParts = vectorizeBitmap(array);
            resultGraph = convertVectorPartsToGraph(vectorParts);
            return resultGraph;
        }

        /**
         * 
         * the array must be checked that it is a array
         * the array must be checked that the width of the bitmap is consistent
         * the values must be checked that they are boolean
         * 
         */
        private static List<VectorPart> vectorizeBitmap(Variadic array)
        {
            List<VectorPart> resultVectorParts;
            int arrayYI;
            int arraySizeX;

            resultVectorParts = new List<VectorPart>();

            System.Diagnostics.Debug.Assert(array.valueArray.Count >= 3);

            arraySizeX = array.valueArray[0].valueArray.Count;

            for( arrayYI = 1; arrayYI < array.valueArray.Count-1; arrayYI++ )
            {
                int arrayXI;

                List<Variadic> arrayYM1 = array.valueArray[arrayYI-1].valueArray;
                List<Variadic> arrayYP0 = array.valueArray[arrayYI  ].valueArray;
                List<Variadic> arrayYP1 = array.valueArray[arrayYI+1].valueArray;

                // we let catch all indexing errors and so on by the runtime

                for (arrayXI = 1; arrayXI < arrayYP0.Count - 1; arrayXI++ )
                {
                    if( !arrayYP0[arrayXI].valueBool )
                    {
                        continue;
                    }
                    // if we are here the middle pixel is true

                    int deltaX;
                    int deltaY;

                    for (deltaY = -1; deltaY < 1; deltaY++ )
                    {
                        for( deltaX = -1; deltaX < 1; deltaX++ )
                        {
                            if( deltaX == 0 && deltaY == 0 )
                            {
                                continue;
                            }
                            // else we are here

                            if( array.valueArray[arrayYI+deltaY].valueArray[arrayXI+deltaX].valueBool )
                            {
                                resultVectorParts.Add(getVectorpartForPositionPlusDelta(arrayXI, arrayYI, deltaX, deltaY));
                            }
                        }
                    }
                }
            }

            return resultVectorParts;
        }

        private static VectorPart getVectorpartForPositionPlusDelta(int positionX, int positionY, int deltaX, int deltaY)
        {
            VectorPart resultVectorPart;

            resultVectorPart = new VectorPart();
            resultVectorPart.a = new Vector2<float>(positionX, positionY);
            resultVectorPart.b = new Vector2<float>(positionX + deltaX, positionY + deltaY);
            
            return resultVectorPart;
        }

        /**
         * converts the vector graphic vectorParts (only one edge)
         * to a variadic graph, which is compatible with the rest of the system
         * 
         */
        private static Variadic convertVectorPartsToGraph(List<VectorPart> vectorParts)
        {
            Variadic resultGraph;
            int vertexCounter;

            resultGraph = new Variadic(Variadic.EnumType.ARRAY);
            resultGraph.valueArray.Add(new Variadic(Variadic.EnumType.ARRAY));
            resultGraph.valueArray.Add(new Variadic(Variadic.EnumType.ARRAY));
            resultGraph.valueArray.Add(new Variadic(Variadic.EnumType.ARRAY));

            List<Variadic> arrayWithEdges = resultGraph.valueArray[0].valueArray;
            List<Variadic> arrayWithVertices = resultGraph.valueArray[1].valueArray;

            vertexCounter = 0;

            foreach( VectorPart iterationVectorPart in vectorParts )
            {
                List<Variadic> createdEdgeArray;
                Variadic createdEdgeVariadic;
                Variadic createdVertexA;
                Variadic createdVertexB;

                // add the edge
                createdEdgeArray = new List<Variadic>();
                createdEdgeArray.Add(new Variadic(Variadic.EnumType.INT));
                createdEdgeArray.Add(new Variadic(Variadic.EnumType.INT));

                createdEdgeArray[0].valueInt = vertexCounter;
                createdEdgeArray[0].valueInt = vertexCounter+1;

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
}
