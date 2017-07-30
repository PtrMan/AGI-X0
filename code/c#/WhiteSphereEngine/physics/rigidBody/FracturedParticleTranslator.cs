using System.Collections.Generic;

using WhiteSphereEngine.physics.microTimestepSimulation;

namespace WhiteSphereEngine.physics.rigidBody {
    // translates(spawns) fractured particles in the physics simulation
    public class FracturedParticleTranslator {
        private FracturedParticleTranslator(){} // disable construction because it's an helper

        // positions and velocities must be transfed to global space!
        public static void spawnParticlesToPhysicsEngine(PhysicsEngine engine, IList<FracturedParticle> fracturedParticles) {
            foreach( FracturedParticle iFracturedParticle in fracturedParticles) {
                PhysicsComponent physicsComponentOfParticle = engine.createPhysicsComponent(iFracturedParticle.relativePosition, iFracturedParticle.relativeVelocity, iFracturedParticle.composition.mass, null);
                engine.addParticle(physicsComponentOfParticle);
            }
        }
    }
}
