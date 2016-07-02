using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace misc {
    class Convolution2d {
        public static Map2d<float> convolution(Map2d<float> input, Map2d<float> kernel) {
            Map2d<float> resultMap = new Map2d<float>(input.getSize());

            for (int y = (int)kernel.getSize().y / 2; y < input.getSize().y - kernel.getSize().y / 2; y++) {
                for (int x = (int)kernel.getSize().x / 2; x < input.getSize().x - kernel.getSize().x / 2; x++) {

                    float sum = 0.0f;

                    for (int kernelY = 0; kernelY < kernel.getSize().y; kernelY++) {
                        for (int kernelX = 0; kernelX < kernel.getSize().x; kernelX++) {
                            float kernelValue = kernel.read(new Vector2d<uint>((uint)kernelX, (uint)kernelY));
                            float inputValue = input.read(new Vector2d<uint>((uint)(x + kernelX - kernel.getSize().x / 2), (uint)(y + kernelY - kernel.getSize().y / 2)));
                            sum += (kernelValue * inputValue);
                        }
                    }

                    resultMap.write(new Vector2d<uint>((uint)x, (uint)y), sum);

                }
            }

            return resultMap;
        }
    }
}
