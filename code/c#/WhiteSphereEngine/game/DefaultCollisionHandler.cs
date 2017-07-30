using WhiteSphereEngine.physics.rigidBody;

namespace WhiteSphereEngine.game {
    // collision handler for testing
    class DefaultCollisionHandler : ICollisionHandler {
        public void beginContact(ref CollisionInformation collisionInformation) {
        }

        public void endContact(ref CollisionInformation collisionInformation) {
        }

        public void instantaniousContact(ref CollisionInformation collisionInformation) {
        }

        public void instantaniousParticleContact(ref CollisionInformation collisionInformation, out EnumParticleCollisionResponse particleResponse) {
            particleResponse = EnumParticleCollisionResponse.ABSORB;
        }
    }
}
