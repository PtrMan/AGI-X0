using System;
using System.Collections.Generic;
//using System.Linq;

using WhiteSphereEngine.math;
using WhiteSphereEngine.physics.solid;
using WhiteSphereEngine.geometry;

namespace WhiteSphereEngine.physics.microTimestepSimulation {
    // simple and primitive fracturing system
    // used to calculate the remains of explosions
    public class SimpleFracturing {
        // useful for acceleration with radiation or a shockwave
        public static void accelerateByRadialPressure(SpatialVectorDouble energySourcePosition, double energyInJoules, IList<FracturedParticle> particles) {
            foreach( FracturedParticle iParticle in particles ) {
                SpatialVectorDouble diff = iParticle.relativePosition - energySourcePosition;
                double distance = diff.length;

                if ( distance > 0.0 ) {
                    double surfaceAreaOfRadiationSphereAtDistance = Area.ofSphere(distance);

                    double surfaceAreaOfProjectedSphere = ProjectedArea.ofSphere(iParticle.radius);

                    double absorbedEnergyRatio = System.Math.Min(surfaceAreaOfProjectedSphere / surfaceAreaOfRadiationSphereAtDistance, 1.0);
                    double absolvedEnergyInJoules = absorbedEnergyRatio * energyInJoules;
                    iParticle.accelerateByEnergyInJoules(diff.normalized(), absolvedEnergyInJoules);
                }
            }
        }

        public static IList<FracturedParticle> fractureSolid(Solid solid, uint roughtlyNumberOfFracturedElements) {
            IList<FracturedParticle> fracturedResult = new List<FracturedParticle>();

            if( solid.shapeType == Solid.EnumShapeType.BOX ) {
                // we first calculate the length of each segment
                double lengthOfSegmentX, lengthOfSegmentY, lengthOfSegmentZ;
                {
                    // ASSUMPTION for this we assume that the cube is roughtly symetrically in all directions and we take as the sidelength the maximum
                    double sideLength = math.Math.max(solid.size.x, solid.size.y, solid.size.y);

                    lengthOfSegmentX = sideLength / math.Math.sqrt3(roughtlyNumberOfFracturedElements);
                    lengthOfSegmentY = sideLength / math.Math.sqrt3(roughtlyNumberOfFracturedElements);
                    lengthOfSegmentZ = sideLength / math.Math.sqrt3(roughtlyNumberOfFracturedElements);
                }
                
                uint numberOfElementsX = (uint)(solid.size.x / lengthOfSegmentX);
                uint numberOfElementsY = (uint)(solid.size.y / lengthOfSegmentY);
                uint numberOfElementsZ = (uint)(solid.size.z / lengthOfSegmentZ);
                
                uint numberOfFracturedElements = numberOfElementsX * numberOfElementsY * numberOfElementsZ;

                double fracturedParticlesRadius = math.Math.min(solid.size.x / (double)numberOfElementsX, solid.size.y / (double)numberOfElementsY, solid.size.z / (double)numberOfElementsZ);

                for ( uint x = 0; x < numberOfElementsX; x++) {
                    for (uint y = 0; y < numberOfElementsY; y++) {
                        for (uint z = 0; z < numberOfElementsZ; z++) {
                            double relativeX = (double)x / (double)(numberOfElementsX - 1);
                            double relativeY = (double)y / (double)(numberOfElementsY - 1);
                            double relativeZ = (double)z / (double)(numberOfElementsZ - 1);

                            double relativeXm1p1 = relativeX * 2.0 - 1.0;
                            double relativeYm1p1 = relativeY * 2.0 - 1.0;
                            double relativeZm1p1 = relativeZ * 2.0 - 1.0;

                            double positionX = 0.0; // relativeXm1p1 * solid.size.x * 0.5;
                            double positionY = 0.0; // relativeYm1p1 * solid.size.y * 0.5;
                            double positionZ = relativeZm1p1 * solid.size.z * 0.5;

                            fracturedResult.Add(new FracturedParticle(new SpatialVectorDouble(new double[]{positionX, positionY, positionZ, }), solid.composition.getPartByRatio(1.0 / (double)numberOfFracturedElements), fracturedParticlesRadius));
                        }
                    }
                }
            }
            else {
                throw new NotImplementedException();
            }

            return fracturedResult;
        }
    }

    // assumed to be a sphere
    public class FracturedParticle {
        public double radius;
        public SpatialVectorDouble relativePosition;
        public Composition composition;
        public SpatialVectorDouble relativeVelocity = new SpatialVectorDouble(new double[]{0,0,0});
        
        private FracturedParticle(){}
        public FracturedParticle(SpatialVectorDouble relativePosition, Composition composition, double radius) {
            this.relativePosition = relativePosition;
            this.composition = composition;
            this.radius = radius;
        }

        public void accelerateByEnergyInJoules(SpatialVectorDouble normal, double absolvedEnergyInJoules) {
            // PHYSICS TO CHECK ASSUMPTION< we just use e = 1/2 * m * v², because v can be relative >
            // see http://www.dummies.com/education/science/physics/how-to-calculate-the-kinetic-energy-of-an-object/
            double vDelta = System.Math.Sqrt(absolvedEnergyInJoules * 2 / composition.mass);
            relativeVelocity += normal.scale(vDelta);
        }
    }
}
