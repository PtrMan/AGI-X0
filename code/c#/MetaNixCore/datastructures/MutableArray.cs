using System;
using System.Diagnostics;

namespace MetaNix.datastructures {
    // TODO< unittest >
    // provides roughtly array performance (with some overhead) for accesses and resize handling for adding and removing elements
    public class MutableArray<Type> {
        public MutableArray(int initialSize = 32) {
            arr = new Type[initialSize];
        }

        public void removeAt(int idx) {
            Debug.Assert(numberOfElements > 0);
            Debug.Assert(idx < numberOfElements);
            Array.Copy(arr, idx+1, arr, idx, numberOfElements - idx - 1);
            numberOfElements--;
        }

        public void appendAt(int idx, Type value) {
            Debug.Assert(numberOfElements >= 0);
            Debug.Assert(idx <= numberOfElements);
            Array.Copy(arr, idx, arr, idx+1, numberOfElements - idx);
            arr[idx] = value;
            numberOfElements++;
        }

        public double length => numberOfElements;

        public Type this[int idx] {
            get {
                Debug.Assert(idx < numberOfElements);
                return arr[idx];
            }
            set {
                Debug.Assert(idx < numberOfElements);
                arr[idx] = value;
            }
        }

        Type[] arr;
        int numberOfElements;
    }
}
