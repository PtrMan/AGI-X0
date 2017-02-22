using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using MetaNix;

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
    }
}
