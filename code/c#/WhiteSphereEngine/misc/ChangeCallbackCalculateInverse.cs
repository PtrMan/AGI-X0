using WhiteSphereEngine.math;

namespace WhiteSphereEngine.misc {
    public class ChangeCallbackCalculateInverse : ChangeCallback<Matrix> {
        public Matrix getInverse() {
            return inverse;
        }

        protected override void changed() {
            inverse = this.get().inverse();
        }

        Matrix inverse;
    }
}
