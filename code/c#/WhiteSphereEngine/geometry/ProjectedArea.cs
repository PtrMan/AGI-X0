
namespace WhiteSphereEngine.geometry {
    public class ProjectedArea {
        public static double ofSphere(double radius) {
            return Area.ofCircle(radius); // projected area of sphere is area of circle
        }
    }
}
