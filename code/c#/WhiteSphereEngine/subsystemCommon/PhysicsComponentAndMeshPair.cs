using WhiteSphereEngine.physics.rigidBody;
using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystemCommon {
    // pair of physicsComponent and TransformedMeshComponent
    public class PhysicsComponentAndMeshPair {
        public PhysicsComponent physicsComponent;
        public TransformedMeshComponent transformedMeshComponent;

        public PhysicsComponentAndMeshPair(PhysicsComponent physicsComponent, TransformedMeshComponent transformedMeshComponent) {
            this.physicsComponent = physicsComponent;
            this.transformedMeshComponent = transformedMeshComponent;
        }

        public void transform() {
            Matrix transformationMatrix = physicsComponent.calcLocalToGlobalRotationAndTranslationMatrix();
            transformedMeshComponent.transformByMatrix(transformationMatrix);
        }
    }
}
