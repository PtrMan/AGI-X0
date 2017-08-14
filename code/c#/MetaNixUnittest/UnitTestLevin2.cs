using Microsoft.VisualStudio.TestTools.UnitTesting;

using MetaNix.search.levin2;

namespace MetaNixUnittest {
    [TestClass]
    public class UnitTestLevin2 {
        [TestMethod]
        public void insertInstructionPositive1() { // test if insertion of an instruction in the positive side works correctly
            InstructionOffsetPreservingArray arr = new InstructionOffsetPreservingArray();

            arr.append((int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, 0)); // jump 0
            arr.append(10);

            arr.insert(1, 11);

            Assert.AreEqual(arr[0], (int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, 1)); // must be jump 1
            Assert.AreEqual(arr[1], 11);
            Assert.AreEqual(arr[2], 10);
        }

        [TestMethod]
        public void insertInstructionPositive2() { // test if insertion of an instruction in the positive side works correctly
            InstructionOffsetPreservingArray arr = new InstructionOffsetPreservingArray();

            arr.append((int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, 0)); // jump 0
            arr.append(10);

            arr.insert(2, 11);

            Assert.AreEqual(arr[0], (int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, 0)); // must not be changed
            Assert.AreEqual(arr[1], 10);
            Assert.AreEqual(arr[2], 11);
        }

        [TestMethod]
        public void insertInstructionNegative1() { // test if insertion of an instruction in the negative side works correctly
            InstructionOffsetPreservingArray arr = new InstructionOffsetPreservingArray();

            arr.append(10);
            arr.append((int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -2)); // jump -2

            arr.insert(0, 11);

            Assert.AreEqual(arr[0], 11);
            Assert.AreEqual(arr[1], 10);
            Assert.AreEqual(arr[2], (int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -2));            
        }

        [TestMethod]
        public void insertInstructionNegative2() { // test if insertion of an instruction in the negative side works correctly
            InstructionOffsetPreservingArray arr = new InstructionOffsetPreservingArray();

            arr.append(10);
            arr.append((int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -2)); // jump -2

            arr.insert(1, 11);

            Assert.AreEqual(arr[0], 10);
            Assert.AreEqual(arr[1], 11);
            Assert.AreEqual(arr[2], (int)InstructionInterpreter.convInstructionAndRelativeToInstruction(3, -3));
        }

    }
}
