using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1 {
    // see https://www.youtube.com/watch?v=XIavUJ848Mk
    // 21:00 core algorithm

    /* uncommented because we are in local crap mode
    public class Gjk {
        // calculate the simplex which is cloest to the origin
        static IList<SpatialVectorDouble> calcSimplex(IList<SpatialVectorDouble> input, SpatialVectorDouble directionToOrigin, out SpatialVectorDouble newSearchDirection) {
            SpatialVectorDouble o = directionToOrigin;

            if( input.Length == 4 ) {
                // we determine just which face is closest to the origin and then jump to the 3-point implementation
                SpatialVectorDouble
                    d = input[3],
                    c = input[2],
                    b = input[1],
                    a = input[0];

                IList<SpatialVectorDouble>[] candidatePlanes = new IList<SpatialVectorDouble>[]{
                    new List<SpatialVectorDouble>(){c, b, a},
                    new List<SpatialVectorDouble>(){d, b, a},
                    new List<SpatialVectorDouble>(){d, c, b},
                    new List<SpatialVectorDouble>(){d, c, a},
                };

                double minDistance = double.MaxValue;
                IList<SpatialVectorDouble> minCandidatePlane = null;

                foreach( var candidatePlane in candidatePlanes ) {
                    double distanceOfPlane = distanceOfPlaneToOrigin(candidatePlane[0], candidatePlane[1], candidatePlane[2]);
                    if( distanceOfPlane < minDistance ) {
                        minDistance = distanceOfPlane;
                        minCandidatePlane = candidatePlane;
                    }
                }

                return calcSimplex(minCandidatePlane, directionToOrigin, out newSearchDirection);
            }
            else if (input.Length == 3) {
                SpatialVectorDouble
                    c = input[2],
                    b = input[1],
                    a = input[0],

                    // in the video he calls the normal ABC, we call it normal
                    normal = calcNormal(a, b, c),

                    ac = c - a,
                    ao = o - a,
                    ab = b - a;

                if (SpatialVectorDouble.dot(n, ac) > 0.0) {
                    if( SpatialVectorDouble.dot(ac, o) > 0.0 ) {
                        newSearchDirection = cross3(ac, ao, ac);
                        return new IList<SpatialVectorDouble>(){a, c};
                    }
                    else {
                        goto caseStar;
                    }
                }
                else {
                    if (SpatialVectorDouble.dot(SpatialVectorDouble.cross(ab, n), o) > 0.0) {
                        goto caseStar;
                    }
                    else {
                        if (SpatialVectorDouble.dot(n, o) > 0.0) {
                            newSearchDirection = n;
                            return new IList<SpatialVectorDouble>(){a, b, c};
                        }
                        else {
                            newSearchDirection = -n;
                            return new IList<SpatialVectorDouble>(){a, c, b};
                        }
                    }
                }

                throw new Exception("Not reachable!");
                // comon case
                caseStar:
                if (SpatialVectorDouble.dot(ab, o) > 0.0) {
                    newSearchDirection = cross3(ab, ao, ab);
                    return new IList<SpatialVectorDouble>(){a, b};
                }
                else {
                    newSearchDirection = ao;
                    return new IList<SpatialVectorDouble>(){a};
                }
            }
            else if (input.Length == 2) {
                SpatialVectorDouble
                    a = input[0], // last added point
                    b = input[1];
                
                if (SpatialVectorDouble.dot(b - a, o) > 0.0) {
                    newSearchDirection = cross3(b - a, o, b - a);
                    return input;
                }
                else {
                    newSearchDirection = o - a;
                    return new IList<SpatialVectorDouble>(){input[0]};
                }
            }
            else { // (input.Length < 2 || input.Length > 4)
                // invalid, should never happen
                throw new Exception();
            }
        }

        // helper function to calculate the cross product(s) of three vectors
        static SpatialVectorDouble cross3(SpatialVectorDouble a, SpatialVectorDouble b, SpatialVectorDouble c) {
            return SpatialVectorDouble.cross( SpatialVectorDouble.cross(a, b), c);
        }

        static SpatialVectorDouble calcNormal(SpatialVectorDouble a, SpatialVectorDouble b, SpatialVectorDouble c) {
            SpatialVectorDouble
                ab = b-a,
                bc = c-b;

            return SpatialVectorDouble.cross(ab, bc);
        }
    }
     * */
}
