using WhiteSphereEngine.math;

namespace WhiteSphereEngine.geometry {
    public class MeshWithExplicitFaces {
        public struct Face {
            public Face(uint[] verticesIndices) {
                this.verticesIndices = verticesIndices;
            }

            // helper
            public uint[] calcSubPolygonVertexIndices(int index) {
                uint[] arr = new uint[3];

                for( int i = 0; i < 3; i++ ) {
                    arr[i] = verticesIndices[(i + index) % verticesIndices.Length];
                }

                return arr;
            }

            public uint[] verticesIndices;
        }

        public VerticesWithAttributes verticesWithAttributes;

        // slow, accessing verticesWithAttributes directly is faster
        public SpatialVectorDouble[] getVertexPositionsAsVector4Array() {
            var positionAccessor = verticesWithAttributes.double4PositionAccessor;

            SpatialVectorDouble[] result = new SpatialVectorDouble[verticesWithAttributes.numberOfVertices];
            for(int i = 0; i < verticesWithAttributes.numberOfVertices; i++ ) {
                result[i] = new SpatialVectorDouble(positionAccessor[i]);
            }
            return result;
        }

        // slow, accessing verticesWithAttributes directly is faster
        public SpatialVectorDouble[] getVertexPositionsAsVector3Array() {
            var positionAccessor = verticesWithAttributes.double4PositionAccessor;

            SpatialVectorDouble[] result = new SpatialVectorDouble[verticesWithAttributes.numberOfVertices];
            for (int i = 0; i < verticesWithAttributes.numberOfVertices; i++) {
                result[i] = new SpatialVectorDouble(new double[]{positionAccessor[i][0], positionAccessor[i][1], positionAccessor[i][2], });
            }
            return result;
        }


        public Face[] faces;
    }

    public class MeshUtilities {
        public static SpatialVectorDouble calcPlaneByFace(MeshWithExplicitFaces mesh, uint faceIndex, int normalSideness = 1) {
            return calcPlaneByFace(mesh.faces[faceIndex], mesh.getVertexPositionsAsVector4Array(), normalSideness);
        }

        public static SpatialVectorDouble calcPlaneByFace(MeshWithExplicitFaces.Face face, SpatialVectorDouble[] vertices, int normalSideness = 1) {
            SpatialVectorDouble p0 = vertices[face.verticesIndices[0]];
            SpatialVectorDouble p1 = vertices[face.verticesIndices[1]];
            SpatialVectorDouble p2 = vertices[face.verticesIndices[2]];

            SpatialVectorDouble diff01 = p1 - p0;
            SpatialVectorDouble diff02 = p2 - p0;
            SpatialVectorDouble normal = SpatialVectorDouble.crossProduct(diff01, diff02).normalized();


            if(normalSideness == -1) {
                normal = new SpatialVectorDouble(new double[] { -normal.x, -normal.y, -normal.z });
            }

            double w = -Plane.calcW(p0, normal);
            return new SpatialVectorDouble(new double[] { normal.x, normal.y, normal.z, w }); // plane is normal,w
        }

        public static SpatialVectorDouble[] calcAllPlanes(MeshWithExplicitFaces.Face[] faces, SpatialVectorDouble[] vertices, int normalSideness = 1) {
            SpatialVectorDouble[] result = new SpatialVectorDouble[faces.Length];
            for( uint faceI = 0; faceI < faces.Length; faceI++ ) {
                result[faceI] = calcPlaneByFace(faces[faceI], vertices, normalSideness);
            }
            return result;
        }
    }
}
