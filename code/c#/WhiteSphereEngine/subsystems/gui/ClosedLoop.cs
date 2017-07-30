using System.Collections.Generic;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.gui {
    /** \brief A closed loop of points
     *
     * All points are connected with lines in between and there is a line from the last point to the first
     *
     * This class doesn't define how it is drawn, the width, if it can be convex and so on
     *
     */
    public class ClosedLoop {
        public IList<SpatialVectorDouble> points = new List<SpatialVectorDouble>(); // 2d points
    }
}
