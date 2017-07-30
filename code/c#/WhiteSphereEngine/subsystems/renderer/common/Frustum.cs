using System;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.renderer.common {
    public class FrustumSphere {
	    public SpatialVectorDouble position;
	    public double radius;
    }

    public class FrustumPlane {
	    public enum EnumSideOfPlane {
            FRONT,
            BEHIND
        }

        public SpatialVectorDouble normal;
	    public double distance;

        public static FrustumPlane makeFrom4Component(SpatialVectorDouble plane) {
            FrustumPlane made = new FrustumPlane();
            made.normal = SpatialVectorUtilities.toVector3(plane);
            made.distance = plane.w;
            return made;
        }

        public EnumSideOfPlane getSideOfPlane(SpatialVectorDouble position) {
		    // fast version, doesn't check for equality
		    if(Frustum.planePointDistance(normal, distance, position) > 0.0 ) {
			    return EnumSideOfPlane.FRONT;
		    }
		    return EnumSideOfPlane.BEHIND;
	    }

	    public static FrustumPlane createByPointOnPlaneAndNormal(SpatialVectorDouble position, SpatialVectorDouble normal) {
		    FrustumPlane resultPlane = new FrustumPlane();
            resultPlane.normal = normal.deepClone();
		    resultPlane.distance = -SpatialVectorDouble.dot(normal, position);
		    return resultPlane;
	    }
    }

    public class FrustumAabb {
        public SpatialVectorDouble[] vertices {
            get {
                // TODO< calculate AABB vertices from referenced kdop >

                SpatialVectorDouble temp = new SpatialVectorDouble(new double[] { 0, 0, 1 });

                return new SpatialVectorDouble[] {
                    temp, temp, temp, temp,
                    temp, temp, temp, temp,
                };

                //throw new NotImplementedException();

                //SpatialVector!(3, Type)[8] result;

                // TODO TODO TODO

                //return result;
                //return null;
            }
        }

        public bool containsPoint(SpatialVectorDouble point) {
            throw new NotImplementedException();

            // TODO TODO TODO
            //return false;
        }
    }
    
    public class Frustum {
        public enum EnumFrustumIntersectionResult {
            OUTSIDE, // object to test is fully outside of the frustum
            INTERSECT, // object to test intersects with frustum
            INSIDE,
        }

        public static double planePointDistance(SpatialVectorDouble planeNormal, double planeD, SpatialVectorDouble point) {
	        return SpatialVectorDouble.dot(planeNormal, point) + planeD;
        }


        // tests if a sphere is inside the frustum
        public EnumFrustumIntersectionResult calcContainsForSphere(FrustumSphere sphere) {
            for( int i = 0; i < planes.Length; i++) {
                // find the distance to this plane
                double distance = SpatialVectorDouble.dot(planes[i].normal, sphere.position) + planes[i].distance;

                if (distance < -sphere.radius) {
                    return EnumFrustumIntersectionResult.OUTSIDE;
                }

                if (System.Math.Abs(distance) < sphere.radius) {
                    return EnumFrustumIntersectionResult.INTERSECT;
                }
            }

            return EnumFrustumIntersectionResult.INSIDE;
        }

        // tests if a AABB is within the frustrum
        public  EnumFrustumIntersectionResult calcContainsForAabb(FrustumAabb aabb) {
            uint totalInCounter = 0;

            // get the corners of the box into the cornerVertices array
            SpatialVectorDouble[] cornerVertices = aabb.vertices;

            // test all 8 corners against the 6 sides 
            // if all points are behind 1 specific plane, we are out
            // if we are in with all points, then we are fully in
            for( int p = 0; p < planes.Length; p++) {
                uint pointInfrontCounter = 8;
                uint inIncrementForPlane = 1;

                foreach( SpatialVectorDouble iterationCornerVertex in cornerVertices ) {
                    // test this point against the planes
                    if (planes[p].getSideOfPlane(iterationCornerVertex) == FrustumPlane.EnumSideOfPlane.BEHIND) {
                        inIncrementForPlane = 0;
                        --pointInfrontCounter;
                    }
                }

                // were all the points outside of plane p?
                if (pointInfrontCounter == 0) {
                    return EnumFrustumIntersectionResult.OUTSIDE;
                }

                // check if they were all on the right side of the plane
                totalInCounter += inIncrementForPlane;
            }


            // so if totalInCounter is 6, then all are inside the view
            if (totalInCounter == 6) {
                return EnumFrustumIntersectionResult.INSIDE;
            }

            // we must be partly in then otherwise
            return EnumFrustumIntersectionResult.INTERSECT;
        }

        public FrustumPlane[] planes;
    }
}
