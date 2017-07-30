using WhiteSphereEngine.math;

namespace WhiteSphereEngine.physics.rigidBody {
    public struct CollisionInformation {
        public SpatialVectorDouble globalPosition;
        public SpatialVectorDouble globalNormal;
        public PhysicsComponent a, b; // collision partners

        public bool aIsParticle;
    }

    public enum EnumParticleCollisionResponse {
        REFLECT,
        ABSORB,
        SCATTER, // let the particle go into the object
        DELETEA,

        NEUTRAL, // doesn't influence the reaction, if all handlers return NEUTRAL it reflects
    }

    public interface ICollisionHandler {
        // normal is normalized
        void beginContact(ref CollisionInformation collisionInformation);

        // normal is normalized
        void endContact(ref CollisionInformation collisionInformation);

        void instantaniousContact(ref CollisionInformation collisionInformation);

        // for the case when no begin/end information is accesible or if the engine is configured this way that it doesn't take track of begin/end of a contact
        void instantaniousParticleContact(ref CollisionInformation collisionInformation, out EnumParticleCollisionResponse particleResponse);
    }
}
