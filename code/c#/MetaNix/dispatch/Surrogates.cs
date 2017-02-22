using System;
using System.Collections.Generic;

namespace MetaNix.dispatch {
    // dispatches a call to an function to the interpreter or JIT or native code
    interface ISurrogate {
        Node dispatchCall(HiddenFunctionId functionId, IList<Node> arguments);
        void invalidateByFunctionId(HiddenFunctionId functionId);
        void updateFunctionBody(HiddenFunctionId functionId, Node body);
        void updateParameterNames(HiddenFunctionId functionId, IList<string> parameterNames);
    }
    
    class FunctionalInterpreterSurrogate : ISurrogate {
        FunctionalInterpretationContext interpretationContext;
        FunctionalInterpreter interpreter;

        class FunctionDescriptor {
            public Node body;
            public IList<string> parameterNames;
        }

        Dictionary<HiddenFunctionId, FunctionDescriptor> functiondescriptorByFunctionId = new Dictionary<HiddenFunctionId, FunctionDescriptor>();

        public FunctionalInterpreterSurrogate(FunctionalInterpreter interpreter, FunctionalInterpretationContext context) {
            this.interpreter = interpreter;
            this.interpretationContext = context;
        }
        
        public Node dispatchCall(HiddenFunctionId functionId, IList<Node> arguments) {
            Ensure.ensure(functiondescriptorByFunctionId.ContainsKey(functionId));

            FunctionDescriptor fnDescriptor = functiondescriptorByFunctionId[functionId];
            interpreter.interpret(interpretationContext, fnDescriptor.body, arguments, fnDescriptor.parameterNames);
            return fnDescriptor.body.interpretationResult;
        }

        public void invalidateByFunctionId(HiddenFunctionId functionId) {
            if (!functiondescriptorByFunctionId.ContainsKey(functionId)) {
                return;
            }
            functiondescriptorByFunctionId.Remove(functionId);
        }

        public void updateFunctionBody(HiddenFunctionId functionId, Node body) {
            if (!functiondescriptorByFunctionId.ContainsKey(functionId)) {
                functiondescriptorByFunctionId[functionId] = new FunctionDescriptor();
            }
            functiondescriptorByFunctionId[functionId].body = body;
        }

        public void updateParameterNames(HiddenFunctionId functionId, IList<string> parameterNames) {
            if (!functiondescriptorByFunctionId.ContainsKey(functionId)) {
                functiondescriptorByFunctionId[functionId] = new FunctionDescriptor();
            }
            functiondescriptorByFunctionId[functionId].parameterNames = parameterNames;
        }
    }

    // acts as a dispatching point of the function calls to th corresponding surrogates
    class SurrogateProvider : IHiddenDispatcher {
        Dictionary<HiddenFunctionId, ISurrogate> surrogateByFunctionId = new Dictionary<HiddenFunctionId, ISurrogate>();
        
        public Node dispatch(HiddenFunctionId functionId, IList<Node> arguments) {
            Ensure.ensure(existsFunctionId(functionId));
            return surrogateByFunctionId[functionId].dispatchCall(functionId, arguments);
        }
        
        public void updateSurrogateByFunctionId(HiddenFunctionId functionId, ISurrogate surrogate) {
            // if there is already an surrogate we invalidate it
            if(surrogateByFunctionId.ContainsKey(functionId) ) {
                surrogateByFunctionId[functionId].invalidateByFunctionId(functionId);
            }

            surrogateByFunctionId[functionId] = surrogate;
        }

        bool existsFunctionId(HiddenFunctionId functionId) {
            return surrogateByFunctionId.ContainsKey(functionId);
        }

    }

    // allows to call functions by name or public function id
    class PublicCallDispatcher {
        //public SurrogateProvider surrogateProvider;

        PublicDispatcherByArguments publicDispatcherByArguments;

        Dictionary<string, PublicFunctionId> functionIdByFunctionname = new Dictionary<string, PublicFunctionId>();

        public PublicCallDispatcher(PublicDispatcherByArguments publicDispatcherByArguments) {
            this.publicDispatcherByArguments = publicDispatcherByArguments;
        }

        public void setFunctionId(string functionname, PublicFunctionId functionId) {
            functionIdByFunctionname[functionname] = functionId;
        }

        // call for more convinience
        public Node dispatchCallByFunctionName(string functionname, IList<Variant> argumentVariants) {
            IList<Node> arguments = new List<Node>(argumentVariants.Count);
            for (int i = 0; i < argumentVariants.Count; i++) {
                arguments[i] = Node.makeAtomic(argumentVariants[i]);
            }
            return publicDispatcherByArguments.dispatch(functionIdByFunctionname[functionname], arguments);
        }
    }
}
