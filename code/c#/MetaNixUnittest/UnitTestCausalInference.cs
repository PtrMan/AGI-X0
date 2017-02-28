using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using MetaNix;
using MetaNix.fringeAi.causal;

namespace MetaNixUnittest {
    [TestClass]
    public class UnitTestCausalInference {
        [TestMethod]
        public void causalInference_EntropyMinimize_Trivial1() {
            Linearizer linearizer = new Linearizer();
            linearizer.random = new Random(42);

            float terminationPropability = 0.1f;
            NonanytimeEntropyMinimizer entropyMinimizer = new NonanytimeEntropyMinimizer(linearizer, terminationPropability);
            entropyMinimizer.random = new Random(42);

            CausalSetSystemBlock testCausalBlock = new CausalSetSystemBlock();

            // fill it
            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[0].globalIndex = 0;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[1].globalIndex = 1;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[2].globalIndex = 2;

            // 0 -> 1
            // 1 -> 2
            testCausalBlock.nodes[0].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(1) };
            testCausalBlock.nodes[1].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(2) };

            entropyMinimizer.entry(testCausalBlock);


            entropyMinimizer.anytimeInvoke();



            long energyOfLinearization;
            GlobalLinearization linearization = entropyMinimizer.getBestGlobalLinearizationAndEnergy(out energyOfLinearization);
            Assert.AreEqual(energyOfLinearization, 2);

            Assert.IsTrue(linearization.linearization[0] == 0);
            Assert.IsTrue(linearization.linearization[1] == 1);
            Assert.IsTrue(linearization.linearization[2] == 2);

