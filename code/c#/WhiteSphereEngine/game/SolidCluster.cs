using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WhiteSphereEngine.math;
using WhiteSphereEngine.physics.solid;

namespace WhiteSphereEngine.game {
    // cluster of solids, each solid has it's local position and rotation
    public class SolidCluster {
        public class SolidWithPositionAndRotation {
            public Solid solid;
            public SpatialVectorDouble localPosition;
            public SpatialVectorDouble localRotation;

            private SolidWithPositionAndRotation(){}
            public SolidWithPositionAndRotation(Solid solid, SpatialVectorDouble localPosition, SpatialVectorDouble localRotation) {
                this.solid = solid;
                this.localPosition = localPosition;
                this.localRotation = localRotation;
            }
        }

        public IList<SolidWithPositionAndRotation> solids = new List<SolidWithPositionAndRotation>();
    }
}
