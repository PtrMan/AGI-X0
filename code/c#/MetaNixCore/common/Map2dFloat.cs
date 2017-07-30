using System;
using System.Diagnostics;

namespace MetaNix.common
{
    public class Map2dFloat {
        public Map2dFloat(uint height, uint width) {
            this.width = (int)width;
            this.height = (int)height;

            wasResized();
        }

        // matrix notation, y followed by x
        public float this[int y, int x] {
            get {
                Debug.Assert(isInRange(y, x));
                return arr[x + y * height];
            }
            set {
                Debug.Assert(isInRange(y, x));
                arr[x + y * height] = value;
            }
        }

        public bool isInRange(int y, int x) {
            return x >= 0 && x <= width && y >= 0 && y <= height;
        }

        public void clear(float value = 0.0f) {
            Array.Clear(arr, 0, arr.Length);
        }

        void wasResized() {
            arr = new float[width * height];
        }

        float[] arr;

        public readonly int width, height; // int is an optimization because we don't need to cast
    }
}
