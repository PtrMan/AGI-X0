using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using MetaNix;

namespace MetaNixUnittest {
    [TestClass]
    public class UnitTest1 {
        NodeRefererEntry interpretAndReturnRootnode(string functionalProgram) {
            Functional.ParseTreeElement parseTree = Functional.parseRecursive(functionalProgram);
            NodeRefererEntry rootnode = TranslateFunctionalParseTree.translateRecursive(parseTree);

            FunctionalInterpreter functionalInterpreter = new FunctionalInterpreter();
            functionalInterpreter.tracer = new NullFunctionalInterpreterTracer();

            functionalInterpreter.interpret(new FunctionalInterpretationContext(), rootnode.entry, new List<ImmutableNodeReferer>(), new List<string>());

            return rootnode;
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
