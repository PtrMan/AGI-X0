using System.Collections.Generic;

namespace WhiteSphereEngine.physics.rigidBody {
    public class PhysicsComponentAndCollidersPair {
        private PhysicsComponentAndCollidersPair() {}
        public PhysicsComponentAndCollidersPair(PhysicsComponent physicsComponent) {
            this.physicsComponent = physicsComponent;
        }

        public PhysicsComponentAndCollidersPair(PhysicsComponent physicsComponent, ColliderComponent collider) {
            this.physicsComponent = physicsComponent;
            this.colliders = new List<ColliderComponent>() { collider };
        }

        public PhysicsComponentAndCollidersPair(PhysicsComponent physicsComponent, IList<ColliderComponent> colliders) {
            this.physicsComponent = physicsComponent;
            this.colliders = colliders;
        }

        public IList<ColliderComponent> colliders = new List<ColliderComponent>();
        public PhysicsComponent physicsComponent;
    }
}
