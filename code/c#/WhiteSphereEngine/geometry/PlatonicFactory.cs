using System.Diagnostics;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.geometry {
    // creates basic platonic solids
    public class PlatonicFactory {
        public static MeshWithExplicitFaces createBox(double sizeX, double sizeY, double sizeZ) {
            MeshWithExplicitFaces resultMesh = new MeshWithExplicitFaces();

            double[] ppp = new double[]{ sizeX/2.0, sizeY / 2.0, sizeZ / 2.0, 1 }; uint ppp_i = 0;
            double[] npp = new double[] {-sizeX / 2.0, sizeY / 2.0, sizeZ / 2.0, 1 }; uint npp_i = 1;
            double[] pnp = new double[] { sizeX / 2.0, -sizeY / 2.0, sizeZ / 2.0, 1 }; uint pnp_i = 2;
            double[] nnp = new double[] { -sizeX / 2.0, -sizeY / 2.0, sizeZ / 2.0, 1 }; uint nnp_i = 3;

            double[] ppn = new double[] { sizeX / 2.0, sizeY / 2.0, -sizeZ / 2.0, 1 }; uint ppn_i = 4;
            double[] npn = new double[] { -sizeX / 2.0, sizeY / 2.0, -sizeZ / 2.0, 1 }; uint npn_i = 5;
            double[] pnn = new double[] { sizeX / 2.0, -sizeY / 2.0, -sizeZ / 2.0, 1 }; uint pnn_i = 6;
            double[] nnn = new double[] { -sizeX / 2.0, -sizeY / 2.0, -sizeZ / 2.0, 1 }; uint nnn_i = 7;


            { // create a VerticesWithAttributes with just positions
                MutableMeshAttribute positionMeshAttribute = MutableMeshAttribute.makeDouble4ByLength(8);
                positionMeshAttribute.getDouble4Accessor()[0] = ppp;
                positionMeshAttribute.getDouble4Accessor()[1] = npp;
                positionMeshAttribute.getDouble4Accessor()[2] = pnp;
                positionMeshAttribute.getDouble4Accessor()[3] = nnp;
                positionMeshAttribute.getDouble4Accessor()[4] = ppn;
                positionMeshAttribute.getDouble4Accessor()[5] = npn;
                positionMeshAttribute.getDouble4Accessor()[6] = pnn;
                positionMeshAttribute.getDouble4Accessor()[7] = nnn;

                VerticesWithAttributes verticesWithAttributes = new VerticesWithAttributes(new AbstractMeshAttribute[] { positionMeshAttribute }, 0);
                resultMesh.verticesWithAttributes = verticesWithAttributes;
            }

            resultMesh.faces = new MeshWithExplicitFaces.Face[] {
                new MeshWithExplicitFaces.Face(new uint[]{ppp_i, pnp_i, pnn_i, ppn_i,}), // +x face
                new MeshWithExplicitFaces.Face(new uint[]{npn_i, nnn_i, nnp_i, npp_i,}), // -x face (reversed +x)

                new MeshWithExplicitFaces.Face(new uint[]{ppp_i, ppn_i, npn_i, npp_i,}), // +y face
                new MeshWithExplicitFaces.Face(new uint[]{nnp_i, nnn_i, pnn_i, pnp_i,}), // -y face (reversed +y)
                
                new MeshWithExplicitFaces.Face(new uint[]{pnp_i, ppp_i, npp_i, nnp_i,}), // +z face 
                new MeshWithExplicitFaces.Face(new uint[]{nnn_i, npn_i, ppn_i, pnn_i,}), // -z face (reversed +z)
            };


            // check sideness
            // only required in debug build
#if DEBUG
            {
                SpatialVectorDouble[] expectedNormalsOfFaces = new SpatialVectorDouble[] {
                    new SpatialVectorDouble(new double[]{1, 0, 0}),
                    new SpatialVectorDouble(new double[]{-1, 0, 0}),
                    new SpatialVectorDouble(new double[]{0, 1, 0}),
                    new SpatialVectorDouble(new double[]{0, -1, 0}),
                    new SpatialVectorDouble(new double[]{0, 0, 1}),
                    new SpatialVectorDouble(new double[]{0, 0, -1}),
                };

                for( int faceI = 0; faceI < 6; faceI++) {
                    for (int subpolygonIndex = 0; subpolygonIndex < 4; subpolygonIndex++) {
                        uint[] subpolygonVertexIndices = resultMesh.faces[faceI].calcSubPolygonVertexIndices(subpolygonIndex);

                        SpatialVectorDouble p0 = resultMesh.getVertexPositionsAsVector4Array()[subpolygonVertexIndices[0]];
                        SpatialVectorDouble p1 = resultMesh.getVertexPositionsAsVector4Array()[subpolygonVertexIndices[1]];
                        SpatialVectorDouble p2 = resultMesh.getVertexPositionsAsVector4Array()[subpolygonVertexIndices[2]];

                        SpatialVectorDouble diff01 = p1 - p0;
                        SpatialVectorDouble diff02 = p2 - p0;
                        SpatialVectorDouble normal = SpatialVectorDouble.crossProduct(diff01, diff02).normalized();

                        Debug.Assert(System.Math.Abs(normal.x - expectedNormalsOfFaces[faceI].x) < 0.001);
                        Debug.Assert(System.Math.Abs(normal.y - expectedNormalsOfFaces[faceI].y) < 0.001);
                        Debug.Assert(System.Math.Abs(normal.z - expectedNormalsOfFaces[faceI].z) < 0.001);
                    }
                }
            }
#endif

            return resultMesh;
        }
    }
}
