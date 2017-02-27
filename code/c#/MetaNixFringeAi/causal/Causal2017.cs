using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using MetaNix.misc;

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

        public uint? nodeIndexInParentSystemBlock;

        public uint? globalIndex; // can be null if it is a fused node
        private CausalSetSystemBlock higherLevelBlock;

        public CausalSetNode(CausalSetSystemBlock content = null) {
            this.content = content;
        }

        public CausalSetNode copyShallow() {
            Ensure.ensureHard(content == null);
            CausalSetNode result = new CausalSetNode();
            result.next = ListHelpers.copy(next);
            result.nodeIndexInParentSystemBlock = nodeIndexInParentSystemBlock;
            result.globalIndex = globalIndex;
            return result;
        }

        public CausalSetNode copyDeep() {
            Ensure.ensureHard(content == null);
            CausalSetNode result = new CausalSetNode();
            result.next = ListHelpers.copy(next);
            result.nodeIndexInParentSystemBlock = nodeIndexInParentSystemBlock;
            result.globalIndex = globalIndex;
            if( content != null ) {
                result.content = result.content.copyDeep();
            }

            return result;
        }

    }

    // every block has it's local own nodes and indirection array, the indices are not globally defined
    public class CausalSetSystemBlock {
        // indirection to nodes, indexed with indirectionIndex
        public IList<uint> indirectionArray = new List<uint>();

        public IList<CausalSetNode> nodes = new List<CausalSetNode>();

        // copies the whole metainformation and connectivity (indirection array) but doesn't copy recursive nodes!
        public CausalSetSystemBlock copyShallow() {
            CausalSetSystemBlock copyResult = new CausalSetSystemBlock();
            copyResult.indirectionArray = ArrayHelpers.copy(indirectionArray);
            copyResult.indirectionCounter = indirectionCounter;
            copyResult.entryIndices = entryIndices != null ? ListHelpers.copy(entryIndices) : null;
            foreach ( CausalSetNode iteratorNode in nodes ) {
                copyResult.nodes.Add(iteratorNode.copyShallow());
            }
            return copyResult;
        }

        public CausalSetSystemBlock copyDeep() {
            CausalSetSystemBlock copyResult = new CausalSetSystemBlock();
            copyResult.indirectionArray = ArrayHelpers.copy(indirectionArray);
            copyResult.indirectionCounter = indirectionCounter;
            copyResult.entryIndices = entryIndices != null ? ListHelpers.copy(entryIndices) : null;

            foreach (CausalSetNode iteratorNode in nodes) {
                copyResult.nodes.Add(iteratorNode.copyDeep());
            }
            return copyResult;
        }


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

        public IList<uint> entryIndices; // indices to "nodes" of the elements which have no parents

        // fuses indirections to nodes to just one node
        public void fuse(IList<uint> indices, uint fusedAsIndex) {
            foreach (uint iIdx in indices) {
                this.indirectionArray[(int)iIdx] = fusedAsIndex;
            }
        }



        public void chooseRandomEntryAndFuseRandomElements(float terminationPropability, uint fusedAsIndex) {
            IList<uint> entryIndices = this.entryIndices;
            uint randomEntryIdx = entryIndices[random.Next((int)entryIndices.Count())];

            uint[] randomFollowerSetIndices = this.getRandomFollowerSet(randomEntryIdx, terminationPropability).ToArray();
            this.fuse(randomFollowerSetIndices, fusedAsIndex);
        }

        public IEnumerable<uint> getRandomFollowerAsEnumerable(uint entryIdx, float terminationPropability) {
            return getRandomFollowerSet(entryIdx, terminationPropability).ToList();
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

        public void updateEntryIndices() {
            // entry indices are indices of nodes which are not referenced

            entryIndices = new List<uint>();

            uint[] referenceCounterByNode = new uint[nodes.Count];
            foreach( CausalSetNode iterationNode in nodes ) {
                foreach( uint iterationNextIndex in iterationNode.next.Select(iterationIndirectionIndex => translateIndirectIndexToIndex(iterationIndirectionIndex)) ) {
                    referenceCounterByNode[iterationNextIndex]++;
                }
            }

            for(uint i = 0; i < nodes.Count; i++) {
                if(referenceCounterByNode[i] == 0) { // if unreferenced
                    entryIndices.Add(i);
                }
            }
        }


        private bool randomBool(float propability) {
            return random.NextDouble() < propability;
        }

        // public to access for unittesting
        public Random random = new Random();
    }







    public class Linearizer {
        ICausalSetSystemBlockAccessor causalSystemBlockAccessor = new CausalSetSystemBlockAccessor();

        public Random random = new Random();

        // \param recursive do we recurse down if the embeded block is present
        public IList<uint> linearize(CausalSetSystemBlock causalSystemBlock, bool recusive = false) {
            IList<uint> linearization = new List<uint>(); // (global) indices of nodes of the linearlization
            linearizeInternal(causalSystemBlock, linearization, recusive);
            return linearization;
        }
        
        void linearizeInternal(CausalSetSystemBlock causalSystemBlock, IList<uint> linearization, bool recusive) {
            causalSystemBlock.updateEntryIndices(); // we calculate this always new because we don't care about performance for now
            IList<uint> indicesOfEntryNodes = causalSystemBlockAccessor.getEntryIndicesAsList(causalSystemBlock);

            IList<uint> openNodeIndices = ListHelpers.copy(indicesOfEntryNodes);
            ISet<uint> nodeIndicesInLinearization = new HashSet<uint>(); // set, bool values hae no meaning

            while (!openNodeIndices.isEmpty() ) {
                uint currentCandidateNodeIndex;

                {// choose random element from openNodeIndices and remove
                    uint candidateIndexOfOpenNodeIndices = (uint)random.Next(openNodeIndices.Count);
                    currentCandidateNodeIndex = openNodeIndices[(int)candidateIndexOfOpenNodeIndices];
                    openNodeIndices.RemoveAt((int)candidateIndexOfOpenNodeIndices);
                }

                Ensure.ensureHard(!nodeIndicesInLinearization.Contains(currentCandidateNodeIndex));
                nodeIndicesInLinearization.Add(currentCandidateNodeIndex);

                // if we can and should recurse down
                if( recusive && causalSystemBlock.nodes[(int)currentCandidateNodeIndex].content != null ) {
                    CausalSetSystemBlock childrenBlock = causalSystemBlock.nodes[(int)currentCandidateNodeIndex].content;
                    linearizeInternal(childrenBlock, linearization, recusive);
                }
                else {
                    // the global index must be valid
                    linearization.Add(causalSystemBlock.nodes[(int)currentCandidateNodeIndex].globalIndex.Value);
                }
                

                ISet<uint> nextIndicesOfCurrentCandidate = SetHelpers.subtract(causalSystemBlockAccessor.getNextIndicesOfNodeAsSet(causalSystemBlock, currentCandidateNodeIndex), nodeIndicesInLinearization);
                openNodeIndices = SetHelpers.union(SetHelpers.toSet(openNodeIndices), nextIndicesOfCurrentCandidate).ToList();
            }
        }
    }

    public interface ICausalSetSystemBlockAccessor {
        IList<uint> getEntryIndicesAsList(CausalSetSystemBlock causalSystemBlock);
        ISet<uint> getNextIndicesOfNodeAsSet(CausalSetSystemBlock causalSystemBlock, uint nodeIdx);
    }

    // implementation of CausalSystemBlockAccessor for the type "CausalSystemBlock"
    public class CausalSetSystemBlockAccessor : ICausalSetSystemBlockAccessor {
        public IList<uint> getEntryIndicesAsList(CausalSetSystemBlock causalSystemBlock) {
            return causalSystemBlock.entryIndices;
        }

        public ISet<uint> getNextIndicesOfNodeAsSet(CausalSetSystemBlock causalSystemBlock, uint nodeIdx) {
            uint[] nodeIndicesAsList = causalSystemBlock.nodes[(int)nodeIdx].next.Select(n => causalSystemBlock.indirectionArray[(int)n.value]).ToArray();
            return SetHelpers.toSet(nodeIndicesAsList);
        }
    }



    public sealed class ArrayHelpers {
        public static List<uint> copyAsList(uint[] arr) {
            List<uint> result = new List<uint>();
            for (int i = 0; i < arr.Length; i++) {
                result.Add(arr[i]);
            }
            return result;
        }

        internal static IList<Type> copy<Type>(IList<Type> list) {
            IList<Type> result = new List<Type>();
            foreach(Type iValue in list) {
                result.Add(iValue);
            }
            return result;
        }
    }
    


    // public for unittest
    public sealed class CausalSetNodeFuser {
        // creates a new block with the content as the fused nodes and fuses the nodes in the "entryBlock" and returns the created "CausalSystemBlock" which contains all fused nodes on the next level
        // entryblock doesn't get modified
        static public CausalSetSystemBlock fuse(CausalSetSystemBlock entryBlock, out CausalSetSystemBlock modifiedEntryBlock, IList<uint> indices) {
            CausalSetSystemBlock modifiedEntryBlock_localVar = entryBlock.copyDeep();

            CausalSetSystemBlock higherLevelBlock = new CausalSetSystemBlock();

            // fill higherLevelBlock with as many nodes as required
            foreach(uint iterationIndex in indices) {
                CausalSetNode createdNode = new CausalSetNode();
                createdNode.nodeIndexInParentSystemBlock = iterationIndex;
                higherLevelBlock.nodes.Add(createdNode);

                higherLevelBlock.indirectionArray.Add(higherLevelBlock.getNewIndirectionNumber());
            }

            // translates the index in entryBlock to the index in higherLevelBlock
            Dictionary<uint, uint> indexToElementInHigherLevelBlock = new Dictionary<uint, uint>();
            for (uint i = 0; i < indices.Count; i++) {
                uint redirectedIndex = indices[(int)i];
                indexToElementInHigherLevelBlock[redirectedIndex] = i;
            }


            // rewire from entryBlock the subnetwork to HigherLevelBlock
            foreach (Tuple<uint, CausalSetNode> iterationIndexAndNodePair in indices.Select(index => new Tuple<uint, CausalSetNode>(index, modifiedEntryBlock_localVar.getNodeByIndex(index)))) {
                uint higherLevelBlock_index = indexToElementInHigherLevelBlock[iterationIndexAndNodePair.Item1];

                var nextValidIndicesFromIterationNode =
                    iterationIndexAndNodePair.Item2.next
                        .Select(nextIndirection => modifiedEntryBlock_localVar.translateIndirectIndexToIndex(nextIndirection))
                        .Where(index => indexToElementInHigherLevelBlock.ContainsKey(index)); // if the node points to an node which is not in nextLevelBlock then we ignore it

                foreach (uint iterationNextValidIndexFromIterationNode in nextValidIndicesFromIterationNode) {
                    uint higherLevelBlock_nextIndex = indexToElementInHigherLevelBlock[iterationNextValidIndexFromIterationNode];

                    // make sure that the indirection is an identity
                    // only this way we don't have to do an inverse lookup to get the CausalIndirectionIndex
                    Ensure.ensureHard(higherLevelBlock.translateIndirectIndexToIndex(new CausalIndirectionIndex(higherLevelBlock_nextIndex)) == higherLevelBlock_nextIndex);

                    higherLevelBlock.nodes[(int)higherLevelBlock_index].next.Add(new CausalIndirectionIndex(higherLevelBlock_nextIndex));
                }
            }

            // transfer global index
            // global indices of fused nodes get reset to null by fuse so we ignore them
            for( uint i = 0; i < indices.Count; i++ ) {
                uint entryBlockIndex = indices[(int)i];
 
                uint higherLevelBlock_index = indexToElementInHigherLevelBlock[entryBlockIndex];

                higherLevelBlock.nodes[(int)higherLevelBlock_index].globalIndex = modifiedEntryBlock_localVar.nodes[(int)entryBlockIndex].globalIndex;
            }

            // now do the fuse
            uint fuseAsIndirectionNumber = modifiedEntryBlock_localVar.getNewIndirectionNumber();
            modifiedEntryBlock_localVar.fuse(indices, fuseAsIndirectionNumber);

            // add fused node
            modifiedEntryBlock_localVar.nodes.Add(new CausalSetNode(higherLevelBlock));

            // remove fused nodes and rewire indirectionArray accordingly
            indices.ToList().Sort();
            var reversedSortedIndices = indices.Reverse();
            foreach ( uint iterationNodeIndex in reversedSortedIndices ) {
                // redirect all indices pointing at the element behind the removed one to one before it
                modifiedEntryBlock_localVar.indirectionArray = modifiedEntryBlock_localVar.indirectionArray.Select(v => v > iterationNodeIndex ? v - 1 : v).ToList();
                
                modifiedEntryBlock_localVar.nodes.RemoveAt((int)iterationNodeIndex);
            }

            modifiedEntryBlock = modifiedEntryBlock_localVar;

            return higherLevelBlock;
        }
    }

    public class GlobalLinearization {
        public static GlobalLinearization make(IList<uint> linearization) {
            return new GlobalLinearization(linearization);
        }

        private GlobalLinearization(IList<uint> linearization) {
            this.linearization = linearization;
        }

        public IList<uint> linearization;
    }

    // 
    /**
     * Non interruptable simple implementation.
     * 
     * 
     * divides recursivly the dag into subparts which are then tried to be optimized.
     * 
     * the energy is calculated globally.
     * not optimzed for speed.
     * 
     * 
     */
    public class NonanytimeEntropyMinimizer {
        // \param terminationPropability propability of termination while searching for children nodes in the block of an node
        public NonanytimeEntropyMinimizer(Linearizer linearizer, float terminationPropability) {
            this.linearizer = linearizer;

            this.terminationPropability = terminationPropability;

        }

        public void entry(CausalSetSystemBlock entryBlock) {
            Ensure.ensureHard(rootRewriteTreeElement == null);

            buildTree(entryBlock);
        }

        // called from the outside to do work
        // PROTOTYPING< should be called just once because it doesn't work in an anytime manner jet >
        public void anytimeInvoke() {
            calcEntropyOfAllElementsInTree();
        }


        private void unfoldAndOptimizeAndStore(IList<RewriteTreeElement> rewriteTreeElementsFromRootToChildrens) {
            RewriteTreeElement lastChildrenRewriteTreeElement = rewriteTreeElementsFromRootToChildrens[rewriteTreeElementsFromRootToChildrens.Count - 1];

            // we have to copy because CausalSetNodeFuser.fuse() modifies the argument
            CausalSetSystemBlock modifiedRootSystemBlock = unmodifiedRootSystemBlock.copyShallow();

            CausalSetSystemBlock currentTopBlock = unmodifiedRootSystemBlock;

            for(int rewriteChainIndex = 0; rewriteChainIndex < rewriteTreeElementsFromRootToChildrens.Count(); rewriteChainIndex++) {
                RewriteTreeElement iterationRewriteTreeElement = rewriteTreeElementsFromRootToChildrens[rewriteChainIndex];

                CausalSetSystemBlock modifiedParent;
                CausalSetSystemBlock afterFusion = CausalSetNodeFuser.fuse(currentTopBlock, out modifiedParent, iterationRewriteTreeElement.indicesOfParentNodesForRewrite.ToList());

                currentTopBlock = afterFusion;
            }


            // linearize and calculate energy, store back to tree (in the children)
            IList<uint> globalLinearized = linearizer.linearize(modifiedRootSystemBlock, /*recursive*/true);
            lastChildrenRewriteTreeElement.globalLinearized = GlobalLinearization.make(linearizer.linearize(modifiedRootSystemBlock, /*recursive*/true));
            Debug.Assert(lastChildrenRewriteTreeElement.globalLinearized.linearization.Count == unmodifiedRootSystemBlock.nodes.Count);

            //  calculate global energy
            lastChildrenRewriteTreeElement.globalEnergyAfterRewrite = calculateEnergy(lastChildrenRewriteTreeElement.globalLinearized, modifiedRootSystemBlock);

            /*
            rewriteTreeElements[0].blockBeforeRewrite
                .guranteeNoChildren() // make sure that there are no children Blocks
                .deepCopy()
                .fuse(rewriteTreeElements[0].indicesOfParentNodesForRewrite)
                */
        }

        private long calculateEnergy(GlobalLinearization linearization, CausalSetSystemBlock root) {
            IDictionary<uint, uint> indexInLinearizationByIndex = new Dictionary<uint, uint>();
            for( uint i = 0; i < linearization.linearization.Count; i++) {
                uint nodeIndex = linearization.linearization[(int)i];
                Ensure.ensureHard(!indexInLinearizationByIndex.ContainsKey(nodeIndex));
                indexInLinearizationByIndex[nodeIndex] = i;
            }

            long energy = 0;
            
            for( uint i = 0; i < linearization.linearization.Count; i++) {
                uint nodeIndex = linearization.linearization[(int)i];
                
                foreach( CausalIndirectionIndex nextNodeIndicesFromCurrentNode in unmodifiedRootSystemBlock.nodes[(int)nodeIndex].next ) {
                    uint nextNodeIndexFromCurrentNode = root.translateIndirectIndexToIndex(nextNodeIndicesFromCurrentNode);

                    // translate the index of the next node which follows the current node to the index in the linearization
                    uint indexInLinearization = indexInLinearizationByIndex[nextNodeIndexFromCurrentNode];

                    // calculate energy from current node to next node based on linearization
                    Debug.Assert(indexInLinearization >= 0);
                    Debug.Assert(nodeIndex >= 0);
                    int energyFromCurrentNodeToNextNode = (int)indexInLinearization - (int)nodeIndex;

                    // must be greater than zero else we have some really nasty bug in the code
                    Ensure.ensureHard(energyFromCurrentNodeToNextNode > 0);

                    energy += energyFromCurrentNodeToNextNode;
                }
            }

            return energy;
        }

        private void calcEntropyOfAllElementsInTree() {
            // iterate over all tree nodes, calculate best linearlization of each node and the corresponding energy

            // call into an helper which calls unfoldAndOptimizeAndStore
            calcEntropyOfAllElementsInTreeRecursive(rootRewriteTreeElement);
        }

        private void calcEntropyOfAllElementsInTreeRecursive(RewriteTreeElement rewriteTreeElement) {
            // calculate energy, of the whole permutation described by rewriteTreeElement and store the best found global linearization and the corresponding energy
            {
                IEnumerable<RewriteTreeElement> pathFromIterationChildrenToParentToRoot = getRewriteTreeElementsFromParentsToRoot(rewriteTreeElement);
                IEnumerable<RewriteTreeElement> pathFromRootToIterationChildren = pathFromIterationChildrenToParentToRoot.Reverse();

                Debug.Assert(pathFromRootToIterationChildren.Count() > 0);
                // if we are at the RewriteTreeElement which describes the root then we can't calculate the entropy because there is no permutation
                if (rewriteTreeElement != rootRewriteTreeElement) {
                    unfoldAndOptimizeAndStore(pathFromRootToIterationChildren.ToList());
                }
            }

            foreach (RewriteTreeElement iteratorChildren in rewriteTreeElement.childrenRewrites ) {
                calcEntropyOfAllElementsInTreeRecursive(iteratorChildren);
            }
        }

        private IEnumerable<RewriteTreeElement> getRewriteTreeElementsFromParentsToRoot(RewriteTreeElement startParent) {
            IList<RewriteTreeElement> result = new List<RewriteTreeElement>();

            RewriteTreeElement currentElement = startParent;
            for(;;) {
                if( currentElement == null ) {
                    break;
                }

                result.Add(currentElement);

                currentElement = currentElement.parent;
            }

            return result;
        }

        public GlobalLinearization getBestGlobalLinearizationAndEnergy(out long minimalEnergy) {
            GlobalLinearization bestLinearization = null;
            minimalEnergy = (long)0x7FFFFFFFFFFFFFFF;
            getBestGlobalLinearizationAndEnergyRecursive(rootRewriteTreeElement, ref minimalEnergy, ref bestLinearization);

            Ensure.ensureHard(bestLinearization != null);

            return bestLinearization;

        }

        private void getBestGlobalLinearizationAndEnergyRecursive(RewriteTreeElement entry, ref long minimalEnergy, ref GlobalLinearization linearization) {
            if (entry != rootRewriteTreeElement) {
                Ensure.ensureHard(entry.globalLinearized != null);
                if (entry.globalEnergyAfterRewrite < minimalEnergy) {
                    minimalEnergy = entry.globalEnergyAfterRewrite;
                    linearization = entry.globalLinearized;
                }
            }

            foreach (RewriteTreeElement iterationChildren in entry.childrenRewrites ) {
                getBestGlobalLinearizationAndEnergyRecursive(iterationChildren, ref minimalEnergy, ref linearization);
            }
        }

        public void finish() {
            invalidateRewriteTree();
        }

        private void invalidateRewriteTree() {
            rootRewriteTreeElement = null;
        }

        void buildTree(CausalSetSystemBlock entryBlock) {
            // store it for later reference for calculating the global energy
            unmodifiedRootSystemBlock = entryBlock;

            rootRewriteTreeElement = new RewriteTreeElement();
            buildRewriteTreeRecursive(rootRewriteTreeElement, entryBlock.copyDeep());
        }

        void buildRewriteTreeRecursive(RewriteTreeElement parentRewriteTreeElement, CausalSetSystemBlock recursedSystemBlock) {
            uint rewriteRounds = 1; // for now we just do one rewrite for testing
            for (uint rewriteRound = 0; rewriteRound < rewriteRounds; rewriteRound++) {
                uint fuseEntryIndex = (uint)random.Next(recursedSystemBlock.nodes.Count);

                RewriteTreeElement rewriteTreeElementAfterRewrite = new RewriteTreeElement();
                rewriteTreeElementAfterRewrite.indicesOfParentNodesForRewrite = recursedSystemBlock.getRandomFollowerAsEnumerable(fuseEntryIndex, terminationPropability);

                /// uncommented, let it be null, because we don't need it
                //rewriteTreeElementAfterRewrite.blockBeforeRewrite = recursedSystemBlock;

                

                // rewrite
                rewriteTreeElementAfterRewrite.blockAfterRewrite = CausalSetNodeFuser.fuse(recursedSystemBlock, out recursedSystemBlock, rewriteTreeElementAfterRewrite.indicesOfParentNodesForRewrite.ToList());

                parentRewriteTreeElement.childrenRewrites.Add(rewriteTreeElementAfterRewrite);
            }

            
        }

        /* uncommented because some old stub from 26.02.2017
        // unrolls all recursive CausalSetSystemBlock into one (valid) sequence,
        // calculate the energy/entropy
        long calcEntropyRecursiveLinearized(CausalSetSystemBlock globalForEntropyCalculation) {
            IList<uint> linearized = linearizer.linearize(globalForEntropyCalculation, true);

            // TODO< calculate energy of linearized >
            throw new NotImplementedException(); // TODO TODOT DOTO

        }
        */

        /* Tree to store the rewrite progress
         */
        class RewriteTreeElement {
            public RewriteTreeElement parent;

            //uint? parentIndex; // where is the block in the parent CausalSetSystemBlock
            //public CausalSetSystemBlock blockBeforeRewrite; // which block was used for rewrite, is before the rewrite
            public CausalSetSystemBlock blockAfterRewrite;


            
            public IEnumerable<uint> indicesOfParentNodesForRewrite;

            public IList<RewriteTreeElement> childrenRewrites = new List<RewriteTreeElement>(); // all rewrites done to and within this Block "rewrittenBlock"

            public int numberOfRemainingRewrites;

            //public uint depth; // depth of the "RewriteTreeElement"

            public long globalEnergyAfterRewrite;
            public GlobalLinearization globalLinearized; // global linearized sequence, not locally linearized (where local is just from this CausalSetSystemBlock and all children CausalSetSystemBlock)
        }

        RewriteTreeElement rootRewriteTreeElement;

        Linearizer linearizer;

        float terminationPropability;

        public Random random = new Random(); // public to make it possible to set random from outside for deterministic testcases

        CausalSetSystemBlock unmodifiedRootSystemBlock;
    }

    sealed class ListHelpers {
        public static IList<Type> copy<Type>(IList<Type> list) {
            IList<Type> result = new List<Type>();
            foreach(Type v in list) {
                result.Add(v);
            }
            return result;
        }
    }
}
