using AiThisAndThat.framework.misc;

namespace AiThisAndThat.patternMatching {
    public interface IDecoration<DecorationType> : IDeepCopyable<DecorationType> {
        bool checkEqualValue(DecorationType other);
    }
}
