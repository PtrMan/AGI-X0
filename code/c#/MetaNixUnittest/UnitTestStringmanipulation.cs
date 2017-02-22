using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using MetaNix.symbolic;

namespace MetaNixUnittest {

    class ArrayStringAccessor : IStringAccessor<int> {
        IList<int> arr;

        public ArrayStringAccessor(IList<int> arr) {
            this.arr = arr;
        }

        public int at(int idx) {
            return arr[idx];
        }
        public int length() {
            return arr.Count;
        }
    }

    [TestClass]
    public class UnitTestStringmanipulation {
        [TestMethod]
        public void kmp() {
            ArrayStringAccessor W = new ArrayStringAccessor(new List<int> { 0, 1, 2, 3, 0, 1, 3 });
            int[] T = new int[W.length()];

            StringManipulation.kmpTable(W, ref T);

            // ABC ABCDAB ABCDABCDABDE
            ArrayStringAccessor S = new ArrayStringAccessor(new List<int> { 0, 1, 2, 50, 0, 1, 2, 3, 0, 1, 50, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 3, 4 });


            bool found;
            int searchResult = StringManipulation.kmpSearch(S, W, T, out found);
            Assert.IsTrue(found);
            Assert.AreEqual(searchResult, 15);
        }
    }
}
