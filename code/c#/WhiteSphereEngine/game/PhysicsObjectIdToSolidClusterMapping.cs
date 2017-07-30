using System.Collections.Generic;

namespace WhiteSphereEngine.game {
    // mapping from physics object to solid cluster by physics object id
    public class PhysicsObjectIdToSolidClusterMapping {
        public IDictionary<ulong, SolidCluster> physicsObjectIdToSolidCluster = new Dictionary<ulong, SolidCluster>();
    }
}
