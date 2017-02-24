using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaNix.fringeAi.causal {

    // small simple (slow) implementation of causal set algorithm(s)
    // from 2017



    // definition of indirex indirection type
    // used to point with an index to the index of an element in the causal set
    //alias Typedef!long IdxInd;

    class Node {
        public uint[] next = new uint[0]; // points to the children elements of this element

        public CausalSystemBlock content; // embeded block, can be null
    }

    // every block has it's local own nodes and indirection array, the indices are not globally defined
    class CausalSystemBlock {
        // indirection to nodes 
        public IList<uint> indirectionArray = new List<uint>();

        public IList<Node> nodes = new List<Node>();

        uint translateIndirectIndexToIndex(uint idxInd) {
            return indirectionArray[(int)idxInd];
        }

        uint indirectionCounter; // counter, doesn't reflect the # of elements in the arrays

        // used to determine the unused id of indirection elements
        public uint getNewIndirectionNumber() {
            return indirectionCounter++;
        }

        public uint[] entryIndices; // indices to "nodes" of the elements which have no parents

        // fuses indirections to nodes to just one node
        public void fuse(uint[] indices, uint fusedAsIndex) {
            foreach (uint iIdx in indices) {
                this.indirectionArray[(int)iIdx] = fusedAsIndex;
            }
        }



        public void chooseRandomEntryAndFuseRandomElements(float terminationPropability, uint fusedAsIndex) {
            uint[] entryIndices = this.entryIndices;
            uint randomEntryIdx = entryIndices[random.Next((int)entryIndices.Count())];

            uint[] randomFollowerSetIndices = this.getRandomFollowerSet(randomEntryIdx, terminationPropability).ToArray();
            this.fuse(randomFollowerSetIndices, fusedAsIndex);
        }

        public uint[] getRandomFollowerList(uint entryIdx, float terminationPropability) {
            return getRandomFollowerSet(entryIdx, terminationPropability).ToArray();
        }

        // bool[] just as a dummy for a set
        public ISet<uint> getRandomFollowerSet(uint entryIdx, float terminationPropability) {
            ISet<uint> resultSet = new HashSet<uint>();
            resultSet.Add(entryIdx); // entry ind must be included
            this.getRandomFollowerSetInternalRecursive(entryIdx, resultSet, terminationPropability);
            return resultSet;
        }

        public void getRandomFollowerSetInternalRecursive(uint entryIdx, ISet<uint> followerSet, float terminationPropability) {
            bool terminate = randomBool(terminationPropability);
            if (terminate) {
                return;
            }

            followerSet.Add(entryIdx);

            Node selectedNode = this.nodes[(int)entryIdx];
            foreach (uint iFollowerIndirectionIdx in selectedNode.next) {
                uint followerIndex = this.translateIndirectIndexToIndex(iFollowerIndirectionIdx);
                this.getRandomFollowerSetInternalRecursive(followerIndex, followerSet, terminationPropability);
            }
        }


        private bool randomBool(float propability) {
            return random.NextDouble() < propability;
        }

        // public to access for unittesting
        public Random random = new Random();
    }







    class Linearizer {
        ICausalSystemBlockAccessor causalSystemBlockAccessor;

        public Random random = new Random();

        public IList<uint> linearlize(CausalSystemBlock causalSystemBlock) {
            IList<uint> linearlization = new List<uint>(); // indices of nodes of the linearlization

            uint[] indicesOfEntryNodes = causalSystemBlockAccessor.getEntryIndicesAsList(causalSystemBlock);

            IList<uint> openNodeIndices = ArrayHelpers.copy(indicesOfEntryNodes);
            ISet<uint> nodeIndicesInLinearlization = new HashSet<uint>(); // set, bool values hae no meaning

            while (openNodeIndices.Count != 0) {
                uint currentCandidateNodeIndex;

                {// choose random element from openNodeIndices and remove
                    uint candidateIndexOfOpenNodeIndices = (uint)random.Next(openNodeIndices.Count);
                    currentCandidateNodeIndex = openNodeIndices[(int)candidateIndexOfOpenNodeIndices];
                    openNodeIndices.RemoveAt((int)candidateIndexOfOpenNodeIndices);
                }

                Ensure.ensureHard(!nodeIndicesInLinearlization.Contains(currentCandidateNodeIndex));
                nodeIndicesInLinearlization.Add(currentCandidateNodeIndex);

                linearlization.Add(currentCandidateNodeIndex);

                ISet<uint> nextIndicesOfCurrentCandidate = SetHelpers.subtract(causalSystemBlockAccessor.getNextIndicesOfNodeAsSet(causalSystemBlock, currentCandidateNodeIndex), nodeIndicesInLinearlization);
                openNodeIndices = SetHelpers.union(SetHelpers.toSet(openNodeIndices), nextIndicesOfCurrentCandidate).ToList();
            }

            return linearlization;
        }
    }

    interface ICausalSystemBlockAccessor {
        uint[] getEntryIndicesAsList(CausalSystemBlock causalSystemBlock);
        ISet<uint> getNextIndicesOfNodeAsSet(CausalSystemBlock causalSystemBlock, uint nodeIdx);
    }

    // implementation of CausalSystemBlockAccessor for the type "CausalSystemBlock"
    class CausalSystemBlockAccessor : ICausalSystemBlockAccessor {
        public uint[] getEntryIndicesAsList(CausalSystemBlock causalSystemBlock) {
            return causalSystemBlock.entryIndices;
        }

        public ISet<uint> getNextIndicesOfNodeAsSet(CausalSystemBlock causalSystemBlock, uint nodeIdx) {
            uint[] nodeIndicesAsList = causalSystemBlock.nodes[(int)nodeIdx].next.Select(n => causalSystemBlock.indirectionArray[(int)n]).ToArray();
            return SetHelpers.toSet(nodeIndicesAsList);
        }
    }



    sealed class ArrayHelpers {
        public static uint[] copy(uint[] arr) {
            uint[] result = new uint[arr.Length];
            for (int i = 0; i < arr.Length; i++) {
                result[i] = arr[i];
            }
            return result;
        }
    }

    sealed class SetHelpers {
        public static ISet<uint> toSet(uint[] arr) {
            ISet<uint> result = new HashSet<uint>();
            foreach (uint iArr in arr) {
                result.Add(iArr);
            }
            return result;
        }

        public static ISet<uint> toSet(IList<uint> arr) {
            ISet<uint> result = new HashSet<uint>();
            foreach (uint iArr in arr) {
                result.Add(iArr);
            }
            return result;
        }

        public static ISet<uint> subtract(ISet<uint> a, ISet<uint> b) {
            ISet<uint> result = new HashSet<uint>();
            foreach (uint ia in a) {
                if (!b.Contains(ia)) {
                    result.Add(ia);
                }
            }
            return result;
        }

        public static ISet<uint> union(ISet<uint> a, ISet<uint> b) {
            ISet<uint> result = new HashSet<uint>();
            foreach (uint ai in a) {
                result.Add(ai);
            }

            foreach (uint bi in b) {
                result.Add(bi);
            }
            return result;
        }

    }

    

    class Program {


        // roll one bit to the left
        static ulong rolLeft1(ulong number) {
            ulong carryOver = number >> (64 - 1);
            ulong shiftedToLeft = number << 1;
            return carryOver | shiftedToLeft;
        }

        class Obj {
            public int a;
            public int b;
        }

        static void Main(string[] args) {
            //IList<Obj> input = null;
            //IOrderedEnumerable<Obj> ordered = input.OrderBy(v => v.a);


            /* test for old impl
            CausalDag dag = new CausalDag();

            dag.elements.Add(new CausalDagElement());
            dag.elements.Add(new CausalDagElement());
            dag.elements.Add(new CausalDagElement());

            dag.elements[0].childrenIndices = new List<uint>{2};
            dag.elements[0].marker = true;
            dag.elements[1].childrenIndices = new List<uint>{2};
            dag.elements[1].marker = true;

            CausalDag resultDag = dag.fuseMarkedElements();

                * 
                */




            CausalSystemBlock testCausalSystemBlock = new CausalSystemBlock();

            // TODO< fill it >
            testCausalSystemBlock.nodes.Add(new Node());
            testCausalSystemBlock.indirectionArray.Add(testCausalSystemBlock.getNewIndirectionNumber());

            testCausalSystemBlock.nodes.Add(new Node());
            testCausalSystemBlock.indirectionArray.Add(testCausalSystemBlock.getNewIndirectionNumber());

            testCausalSystemBlock.nodes.Add(new Node());
            testCausalSystemBlock.indirectionArray.Add(testCausalSystemBlock.getNewIndirectionNumber());

            testCausalSystemBlock.nodes.Add(new Node());
            testCausalSystemBlock.indirectionArray.Add(testCausalSystemBlock.getNewIndirectionNumber());

            testCausalSystemBlock.nodes.Add(new Node());
            testCausalSystemBlock.indirectionArray.Add(testCausalSystemBlock.getNewIndirectionNumber());

            testCausalSystemBlock.nodes[0].next = new uint[] { 1, 2 };

            testCausalSystemBlock.nodes[2].next = new uint[] { 3, 4 };


            float terminationPropability = 0.1f;
            uint entryIdx = 0;
            uint[] result = testCausalSystemBlock.getRandomFollowerList(entryIdx, terminationPropability);

            foreach (uint i in result) {
                Console.Write("{0},", i);
            }







            int x = 0;
        }
    }
}
