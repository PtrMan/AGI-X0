using System.Drawing;

using WhiteSphereEngine.math;
using WhiteSphereEngine.subsystemCommon;
using WhiteSphereEngine.geometry;
using WhiteSphereEngine.subsystems.renderer.common;
using WhiteSphereEngine.physics.rigidBody;

namespace WhiteSphereEngine.subsystems.renderer.prototypeFormRenderer {
    public class SoftwareRenderer : AbstractRenderer {
        public Graphics graphics;
        public Pen pen;

        public Matrix modelViewProjection;

        public int viewSize = 300;

        Frustum frustum = new Frustum();

        bool activateFrustumCulling = true; // desable for debugging frustum culling problems

        protected override void renderCore() {
            recalcFrustum();

            foreach ( PhysicsComponentAndMeshPair iterationPhysicsAndMeshPair in physicsAndMeshPairs) {
                if(isPhysicsComponentCulled(iterationPhysicsAndMeshPair.physicsComponent)) {
                    continue;
                }
                
                renderMesh(iterationPhysicsAndMeshPair.transformedMeshComponent);
            }
        }

        void recalcFrustum() {
            // TODO< transform view frustom mesh by rotation of the camera >

            // we transform a mesh which describes the view frustum in global space
            // and then we calculate the normal and distance of the normal of the planes
            MeshWithExplicitFaces frustumMesh = new MeshWithExplicitFaces();

            { // create a VerticesWithAttributes with just positions
                MutableMeshAttribute positionMeshAttribute = MutableMeshAttribute.makeDouble4ByLength(5);
                var positionAccessor = positionMeshAttribute.getDouble4Accessor();
                positionAccessor[0] = new double[] { -1, -1, 1, 1 };
                positionAccessor[1] = new double[] { 1, -1, 1, 1 };
                positionAccessor[2] = new double[] { 1, 1, 1, 1 };
                positionAccessor[3] = new double[] { -1, 1, 1, 1 };
                positionAccessor[4] = new double[] { 0, 0, 0, 1 };

                VerticesWithAttributes verticesWithAttributes = new VerticesWithAttributes(new AbstractMeshAttribute[] { positionMeshAttribute }, 0);
                frustumMesh.verticesWithAttributes = verticesWithAttributes;
            }
            
            frustumMesh.faces = new MeshWithExplicitFaces.Face[] {
                new MeshWithExplicitFaces.Face(new uint[]{4, 0, 1}), // top side
                new MeshWithExplicitFaces.Face(new uint[]{4, 1, 2}), // right side
                new MeshWithExplicitFaces.Face(new uint[]{4, 2, 3}), // bottom side
                new MeshWithExplicitFaces.Face(new uint[]{4, 3, 0}), // left side
            };

            // TODO< use globally transformed vertices >
            // result is 4 component
            SpatialVectorDouble[] planesWith4Component = MeshUtilities.calcAllPlanes(frustumMesh.faces, frustumMesh.getVertexPositionsAsVector3Array());


            
            // we only have 4 planes because we dont limit the maximal z
            frustum.planes = new FrustumPlane[4];
            frustum.planes[0] = FrustumPlane.makeFrom4Component(planesWith4Component[0]);
            frustum.planes[1] = FrustumPlane.makeFrom4Component(planesWith4Component[1]);
            frustum.planes[2] = FrustumPlane.makeFrom4Component(planesWith4Component[2]);
            frustum.planes[3] = FrustumPlane.makeFrom4Component(planesWith4Component[3]);
        }

        bool isPhysicsComponentCulled(PhysicsComponent @object) {
            if( !activateFrustumCulling) {
                return false;
            }

            // TODO< pull bvh and reference FrustumAABB from it >
            FrustumAabb frustumAabb = new FrustumAabb();

            return frustum.calcContainsForAabb(frustumAabb) == Frustum.EnumFrustumIntersectionResult.OUTSIDE;
        }

        private void renderMesh(TransformedMeshComponent transformedMeshComponent) {
            foreach (MeshWithExplicitFaces.Face iterationFace in transformedMeshComponent.meshComponent.mesh.faces) {
                for (int edgeI = 0; edgeI < iterationFace.verticesIndices.Length - 1; edgeI++) {
                    int vertexIndex0 = (int)iterationFace.verticesIndices[edgeI];
                    int vertexIndex1 = (int)iterationFace.verticesIndices[edgeI + 1];

                    SpatialVectorDouble vertex0 = translateTo4ComponentVector(transformedMeshComponent.transformedVertices[vertexIndex0]);
                    SpatialVectorDouble vertex1 = translateTo4ComponentVector(transformedMeshComponent.transformedVertices[vertexIndex1]);
                    renderEdge(vertex0, vertex1);
                }

                {
                    int vertexIndex0 = (int)iterationFace.verticesIndices[iterationFace.verticesIndices.Length - 1];
                    int vertexIndex1 = (int)iterationFace.verticesIndices[0];

                    SpatialVectorDouble vertex0 = translateTo4ComponentVector(transformedMeshComponent.transformedVertices[vertexIndex0]);
                    SpatialVectorDouble vertex1 = translateTo4ComponentVector(transformedMeshComponent.transformedVertices[vertexIndex1]);
                    renderEdge(vertex0, vertex1);
                }
            }

        }

        private void renderEdge(SpatialVectorDouble vertex0, SpatialVectorDouble vertex1) {
            Matrix point0 = new Matrix(new double[] { vertex0.x, vertex0.y, vertex0.z, vertex0.w }, 1);
            Matrix point1 = new Matrix(new double[] { vertex1.x, vertex1.y, vertex1.z, vertex1.w }, 1);

            Matrix projectedPoint0 = modelViewProjection * point0;
            Matrix projectedPoint1 = modelViewProjection * point1;

            Matrix normalizedProjectedPoint0 = SoftwareRendererUtilities.project(projectedPoint0);
            Matrix normalizedProjectedPoint1 = SoftwareRendererUtilities.project(projectedPoint1);

            graphics.DrawLine(pen,
                viewSize / 2 + (int)normalizedProjectedPoint0[0, 0], viewSize / 2 + (int)normalizedProjectedPoint0[1, 0],
                viewSize / 2 + (int)normalizedProjectedPoint1[0, 0], viewSize / 2 + (int)normalizedProjectedPoint1[1, 0]);
        }

        // helper
        static SpatialVectorDouble translateTo4ComponentVector(SpatialVectorDouble value) {
            return new SpatialVectorDouble(new double[] { value.x, value.y, value.z, 1 });
        }
    }
}
