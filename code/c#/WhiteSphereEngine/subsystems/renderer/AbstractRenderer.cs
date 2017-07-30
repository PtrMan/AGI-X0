using System;
using System.Collections.Generic;

using WhiteSphereEngine.subsystemCommon;

namespace WhiteSphereEngine.subsystems.renderer {
    public abstract class AbstractRenderer {
        public IList<PhysicsComponentAndMeshPair> physicsAndMeshPairs = new List<PhysicsComponentAndMeshPair>();

        public void render() {
            transformAllObjects();
            renderCore();
        }

        abstract protected void renderCore();

        void transformAllObjects() {
            // TODO PERFORMANCE< parallel for >
            foreach (PhysicsComponentAndMeshPair iPhysicsComponentAndMesh in physicsAndMeshPairs) {
                iPhysicsComponentAndMesh.transform();
            }
        }
    }
}
