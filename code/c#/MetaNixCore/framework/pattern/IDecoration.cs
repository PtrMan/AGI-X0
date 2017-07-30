using MetaNix.framework.misc;

namespace MetaNix.framework.pattern {
    public interface IDecoration<DecorationType> : IDeepCopyable<DecorationType> {
        bool checkEqualValue(DecorationType other);
    }
}
