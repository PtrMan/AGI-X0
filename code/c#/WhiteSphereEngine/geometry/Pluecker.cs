using WhiteSphereEngine.math;

namespace WhiteSphereEngine.geometry {
    // translation of https://github.com/PtrMan/WhiteSphereEngine/blob/ed5d4f0b98287aabdf7e5c28669533f697b04efe/src/d/geometry/Pluecker.d to C#
    struct PlueckerCoordinate {
        SpatialVectorDouble u, v; // size 3

	    public static PlueckerCoordinate createByPandQ(SpatialVectorDouble p, SpatialVectorDouble q) {
            PlueckerCoordinate result;

            result.u = p - q;
		    result.v = SpatialVectorDouble.crossProduct(p, q);

		    return result;
	    }

        // could be optimized but not wirth the time to optimize
        public static PlueckerCoordinate createByVector(SpatialVectorDouble p, SpatialVectorDouble dir) {
		    return PlueckerCoordinate.createByPandQ(p, p + dir);
	    }
	
	    // could be optimized but not wirth the time to optimize
	    public static PlueckerCoordinate createByNegativeVector(SpatialVectorDouble p, SpatialVectorDouble dir) {
		    return PlueckerCoordinate.createByPandQ(p, p - dir);
	    }

        //  see http://slidegur.com/doc/1106443/plucker-coordinate   slide 46
        // "inside" if U1.V2 + V1.U2 > 0
        static public bool checkCcw(PlueckerCoordinate c1, PlueckerCoordinate c2) {
	        return SpatialVectorDouble.dot(c1.u, c2.v) + SpatialVectorDouble.dot(c1.v, c2.u) > (double)0;
        }
    }
}
