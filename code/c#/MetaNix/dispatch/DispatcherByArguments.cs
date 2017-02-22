using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    // looks up the function which is specialized for certain parameters
    public class PublicDispatcherByArguments {
        IHiddenDispatcher hiddenDispatcher;

        public PublicDispatcherByArguments(IHiddenDispatcher hiddenDispatcher) {
            this.hiddenDispatcher = hiddenDispatcher;
        }

        public ImmutableNodeReferer dispatch(PublicFunctionId publicFunctionId, IList<ImmutableNodeReferer> arguments) {
            Ensure.ensure(functionsByPublicFunctionId.ContainsKey(publicFunctionId));

            FunctionDescriptor fnDescriptor = functionsByPublicFunctionId[publicFunctionId];

            HiddenFunctionId hiddenFunctionIdByArguments = fnDescriptor.getHiddenFunctionForArguments(extractValuesFromNodes(arguments));

            return hiddenDispatcher.dispatch(hiddenFunctionIdByArguments, arguments);
        }

        public void setFunctionDescriptor(PublicFunctionId functionId, FunctionDescriptor functionDescriptor) {
            functionsByPublicFunctionId[functionId] = functionDescriptor;
        }

        static IList<Variant> extractValuesFromNodes(IList<ImmutableNodeReferer> arguments) {
            IList<Variant> result = new List<Variant>(arguments.Count);
            for(int i =0;i<arguments.Count;i++) {
                result[i] = arguments[i].value;
            }
            return result;
        }

        

        // describes a pattern to be matched
        public class ArgumentPattern {
            public bool argumentWildcard; // can it have any value?
            public VariantRange range; // only matched if wildcard is not enabled

            public bool checkMatch(Variant value) {
                if( argumentWildcard ) {
                    return true;
                }
                return range.isInRange(value);
            }
        }

        // TODO optimization< use an https://en.wikipedia.org/wiki/Interval_tree for fast matching intervals >
        public class ArgumentPatternWithFunction {
            public ArgumentPattern[] patterns;
            public HiddenFunctionId hiddenFunctionId; // used hidden function id in case of match

            public bool checkIfPatternApplies(IList<Variant> values) {
                for(int i=0;i< values.Count;i++) {
                    if(!patterns[i].checkMatch(values[i])) {
                        return false;
                    }
                }
                return true;
            }
        }

        // 
        public class FunctionDescriptor {
            public HiddenFunctionId? wildcardHiddenFunctionId; // is tried if no match was found for the arguments

            // TODO OPTIMIZATION some hashing scheme for the pattern
            public IList<ArgumentPatternWithFunction> patternsWithFunction = new List<ArgumentPatternWithFunction>();

            public HiddenFunctionId getHiddenFunctionForArguments(IList<Variant> values) {
                foreach(ArgumentPatternWithFunction iArgPatternWithFunction in patternsWithFunction) {
                    if( iArgPatternWithFunction.checkIfPatternApplies(values) ) {
                        return iArgPatternWithFunction.hiddenFunctionId;
                    }
                }

                // if we are here no pattern did apply, so the only solution could be the wildcard
                Ensure.ensure(wildcardHiddenFunctionId.HasValue);
                return wildcardHiddenFunctionId.Value;
            }
        }

        // by (external) function id
        IDictionary<PublicFunctionId, FunctionDescriptor> functionsByPublicFunctionId = new Dictionary<PublicFunctionId, FunctionDescriptor>();
    }
}
