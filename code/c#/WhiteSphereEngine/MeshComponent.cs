using WhiteSphereEngine.math;
using WhiteSphereEngine.geometry;
using WhiteSphereEngine.entity;
using System;
using System.Collections.Generic;

namespace WhiteSphereEngine {
    // component which holds an mesh for use in rendering and physics
    public class MeshComponent : IComponent {
        public MeshWithExplicitFaces mesh;

        public bool requiresUpdate => false;
        public void update(Entity entity) {
            // do nothing
        }

        public void entry(Entity parentEntity) {
            // do nothing
        }

        public void @event(string type, IDictionary<string, object> parameters) {
            // do nothing
        }
    }

    // used by renderer to store the transformed coordinates of an mesh
    public class TransformedMeshComponent : IComponent {
        public MeshComponent meshComponent;

        TransformedMesh transformedMesh;

        public SpatialVectorDouble[] transformedVertices {
            get {
                return transformedMesh.transformedVertices;
            }
        }
        
        public void transformByMatrix(Matrix matrix) {
            transformedMesh.transformByMatrix(matrix, meshComponent.mesh);
        }

        public void invalidate() {
            transformedMesh.invalidate();
        }

        public bool requiresUpdate => false;
        public void update(entity.Entity entity) {
            // do nothing
        }

        public void entry(Entity parentEntity) {
            // do nothing
        }

        public void @event(string type, IDictionary<string, object> parameters) {
            // do nothing
        }
    }
}
