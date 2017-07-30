//using System;
//using System.Collections.Generic;

using WhiteSphereEngine.geometry;
using WhiteSphereEngine.math;
using WhiteSphereEngine.entity;
using System;
using System.Collections.Generic;

namespace WhiteSphereEngine {
    // colliders are primitives which act for detecting collisions
    // inspired by unity engine https://docs.unity3d.com/Manual/class-Rigidbody.html
    public class ColliderComponent : IComponent {
        public enum EnumType {
            BOX,
        }

        public EnumType type {
            get {
                return privateType;
            }
        }

        public bool isConvex {
            get {
                return privateType == EnumType.BOX;
            }
        }

        private ColliderComponent(EnumType type, SpatialVectorDouble localPosition, SpatialVectorDouble localRotation) {
            this.privateType = type;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
        }
        
        EnumType privateType;
        public SpatialVectorDouble localPosition;
        public SpatialVectorDouble localRotation;

        MeshWithExplicitFaces mesh; // untransformed mesh, can be null if the collider doesn't have any mesh

        TransformedMesh transformedMesh;

        public SpatialVectorDouble[] transformedVertices {
            get {
                return transformedMesh.transformedVertices;
            }
        }

        public MeshWithExplicitFaces.Face[] faces {
            get {
                return mesh.faces;
            }
        }
        

        // matrix is the position, rotation matrix of the global translation and rotation of the object to which the collider belongs to
        public void transformByMatrix(Matrix globalMatrix) {
            Matrix rotationMatrix = Matrix44.createRotationX(localRotation.x);
            rotationMatrix = rotationMatrix * Matrix44.createRotationY(localRotation.y);
            rotationMatrix = rotationMatrix * Matrix44.createRotationZ(localRotation.z);

            Matrix localMatrix = Matrix44.createIdentity();
            localMatrix = localMatrix * Matrix44.createTranslation(localPosition.x, localPosition.y, localPosition.z);
            localMatrix = localMatrix * rotationMatrix;

            transformedMesh.transformByMatrix(globalMatrix * localMatrix, mesh);
        }

        public void invalidate() {
            transformedMesh.invalidate();
        }

        public static ColliderComponent makeBox(SpatialVectorDouble size, SpatialVectorDouble localPosition, SpatialVectorDouble localRotation) {
            ColliderComponent result = new ColliderComponent(EnumType.BOX, localPosition, localRotation);
            result.mesh = PlatonicFactory.createBox(size.x, size.y, size.z);
            return result;
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
