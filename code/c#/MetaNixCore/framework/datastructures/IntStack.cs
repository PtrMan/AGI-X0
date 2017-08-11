namespace MetaNix.framework.datastructures {
    public class IntStack {
        int privateCount;
        int[] stackValues = new int[128];

        public void setTo(int[] arr) {
            arr.CopyTo(stackValues, 0);
            privateCount = arr.Length;
        }

        public void push(int value) {
            stackValues[privateCount] = value;
            privateCount++;
        }

        public int pop(out bool success) {
            if (privateCount <= 0) {
                success = false;
                return 0;
            }

            success = true;
            int result = stackValues[privateCount - 1];
            privateCount--;
            return result;
        }

        public int top {
            get {
                return stackValues[privateCount - 1];
            }
        }

        public int count {
            get {
                return privateCount;
            }
        }
    }
}
