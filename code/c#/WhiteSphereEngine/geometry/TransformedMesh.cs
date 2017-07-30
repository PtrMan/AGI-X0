using System.Diagnostics;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.geometry {
    // helper for transformable meshes
    struct TransformedMesh {
        SpatialVectorDouble[] privateTransformedVertices;
        bool valid;

        public SpatialVectorDouble[] transformedVertices {
            get {
                Trace.Assert(valid);
                Debug.Assert(privateTransformedVertices != null);
                return privateTransformedVertices;
            }
        }

        public void transformByMatrix(Matrix matrix, MeshWithExplicitFaces mesh) {
            var positionAccessor = mesh.verticesWithAttributes.double4PositionAccessor;

            bool isInvalidated = privateTransformedVertices == null || privateTransformedVertices.Length != mesh.verticesWithAttributes.numberOfVertices;
            if (isInvalidated) {
                privateTransformedVertices = new SpatialVectorDouble[mesh.verticesWithAttributes.numberOfVertices];
            }

            for (int i = 0; i < mesh.verticesWithAttributes.numberOfVertices; i++) {
                privateTransformedVertices[i] = SpatialVectorUtilities.toVector3(new SpatialVectorDouble(matrix * new Matrix(positionAccessor[i], 1)));
            }

            valid = true;
        }

        public void invalidate() {
            valid = false;
        }
    }
}
