using System;
using System.Collections.Generic;
using System.Linq;

namespace MetaNix.fringeAi.causal {

    // small simple (slow) implementation of causal set algorithm(s)
    // from 2017
    
    
    public struct CausalIndirectionIndex {
        public CausalIndirectionIndex(uint value) {
            this.value = value;
        }

        public uint value;
    }

    // definition of indirex indirection type
    // used to point with an index to the index of an element in the causal set
    //alias Typedef!long IdxInd;

    public class CausalSetNode {
        public IList<CausalIndirectionIndex> next = new List<CausalIndirectionIndex>(); // points to the children elements of this element (are indirect indices)

        public CausalSetSystemBlock content; // embeded block, can be null

        //public uint? index;
    }

    // every block has it's local own nodes and indirection array, the indices are not globally defined
    public class CausalSetSystemBlock {
        // indirection to nodes 
        public IList<uint> indirectionArray = new List<uint>();

        public IList<CausalSetNode> nodes = new List<CausalSetNode>();

        // helper for readability
        public CausalSetNode getNodeByIndex(uint index) {
            return nodes[(int)index];
        }

        public uint translateIndirectIndexToIndex(CausalIndirectionIndex idxInd) {
            return indirectionArray[(int)idxInd.value];
        }

        //public void updateNodeIndices() {
        //    for (uint i = 0; i < nodes.Count; i++) {
        //        nodes[(int)i].index = i;
        //    }
        //}

        public IList<CausalSetNode> getNodesWithNodeAsChildren(CausalIndirectionIndex targetIndirectionIndex) {
            return nodes.Where(v => v.next.Contains(targetIndirectionIndex)).ToList();
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

            CausalSetNode selectedNode = this.nodes[(int)entryIdx];
            foreach (CausalIndirectionIndex iFollowerIndirectionIdx in selectedNode.next) {
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







    public class Linearizer {
        ICausalSetSystemBlockAccessor causalSystemBlockAccessor;

        public Random random = new Random();

        public IList<uint> linearlize(CausalSetSystemBlock causalSystemBlock) {
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

    public interface ICausalSetSystemBlockAccessor {
        uint[] getEntryIndicesAsList(CausalSetSystemBlock causalSystemBlock);
        ISet<uint> getNextIndicesOfNodeAsSet(CausalSetSystemBlock causalSystemBlock, uint nodeIdx);
    }

    // implementation of CausalSystemBlockAccessor for the type "CausalSystemBlock"
    public class CausalSetSystemBlockAccessor : ICausalSetSystemBlockAccessor {
        public uint[] getEntryIndicesAsList(CausalSetSystemBlock causalSystemBlock) {
            return causalSystemBlock.entryIndices;
        }

        public ISet<uint> getNextIndicesOfNodeAsSet(CausalSetSystemBlock causalSystemBlock, uint nodeIdx) {
            uint[] nodeIndicesAsList = causalSystemBlock.nodes[(int)nodeIdx].next.Select(n => causalSystemBlock.indirectionArray[(int)n.value]).ToArray();
            return SetHelpers.toSet(nodeIndicesAsList);
        }
    }



    public sealed class ArrayHelpers {
        public static uint[] copy(uint[] arr) {
            uint[] result = new uint[arr.Length];
            for (int i = 0; i < arr.Length; i++) {
                result[i] = arr[i];
            }
            return result;
        }
    }

    public sealed class SetHelpers {
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


    // public for unittest
    public sealed class CausalSetNodeFuser {
        // creates a new block with the content as the fused nodes and fuses the nodes in the "entryBlock" and returns the created "CausalSystemBlock" which contains all fused nodes on the next level
        static public CausalSetSystemBlock fuse(CausalSetSystemBlock entryBlock, uint[] indices) {
            CausalSetSystemBlock higherLevelBlock = new CausalSetSystemBlock();

            // fill higherLevelBlock with as many nodes as required
            for (uint i = 0; i < indices.Length; i++) {
                higherLevelBlock.nodes.Add(new CausalSetNode());
                higherLevelBlock.indirectionArray.Add(higherLevelBlock.getNewIndirectionNumber());
            }

            // translates the index in entryBlock to the index in higherLevelBlock
            Dictionary<uint, uint> indexToElementInHigherLevelBlock = new Dictionary<uint, uint>();
            for (uint i = 0; i < indices.Length; i++) {
                uint redirectedIndex = indices[i];
                indexToElementInHigherLevelBlock[redirectedIndex] = i;
            }


            // rewire from entryBlock the subnetwork to HigherLevelBlock
            foreach (Tuple<uint, CausalSetNode> iterationIndexAndNodePair in indices.Select(index => new Tuple<uint, CausalSetNode>(index, entryBlock.getNodeByIndex(index)))) {
                uint higherLevelBlock_index = indexToElementInHigherLevelBlock[iterationIndexAndNodePair.Item1];

                var nextValidIndicesFromIterationNode =
                    iterationIndexAndNodePair.Item2.next
                        .Select(nextIndirection => entryBlock.translateIndirectIndexToIndex(nextIndirection))
                        .Where(index => indexToElementInHigherLevelBlock.ContainsKey(index)); // if the node points to an node which is not in nextLevelBlock then we ignore it

                foreach (uint iterationNextValidIndexFromIterationNode in nextValidIndicesFromIterationNode) {
                    uint higherLevelBlock_nextIndex = indexToElementInHigherLevelBlock[iterationNextValidIndexFromIterationNode];

                    // make sure that the indirection is an identity
                    // only this way we don't have to do an inverse lookup to get the CausalIndirectionIndex
                   Ensure.ensureHard(higherLevelBlock.translateIndirectIndexToIndex(new CausalIndirectionIndex(higherLevelBlock_nextIndex)) == higherLevelBlock_nextIndex);

                    higherLevelBlock.nodes[(int)higherLevelBlock_index].next.Add(new CausalIndirectionIndex(higherLevelBlock_nextIndex));
                }
            }


            // now do the fuse
            uint fuseAsIndirectionNumber = entryBlock.getNewIndirectionNumber();
            entryBlock.fuse(indices, fuseAsIndirectionNumber);

            return higherLevelBlock;
        }
    }



}
