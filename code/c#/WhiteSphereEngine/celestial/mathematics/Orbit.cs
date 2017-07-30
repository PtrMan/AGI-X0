namespace WhiteSphereEngine.celestial.mathematics {
    public class Orbit {
        public static double calcVelocityOfCircularOrbit(double centerMass, double radius) {
            return calcVelocityOfCenteredEllipticalOrbit(centerMass, radius, radius);
        }

        public static double calcVelocityOfCenteredEllipticalOrbit(double centerMass, double lengthOfSemimajorAxis, double radius) {
            // https://en.wikipedia.org/wiki/Orbital_speed
            // -> Precise orbital speed

            double standardGravitationalParameter = centerMass * Constants.GravitationalConstant;
            return System.Math.Sqrt(standardGravitationalParameter * ((2.0 / radius) - (1.0 / lengthOfSemimajorAxis)));
        }


        public static double calculateOrbitalPeriod(double centerMass, double lengthOfSemimajorAxis) {
            // https://en.wikipedia.org/wiki/Orbital_period
            return 2.0 * System.Math.PI * System.Math.Sqrt(math.Math.powerByIntegerSlow(lengthOfSemimajorAxis, 3) / (centerMass * Constants.GravitationalConstant));
        }

        public static double calculateForceBetweenObjectsByRadius(double massA, double radiusA, double massB, double radiusB) {
            double difference = radiusA - radiusB;
            return calculateForceBetweenObjectsByDistance(massA, massB, difference*difference);
        }

        public static double calculateForceBetweenObjectsByDistance(double massA, double massB, double distanceSquared) {
            return (Constants.GravitationalConstant * massA * massB) / distanceSquared;
        }
    }
}
