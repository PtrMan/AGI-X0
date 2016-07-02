using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace misc {
    class Convolution2d {
        public static Map2d<float> convolution(Map2d<float> input, Map2d<float> inputKernel) {
            float[,] inputAsMatrix, kernelAsMatrix;
            inputAsMatrix = convertMapToArray(input);
            kernelAsMatrix = convertMapToArray(inputKernel);

            Map2d<float> resultMap = new Map2d<float>(input.getSize());

            for( int y = 0; y < input.getSize().y - inputKernel.getSize().y; y++ ) {
                for ( int x = 0; x < input.getSize().x - inputKernel.getSize().x; x++) {
                    float value = convolutionAt(ref inputAsMatrix, ref kernelAsMatrix, x, y);
                    resultMap.write(new Vector2d<uint>((uint)x, (uint)y), value);
                }
            }

            return resultMap;
        }

        private static float[,] convertMapToArray(Map2d<float> map) {
            float[,] result;
            int x, y;

            result = new float[map.getSize().x, map.getSize().y];

            for (y = 0; y < map.getSize().y; y++) {
                for (x = 0; x < map.getSize().x; x++) {
                    result[x, y] = map.read(new Vector2d<uint>((uint)x, (uint)y));
                }
            }

            return result;
        }

        private static float convolutionAt(ref float[,] input, ref float[,] kernel, int startX, int startY) {
            float result;
            int x, y;

            result = 0.0f;

            for( y = 0; y < kernel.GetLength(1); y++ ) {
                for( x = 0; x < kernel.GetLength(0); x++ ) {
                    result += (input[x + startX, y + startY] * kernel[x, y]);
                }
            }

            return result;
        }
    }
}
