using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MetaNix;
using MetaNix.datastructures;

namespace MetaNixUnittest {
    [TestClass]
    public class UnitTestNode {
        [TestMethod]
        public void arrayInsert1() {
            var orginal = NodeRefererEntryManipulationHelper.makeImmutableArray(new List<Variant>());
            var insertedArray = NodeRefererEntryManipulationHelper.arrayInsert(orginal, 0, ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(Variant.makeInt(42))));
            Assert.AreEqual(insertedArray.entry.children[0].valueInt, 42);
        }

        // TODO< insert with multiple elements >

        [TestMethod]
        public void arrayRemove1() {
            var orginal = NodeRefererEntryManipulationHelper.makeImmutableArray(new List<Variant>{ Variant.makeInt(42), Variant.makeInt(13), Variant.makeInt(3)});
            var manipulatedArray = NodeRefererEntryManipulationHelper.arrayRemove(orginal, 0);
            Assert.AreEqual(manipulatedArray.entry.children[0].valueInt, 13);
            Assert.AreEqual(manipulatedArray.entry.children[1].valueInt, 3);
        }

        [TestMethod]
        public void arrayRemove2() {
            var orginal = NodeRefererEntryManipulationHelper.makeImmutableArray(new List<Variant> { Variant.makeInt(42), Variant.makeInt(13), Variant.makeInt(3) });
            var manipulatedArray = NodeRefererEntryManipulationHelper.arrayRemove(orginal, 1);
            Assert.AreEqual(manipulatedArray.entry.children[0].valueInt, 42);
            Assert.AreEqual(manipulatedArray.entry.children[1].valueInt, 3);
        }

        // TODO< remove with multiple elements >

        [TestMethod]
        public void arrayClear1() {
            var orginal = NodeRefererEntryManipulationHelper.makeImmutableArray(new List<Variant> { Variant.makeInt(42), Variant.makeInt(13), Variant.makeInt(3) });
            var manipulatedArray = NodeRefererEntryManipulationHelper.arrayClear(orginal);
            Assert.AreEqual(manipulatedArray.entry.children.Length, 0);
        }
    }
}
