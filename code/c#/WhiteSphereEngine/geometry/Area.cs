using System;
//using System.Collections.Generic;

namespace WhiteSphereEngine.geometry {
    // functions to calculate surface areas of 2d and 3d bodies
    public class Area {
        private Area(){}

        public static double ofCircle(double radius) {
            return 2.0 * Math.PI * radius;
        }

        public static double ofSphere(double radius) {
            return 4.0 * Math.PI * radius*radius;
        }
    }
}
