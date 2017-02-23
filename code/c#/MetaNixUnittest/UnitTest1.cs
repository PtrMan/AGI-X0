using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using MetaNix;
using MetaNix.dispatch;

namespace MetaNixUnittest {
    [TestClass]
    public class UnitTest1 {
        NodeRefererEntry interpretAndReturnRootnode(string functionalProgram, IList < ImmutableNodeReferer>  parameterValues = null, IList<String> parameterNames = null) {
            Functional.ParseTreeElement parseTree = Functional.parseRecursive(functionalProgram);
            NodeRefererEntry rootnode = TranslateFunctionalParseTree.translateRecursive(parseTree);

            FunctionalInterpreter functionalInterpreter = new FunctionalInterpreter();
            functionalInterpreter.tracer = new NullFunctionalInterpreterTracer();

            functionalInterpreter.interpret(new FunctionalInterpretationContext(), rootnode.entry, parameterValues == null ? new List<ImmutableNodeReferer>() : parameterValues, parameterNames == null ? new List<string>() : parameterNames);

            return rootnode;
        }

        [TestMethod]
        public void InterpreterVariableLookup() {
            FunctionalInterpretationContext functionalContext = new FunctionalInterpretationContext();
            NodeRefererEntry rootNode = interpretAndReturnRootnode("a", new List<ImmutableNodeReferer>{ ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(Variant.makeInt(42))) }, new List<String>{"a" });
            ImmutableNodeReferer result = rootNode.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 42);
        }
        
        [TestMethod]
        public void InterpreterAndLet1() {
            NodeRefererEntry rootNode = interpretAndReturnRootnode("(let [value 4 i 1   read2 (shl value (+ i 1))] read2)");
            ImmutableNodeReferer result = rootNode.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 16);
        }


        [TestMethod]
        public void interpreterAdd1() {
            NodeRefererEntry rootNode = interpretAndReturnRootnode("(+ 5 2)");
            ImmutableNodeReferer result = rootNode.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 7);
        }

        [TestMethod]
        public void interpreterShl1() {
            NodeRefererEntry rootNode = interpretAndReturnRootnode("(shl 1 2)");
            ImmutableNodeReferer result = rootNode.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 4);
        }

        [TestMethod]
        public void interpreterShr1() {
            NodeRefererEntry rootNode = interpretAndReturnRootnode("(shr 4 2)");
            ImmutableNodeReferer result = rootNode.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 1);
        }

        [TestMethod]
        public void interpreterIfTrue1() {
            NodeRefererEntry rootNode = interpretAndReturnRootnode("(if 1 42 5)");
            ImmutableNodeReferer result = rootNode.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 42);
        }

        [TestMethod]
        public void interpreterIfFalse1() {
            NodeRefererEntry rootNode = interpretAndReturnRootnode("(if 0 42 5)");
            ImmutableNodeReferer result = rootNode.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 5);
        }


        [TestMethod]
        // tests if a functioncall to an defined function can be made
        //
        // creates providers and surrogates, then an callee and called function
        public void InterpreterFunctioninvocation1() {
            FunctionalInterpretationContext functionalContext = new FunctionalInterpretationContext();


            FunctionalInterpreter functionalInterpreter = new FunctionalInterpreter();
            functionalInterpreter.tracer = new NullFunctionalInterpreterTracer();

            FunctionalInterpreterSurrogate interpreterSurrogate = new FunctionalInterpreterSurrogate(functionalInterpreter, functionalContext);

            // dispatcher which dispatches hidden function calls to the interpreter
            SurrogateProvider surrogateProvider = new SurrogateProvider();

            // dispatcher which can shadow calls (to the surrogate provider)
            ShadowableHiddenDispatcher shadowableHiddenDispatcher = new ShadowableHiddenDispatcher(surrogateProvider);


            // dispatcher which calls another dispatcher and a number of observers,
            // which is in this case our instrumentation observer
            InstrumentationHiddenDispatcher instrHiddenDispatcher = new InstrumentationHiddenDispatcher(shadowableHiddenDispatcher);


            PublicDispatcherByArguments publicDispatcherByArguments = new PublicDispatcherByArguments(instrHiddenDispatcher);

            PublicCallDispatcher callDispatcher = new PublicCallDispatcher(publicDispatcherByArguments);

            // all calls the functional interpreter makes to an user defined function are dispatched with this
            functionalContext.publicFnRegistryAndDispatcher = new PublicFunctionRegistryAndDispatcher(callDispatcher);
            

            Functional.ParseTreeElement parseTree = Functional.parseRecursive("3");
            NodeRefererEntry rootnodeCalled = TranslateFunctionalParseTree.translateRecursive(parseTree);

            { // set descriptor to route all public function id's 0 to hidden function id 0
                PublicDispatcherByArguments.FunctionDescriptor fnDescriptor = new PublicDispatcherByArguments.FunctionDescriptor();
                fnDescriptor.wildcardHiddenFunctionId = HiddenFunctionId.make(0);
                publicDispatcherByArguments.setFunctionDescriptor(PublicFunctionId.make(0), fnDescriptor);
            }

            surrogateProvider.updateSurrogateByFunctionId(HiddenFunctionId.make(0), interpreterSurrogate);
            interpreterSurrogate.updateFunctionBody(HiddenFunctionId.make(0), rootnodeCalled.entry);
            interpreterSurrogate.updateParameterNames(HiddenFunctionId.make(0), new List<string>());

            callDispatcher.setFunctionId("a", PublicFunctionId.make(0)); // bind function name "a" to public function id 0
            functionalContext.publicFnRegistryAndDispatcher.addFunction("a"); // register so the dispatcher used by the interpreter knows that the function exists

            Functional.ParseTreeElement parseTree2 = Functional.parseRecursive("(a)");
            NodeRefererEntry rootnodeCallee = TranslateFunctionalParseTree.translateRecursive(parseTree2);


            functionalInterpreter.interpret(functionalContext, rootnodeCallee.entry, new List<ImmutableNodeReferer>(), new List<string>());
            ImmutableNodeReferer result = rootnodeCallee.entry.interpretationResult;
            Assert.AreEqual(result.valueInt, 3);
        }
    }
}
