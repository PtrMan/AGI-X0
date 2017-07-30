using System;
using System.Collections.Generic;
using System.Diagnostics;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.physics.boundingVolume {
    // k-Discretely Oriented Polytopes
    public class KDop {
        public double[] min, max;

        // \param baseVectorsStartWithAabb does the baseVector array start with the base vectors of the AABB. Is used for faster boudning volume tests
        public KDop(SpatialVectorDouble[] baseVectors, uint k, bool baseVectorsStartWithAabb) {
            Debug.Assert((k % 2) == 0);
            min = new double[k / 2];
            max = new double[k / 2];

            for(int i = 0; i < min.Length; i++) {
                min[i] = double.PositiveInfinity;
                max[i] = double.NegativeInfinity;
            }

            privateBaseVectors = baseVectors;
            privateBaseVectorsStartWithAabb = baseVectorsStartWithAabb;
        }

        public bool baseVectorsStartWithAabb {
            get {
                return privateBaseVectorsStartWithAabb;
            }
        }

        public uint k {
            get {
                Debug.Assert(min.Length == max.Length);
                return (uint)min.Length * 2;
            }
        }

        // from http://stackoverflow.com/questions/2168055/k-dop-collision-between-different-k-and-volumes
        public static bool checkIntersect(KDop a, KDop b) {
            Debug.Assert(a.k == b.k);

            for (int i = 0; i < a.k / 2; i++) {
                if ((a.min[i] > b.max[i]) || (a.max[i] < b.min[i]))
                    return false;
            }
            return true;
        }

        // based on http://stackoverflow.com/questions/2168055/k-dop-collision-between-different-k-and-volumes
        public static bool checkIntersectAabb(KDop other, KDop aabb) {
            Debug.Assert(other.baseVectorsStartWithAabb);
            Debug.Assert(aabb.baseVectorsStartWithAabb); // must be AABB

            for (int i = 0; i < 3; i++) { // we just have to check the first 3 base vectors
                if ((other.min[i] > aabb.max[i]) || (other.max[i] < aabb.min[i]))
                    return false;
            }
            return true;
        }

        public bool checkIntersectPosition(SpatialVectorDouble position) {
            for (int i = 0; i < k / 2; i++) {
                double dotOfBaseVectorWithPosition = SpatialVectorDouble.dot(baseVectors[i], position);
                if( min[i] > dotOfBaseVectorWithPosition || max[i] < dotOfBaseVectorWithPosition ) {
                    return false;
                }
            }
            return true;
        }

        public static KDop calculateKdopFromVertices(IList<SpatialVectorDouble> vertices, uint k) {
            SpatialVectorDouble[] baseVectors;

            bool baseVectorsStartWithAabb;

            if (k == 6) { // AABB
                baseVectors = BASEVECTORSOFKDOP6;
                baseVectorsStartWithAabb = true;
            }
            else if ( k == 14 ) {
                baseVectors = BASEVECTORSOFKDOP14;
                baseVectorsStartWithAabb = true;
            }
            else {
                throw new ArgumentException();
            }

            return calculateKdopFromVerticesAndbaseVectors(vertices, baseVectors, baseVectorsStartWithAabb);
        }
        
        public static KDop calculateKdopFromVerticesAndbaseVectors(IList<SpatialVectorDouble> vertices, SpatialVectorDouble[] baseVectors, bool baseVectorsStartWithAabb) {
            uint k = (uint)baseVectors.Length * 2;

            KDop result = new KDop(baseVectors, k, baseVectorsStartWithAabb);

            foreach (SpatialVectorDouble iterationVertex in vertices) {
                for (int baseVectorI = 0; baseVectorI < baseVectors.Length; baseVectorI++) {
                    double dotWithIterationVector = SpatialVectorDouble.dot(iterationVertex, baseVectors[baseVectorI]);
                    result.min[baseVectorI] = System.Math.Min(result.min[baseVectorI], dotWithIterationVector);
                    result.max[baseVectorI] = System.Math.Max(result.max[baseVectorI], dotWithIterationVector);
                }
            }

            return result;
        }

        public static SpatialVectorDouble[] BASEVECTORSOFKDOP6 = new SpatialVectorDouble[]{
            new SpatialVectorDouble(new double[]{1, 0, 0}),
            new SpatialVectorDouble(new double[]{0, 1, 0}),
            new SpatialVectorDouble(new double[]{0, 0, 1})
        };

        public static SpatialVectorDouble[] BASEVECTORSOFKDOP14 = new SpatialVectorDouble[]{
            // start with AABB
            new SpatialVectorDouble(new double[]{1, 0, 0}),
            new SpatialVectorDouble(new double[]{0, 1, 0}),
            new SpatialVectorDouble(new double[]{0, 0, 1}),

            new SpatialVectorDouble(new double[]{Constants.DIVSQRT2, Constants.DIVSQRT2, Constants.DIVSQRT2}),
            new SpatialVectorDouble(new double[]{-Constants.DIVSQRT2, Constants.DIVSQRT2, Constants.DIVSQRT2}),
            new SpatialVectorDouble(new double[]{Constants.DIVSQRT2, -Constants.DIVSQRT2, Constants.DIVSQRT2}),
            new SpatialVectorDouble(new double[]{Constants.DIVSQRT2, Constants.DIVSQRT2, -Constants.DIVSQRT2})
        };

        public SpatialVectorDouble[] baseVectors {
            get {
                return privateBaseVectors;
            }
        }

        SpatialVectorDouble[] privateBaseVectors;
        private bool privateBaseVectorsStartWithAabb;

    }

    public class KDopUtilities {
        private KDopUtilities(){}

        public static KDop makeAabbKDopByCenterAndRadius(SpatialVectorDouble center, double radius) {
            Debug.Assert(radius > 0.0);

            KDop result = new KDop(KDop.BASEVECTORSOFKDOP6, 6, true);
            result.min[0] = center.x - radius;
            result.min[1] = center.y - radius;
            result.min[2] = center.z - radius;
            result.max[0] = center.x + radius;
            result.max[1] = center.y + radius;
            result.max[2] = center.z + radius;

            return result;
        }
    }
}