            entropyMinimizer.finish();

        }

        [TestMethod]
        public void causalInference_EntropyMinimize_Trivial2() {
            Linearizer linearizer = new Linearizer();
            linearizer.random = new Random(42);

            float terminationPropability = 0.1f;
            NonanytimeEntropyMinimizer entropyMinimizer = new NonanytimeEntropyMinimizer(linearizer, terminationPropability);
            entropyMinimizer.random = new Random(42);

            CausalSetSystemBlock testCausalBlock = new CausalSetSystemBlock();

            // fill it
            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[0].globalIndex = 0;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[1].globalIndex = 1;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[2].globalIndex = 2;

            // 1 -> 0
            // 0 -> 2
            testCausalBlock.nodes[1].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(0) };
            testCausalBlock.nodes[0].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(2) };

            entropyMinimizer.entry(testCausalBlock);

            entropyMinimizer.anytimeInvoke();

            
            long energyOfLinearization;
            GlobalLinearization linearization = entropyMinimizer.getBestGlobalLinearizationAndEnergy(out energyOfLinearization);
            Assert.AreEqual(energyOfLinearization, 2);

            Assert.IsTrue(linearization.linearization[0] == 1);
            Assert.IsTrue(linearization.linearization[1] == 0);
            Assert.IsTrue(linearization.linearization[2] == 2);

            entropyMinimizer.finish();
        }

        [TestMethod]
        public void causalInference_EntropyMinimize_Trivial3() {
            Linearizer linearizer = new Linearizer();
            linearizer.random = new Random(42);

            float terminationPropability = 0.1f;
            NonanytimeEntropyMinimizer entropyMinimizer = new NonanytimeEntropyMinimizer(linearizer, terminationPropability);
            entropyMinimizer.random = new Random(42);

            CausalSetSystemBlock testCausalBlock = new CausalSetSystemBlock();

            // fill it
            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[0].globalIndex = 0;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[1].globalIndex = 1;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[2].globalIndex = 2;

            // 1 -> 0
            // 2 -> 0
            testCausalBlock.nodes[1].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(0) };
            testCausalBlock.nodes[2].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(0) };

            entropyMinimizer.entry(testCausalBlock);

            entropyMinimizer.anytimeInvoke();


            long energyOfLinearization;
            GlobalLinearization linearization = entropyMinimizer.getBestGlobalLinearizationAndEnergy(out energyOfLinearization);
            Assert.AreEqual(energyOfLinearization, 3);

            Assert.IsTrue(linearization.linearization[2] == 0);
            if( linearization.linearization[1] == 1 ) {
                Assert.IsTrue(linearization.linearization[0] == 2);
            }
            else if (linearization.linearization[1] == 2) {
                Assert.IsTrue(linearization.linearization[0] == 1);
            }
            else {
                throw new Exception("first element must be 1 or 2!");
            }

            entropyMinimizer.finish();
        }

        [TestMethod]
        public void causalInference_EntropyMinimize_Trivial4() {
            Linearizer linearizer = new Linearizer();
            linearizer.random = new Random(42);

            float terminationPropability = 0.1f;
            NonanytimeEntropyMinimizer entropyMinimizer = new NonanytimeEntropyMinimizer(linearizer, terminationPropability);
            entropyMinimizer.random = new Random(42);


            CausalSetSystemBlock testCausalBlock = new CausalSetSystemBlock();

            // fill it
            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[0].globalIndex = 0;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[1].globalIndex = 1;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[2].globalIndex = 2;

            // 1 -> 0
            // 1 -> 2
            testCausalBlock.nodes[1].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(0) };
            testCausalBlock.nodes[2].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(0) };

            entropyMinimizer.entry(testCausalBlock);


            entropyMinimizer.anytimeInvoke();



            long energyOfLinearization;
            GlobalLinearization linearization = entropyMinimizer.getBestGlobalLinearizationAndEnergy(out energyOfLinearization);
            Assert.AreEqual(energyOfLinearization, 3);

            Assert.IsTrue(linearization.linearization[2] == 0);
            if (linearization.linearization[1] == 1) {
                Assert.IsTrue(linearization.linearization[0] == 2);
            }
            else if (linearization.linearization[1] == 2) {
                Assert.IsTrue(linearization.linearization[0] == 1);
            }
            else {
                throw new Exception("first element must be 1 or 2!");
            }

            entropyMinimizer.finish();
        }



        // test if it can tunnel the local minima
        [TestMethod]
        public void CausalInference_EntropyMinimize_Tunnel0() {
            Linearizer linearizer = new Linearizer();
            linearizer.random = new Random(42);

            float terminationPropability = 0.1f;
            NonanytimeEntropyMinimizer entropyMinimizer = new NonanytimeEntropyMinimizer(linearizer, terminationPropability);
            entropyMinimizer.random = new Random(42);

            CausalSetSystemBlock testCausalBlock = new CausalSetSystemBlock();

            
            // fill it
            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[0].globalIndex = 0;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[1].globalIndex = 1;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[2].globalIndex = 2;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[3].globalIndex = 3;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[4].globalIndex = 4;

            // 2 -> 0
            // 3 -> 4
            // 3 -> 1
            // 3 -> 0
            // 0 -> 1
            // 4 -> 1

            testCausalBlock.nodes[2].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(0) };
            testCausalBlock.nodes[3].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(4), new CausalIndirectionIndex(1), new CausalIndirectionIndex(0) };
            testCausalBlock.nodes[0].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(1) };
            testCausalBlock.nodes[4].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(1) };

            entropyMinimizer.entry(testCausalBlock);

            entropyMinimizer.anytimeInvoke();


            // target sequence with minimal energy  is 2 3 0 4 1

            long energyOfLinearization;
            GlobalLinearization linearization = entropyMinimizer.getBestGlobalLinearizationAndEnergy(out energyOfLinearization);
            Assert.AreEqual(energyOfLinearization, 11);

            Assert.IsTrue(linearization.linearization[0] == 2);
            Assert.IsTrue(linearization.linearization[1] == 3);
            Assert.IsTrue(linearization.linearization[2] == 0);
            Assert.IsTrue(linearization.linearization[3] == 4);
            Assert.IsTrue(linearization.linearization[4] == 1);

            entropyMinimizer.finish();
        }


        // next indices must be valid for the folded entry SystemBlock
        [TestMethod]
        public void CausalInference_FuseAndCheckForNextIndices1() {
            CausalSetSystemBlock testCausalBlock = new CausalSetSystemBlock();

            // fill it
            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[0].globalIndex = 0;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[1].globalIndex = 1;

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());
            testCausalBlock.nodes[2].globalIndex = 2;

            // 0 -> 2
            // 2 -> 1
            testCausalBlock.nodes[0].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(2) };
            testCausalBlock.nodes[2].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(1) };

            CausalSetSystemBlock afterFuse;
            CausalSetSystemBlock fused = CausalSetNodeFuser.fuse(testCausalBlock, out afterFuse,  new List<uint> { 1, 2 });

            Assert.AreEqual(afterFuse.nodes[0].next.Count, 1);
            Assert.IsTrue(afterFuse.translateIndirectIndexToIndex(testCausalBlock.nodes[0].next[0]) == 1); // first node must point to next one, which got fused
        }

        
        static CausalSetSystemBlock buildSystemBlock1() {
            CausalSetSystemBlock testCausalBlock = new CausalSetSystemBlock();

            // fill it
            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());

            testCausalBlock.nodes.Add(new CausalSetNode());
            testCausalBlock.indirectionArray.Add(testCausalBlock.getNewIndirectionNumber());

            testCausalBlock.nodes[0].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(1), new CausalIndirectionIndex(2) };
            testCausalBlock.nodes[2].next = new CausalIndirectionIndex[] { new CausalIndirectionIndex(3), new CausalIndirectionIndex(4) };

            return testCausalBlock;
        }


        [TestMethod]
        public void CausalInference_FuseAsNewSystemBlock1() {
            CausalSetSystemBlock testCausalBlock = buildSystemBlock1();
            CausalSetSystemBlock parentAfterFuse;
            CausalSetSystemBlock fused = CausalSetNodeFuser.fuse(testCausalBlock, out parentAfterFuse, new uint[] { 0, 1 });

            // should collapse to an graph 0 --> 1
            Assert.AreEqual(fused.nodes.Count, 2);
            Assert.AreEqual(fused.nodes[0].next.Count, 1);
            Assert.IsTrue(fused.translateIndirectIndexToIndex(fused.nodes[0].next[0]) == 1);

            Assert.AreEqual(fused.nodes[1].next.Count, 0);
        }

        [TestMethod]
        public void CausalInference_FuseAsNewSystemBlock2() {
            CausalSetSystemBlock testCausalBlock = buildSystemBlock1();

            CausalSetSystemBlock parentAfterFuse;
            CausalSetSystemBlock fused4 = CausalSetNodeFuser.fuse(testCausalBlock, out parentAfterFuse, new uint[] { 0, 2 });

            // should collapse to an graph 0 --> 1
            Assert.AreEqual(fused4.nodes.Count, 2);
            Assert.AreEqual(fused4.nodes[0].next.Count, 1);
            Assert.IsTrue(fused4.nodes[0].next[0].value == 1);

            Assert.AreEqual(fused4.nodes[1].next.Count, 0);
        }

        [TestMethod]
        public void CausalInference_FuseAsNewSystemBlock3() {
            CausalSetSystemBlock testCausalBlock = buildSystemBlock1();

            CausalSetSystemBlock parentAfterFuse;
            CausalSetSystemBlock fused2 = CausalSetNodeFuser.fuse(testCausalBlock, out parentAfterFuse, new uint[] { 2, 3 });

            // should collapse to an graph 0 --> 1
            Assert.AreEqual(fused2.nodes.Count, 2);
            Assert.AreEqual(fused2.nodes[0].next.Count, 1);
            Assert.IsTrue(fused2.nodes[0].next[0].value == 1);

            Assert.AreEqual(fused2.nodes[1].next.Count, 0);
        }

        [TestMethod]
        public void CausalInference_FuseAsNewSystemBlock4() {
            CausalSetSystemBlock testCausalBlock = buildSystemBlock1();

            CausalSetSystemBlock parentAfterFuse;
            CausalSetSystemBlock fused3 = CausalSetNodeFuser.fuse(testCausalBlock, out parentAfterFuse, new uint[] { 2 });
            
            // node 2 doesn't point to anythin in the subgraph
            Assert.AreEqual(fused3.nodes.Count, 1);
            Assert.AreEqual(fused3.nodes[0].next.Count, 0);

        }
    }
}
