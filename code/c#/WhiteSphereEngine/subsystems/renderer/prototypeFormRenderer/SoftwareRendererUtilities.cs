using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WhiteSphereEngine.math;

namespace WhiteSphereEngine.subsystems.renderer.prototypeFormRenderer {
    class SoftwareRendererUtilities {
        // from https://en.wikipedia.org/wiki/3D_projection#Perspective_projection
        public static Matrix createProjectionMatrix(double ex, double ey, double ez) {
            return new Matrix(new double[] {
                1, 0, -ex/ez, 0,
                0, 1, -ey/ez, 0,
                0, 0, 1, 0,
                0, 0, 1/ez, 0,

            }, 4);
        }

        public static Matrix project(Matrix vector) {
            return new Matrix(new double[] {vector[0, 0] / vector[3, 0], vector[1, 0] / vector[3, 0] }, 1);
        }
    }
}
