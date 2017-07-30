using WhiteSphereEngine.geometry;
using WhiteSphereEngine.math;

namespace WhiteSphereEngine.physics.intersection {
    class RayPolyhedron {
        // from https://github.com/erich666/GraphicsGems/blob/master/gemsii/RayCPhdron.c

        private const double HUGE_VAL = 1.7976931348623157e+308;

        /* return codes */
        public enum EnumReturnCode {
            MISSED = 0,
            FRONTFACE = 1,
            BACKFACE = -1
        }
        
        /* Ray-Convex Polyhedron Intersection Test by Eric Haines, erich@eye.com
         *
         * This test checks the ray against each face of a polyhedron, checking whether
         * the set of intersection points found for each ray-plane intersection
         * overlaps the previous intersection results.  If there is no overlap (i.e.
         * no line segment along the ray that is inside the polyhedron), then the
         * ray misses and returns 0; else 1 is returned if the ray is entering the
         * polyhedron, -1 if the ray originates inside the polyhedron.  If there is
         * an intersection, the distance and the normal of the face hit is returned.
         */

        public static EnumReturnCode rayCvxPolyhedronInt(
            SpatialVectorDouble org,     /* origin and direction of ray */
            SpatialVectorDouble dir, 
            double tmax,                 /* maximum useful distance along ray */
            SpatialVectorDouble[] phdrn, /* list of planes in convex polyhedron */ // x,y,z, w
            out double tresult,          /* returned: distance of intersection along ray */
            out SpatialVectorDouble? norm, /* returned: normal of face hit */

            out int? fnorm_num,
            out int? bnorm_num
        ) {
            SpatialVectorDouble pln;            /* plane equation */ // x,y,z, w
            double tnear, tfar, t, vn, vd;
            fnorm_num = null; bnorm_num = null;   /* front/back face # hit */

            tresult = double.NaN;
            norm = null;

            tnear = -HUGE_VAL;
            tfar = tmax;

            /* Test each plane in polyhedron */
            for( int plnIndex = phdrn.Length - 1; plnIndex >= 0; plnIndex--) {
                pln = phdrn[plnIndex];

	            /* Compute intersection point T and sidedness */
	            vd = Plane.dot3(dir, pln);
                vn = Plane.dot3(org, pln) + pln.w;
	            if (vd == 0.0 ) {
	                /* ray is parallel to plane - check if ray origin is inside plane's
	                   half-space */
	                if (vn > 0.0 )
		                /* ray origin is outside half-space */
		                return EnumReturnCode.MISSED;
	            } else {
	                /* ray not parallel - get distance to plane */
	                t = -vn / vd ;
	                if (vd< 0.0 ) {
		                /* front face - T is a near point */
		                if (t > tfar ) return EnumReturnCode.MISSED ;
		                if (t > tnear ) {
		                    /* hit near face, update normal */
		                    fnorm_num = plnIndex;
		                    tnear = t;
		                }
	                } else {
		                /* back face - T is a far point */
		                if (t<tnear ) return EnumReturnCode.MISSED;
		                if (t<tfar ) {
		                    /* hit far face, update normal */
		                    bnorm_num = plnIndex;
		                    tfar = t;
		                }
	                }
	            }
            }

            /* survived all tests */
            /* Note: if ray originates on polyhedron, may want to change 0.0 to some
             * epsilon to avoid intersecting the originating face.
             */
            if (tnear >= 0.0 ) {
                /* outside, hitting front face */
                norm = phdrn[fnorm_num.Value];

                tresult = tnear;
	            return EnumReturnCode.FRONTFACE;
                } else {
	            if (tfar<tmax ) {
                    /* inside, hitting back face */
                    norm = phdrn[bnorm_num.Value];

                    tresult = tfar;
	                return EnumReturnCode.BACKFACE;
	            } else {
	                /* inside, but back face beyond tmax */
	                return EnumReturnCode.MISSED;
	            }
            }
        }
        
    }
}
