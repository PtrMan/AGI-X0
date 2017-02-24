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





    /* new algorithm D version we translated from
        * 
        * 

        * 
        * 
        // small simple (slow) implementation of causal set algorithm(s)
// from 2017

import std.typecons : Typedef;
import std.random : uniform01, uniform;

// definition of indirex indirection type
// used to point with an index to the index of an element in the causal set
alias Typedef!long IdxInd;

struct Node {
    IdxInd[] next; // points to the children elements of this element

    CausalSystemBlock *content; // embeded block, can be null

    final void update() {
        next = ().keys;
    }
}

// every block has it's local own nodes and indirection array, the indices are not globally defined
struct CausalSystemBlock {
    // indirection to nodes 
    size_t[] indirectionArray;

    Node*[] nodes;

    final size_t translateIndirectIndexToIndex(IdxInd idxInd) {
        return indirectionArray[cast(size_t)idxInd];
    }

    uint indirectionCounter; // counter, doesn't reflect the # of elements in the arrays

    // used to determine the unused id of indirection elements
    final uint getNewIndirectionNumber() {
    return indirectionCounter++;
    }

    size_t[] entryIndices; // indices to "nodes" of the elements which have no parents
}

// fuses indirections to nodes to just one node
void fuse(CausalSystemBlock *causalSystemBlock, size_t[] indices, size_t fusedAsIndex) {
    foreach( iIdx; indices ) {
        causalSystemBlock.indirectionArray[iIdx] = fusedAsIndex;
    }
}


void chooseRandomEntryAndFuseRandomElements(CausalSystemBlock *causalSystemBlock, float terminationPropability, size_t fusedAsIndex) {
    size_t[] entryIndices = causalSystemBlock.getEntryIndices();
    size_t randomEntryIdx = entryIndices[random(entryIndices.length)];

    size_t[] randomFollowerSetIndices = causalSystemBlock.getRandomFollowerSet(randomEntryIdx, terminationPropability).keys;
    causalSystemBlock.fuse(randomFollowerSetIndices, fusedAsIndex);
}

size_t[] getRandomFollowerList(CausalSystemBlock *causalSystemBlock, size_t entryIdx, float terminationPropability) {
    return getRandomFollowerSet(causalSystemBlock, entryIdx, terminationPropability).keys;
}

// bool[] just as a dummy for a set
bool[size_t] getRandomFollowerSet(CausalSystemBlock *causalSystemBlock, size_t entryIdx, float terminationPropability) {
    bool[size_t] resultSet;
    resultSet[entryIdx] = true; // entry ind must be included
    getRandomFollowerSetInternalRecursive(causalSystemBlock, entryIdx, resultSet, terminationPropability);
    return resultSet;
}

void getRandomFollowerSetInternalRecursive(CausalSystemBlock *causalSystemBlock, size_t entryIdx, ref bool[size_t] followerSet, float terminationPropability) {
    bool terminate = randombool(terminationPropability);
    if(terminate) {
        return;
    }

    followerSet[entryIdx] = true;

    Node *selectedNode = causalSystemBlock.nodes[entryIdx];
    foreach( iFollowerIndirectionIdx; selectedNode.next ) {
        size_t followerIndex = causalSystemBlock.translateIndirectIndexToIndex(iFollowerIndirectionIdx);
        getRandomFollowerSetInternalRecursive(causalSystemBlock, followerIndex, followerSet, terminationPropability);
    }
}

extern bool randombool(float propability) {
    return propability < uniform01!float;
}

extern size_t random(size_t range) {
    return uniform!"[)"(0, range);
}

import std.stdio;

void main() {
    CausalSystemBlock *testCausalSystemBlock = new CausalSystemBlock;

    // TODO< fill it >


    float terminationPropability = 0.1f;
    size_t entryIdx = 0;
    size_t[] result = getRandomFollowerList(testCausalSystemBlock, entryIdx, terminationPropability);
    writeln(result);
}


class Linearization(NodeType, NodeAccessor, CausalSystemBlockType, CausalSystemBlockAccessor) {
    final size_t[] linearlize(CausalSystemBlockType causalSystemBlock) {
        size_t[] linearlization; // indices of nodes of the linearlization

        size_t[] indicesOfEntryNodes = CausalSystemBlockAccessor.getEntryIndicesAsList(causalSystemBlock);

        size_t[] openNodeIndices = indicesOfEntryNodes.dup;
        bool[size_t] nodeIndicesInLinearlization; // set, bool values hae no meaning

        while( openNodeIndices.length != 0 ) {
            size_t currentCandidateNodeIndex;

            {// choose random element from openNodeIndices and remove
                size_t candidateIndexOfOpenNodeIndices = random(openNodeIndices.lengh);
                currentCandidateNodeIndex = openNodeIndices[candidateIndexOfOpenNodeIndices];
                openNodeIndices = openNodeIndices.remove(candidateIndexOfOpenNodeIndices);
            }

            assert(!(currentCandidateNodeIndex in nodeIndicesInLinearlization));
            nodeIndicesInLinearlization[currentCandidateNodeIndex] = true;

            linearlization ~= currentCandidateNodeIndex;

            bool[size_t] nextIndicesOfCurrentCandidate = setSubtract(CausalSystemBlockAccessor.getNextIndicesOfNodeAsSet(causalSystemBlock, currentCandidateNodeIndex), nodeIndicesInLinearlization);
            openNodeIndices = setUnion(toSet(openNodeIndices), nextIndicesOfCurrentCandidate).keys;
        }

        return linearlization;
    }
}

// implementation of CausalSystemBlockAccessor for the type "CausalSystemBlock"
struct CausalSystemBlockAccessor {
    static size_t[] getEntryIndicesAsList(CausalSystemBlock *causalSystemBlock) {
        return causalSystemBlock.entryIndices;
    }

    static bool[size_t] getNextIndicesOfNodeAsSet(CausalSystemBlock *causalSystemBlock, size_t nodeIdx) {
        size_t[] nodeInicesAsList = causalSystemBlock.nodes[nodeIdx].next.map!(n => causalSystemBlock.indirectionArray[n]).array;
        return toSet(nodeIndicesAsList)
    }
}



// set helpers
bool[size_t] toSet(size_t[] arr) {
    bool[size_t] result;
    foreach( iArr; arr ) {
        result[iArr] = true;
    }
    return result;
}

bool[size_t] setSubtract(bool[size_t] a, bool[size_t] b) {
    bool[size_t] result;
    foreach( ia; a.keys ) {
        if( !(ia in b) ) {
            result[ia] = true;
        }
    }
    return result;
}

bool[size_t] setUnion(bool[size_t] a, bool[size_t] b) {
    bool[size_t] result = a.dup;
    foreach( bi; b.keys ) {
        result[bi] = true;
    }
    return result;
}



        * 
        * 
        */















    /* old CAUAL algorithm
    static class ListExtensions {
        public static bool isEmpty<Type>(this IList<Type> arr) {
            return arr.Count != 0;
        }
    }

    static class ListQueueExtensions {
        public static uint dequeue(IList<uint> arr) {
            var result = arr[0];
            arr.RemoveAt(0);
            return result;
        }
    }

    class CausalDagElement {
        public IList<uint> childrenIndices = new List<uint>();

        public bool marker;
        public bool markerEnd;
        public uint weight = 1; // how "wide" is the element in the sequence?
    }

    class CausalDag {
        public delegate void ApplyDelegte( CausalDagElement element, uint elementIndex);
        public delegate bool TerminationCriteriaDelegate( CausalDagElement element, uint elementIndex);

        public IList<CausalDagElement> elements = new List<CausalDagElement>();

        public void applyToAllChildrenUntilTerminationCriteria(uint entry, ApplyDelegte apply, TerminationCriteriaDelegate terminationCriteria) {
            IList<uint> openList = new List<uint>{entry};            
            ISet<uint> visited = new HashSet<uint>();

            while(!ListExtensions.isEmpty(openList)) {
                uint currentIndex = ListQueueExtensions.dequeue(openList);

                if( visited.Contains(currentIndex) ) {
                    continue;
                }
                visited.Add(currentIndex);

                if( terminationCriteria(this.elements[(int)currentIndex], currentIndex) ) {
                    continue;
                }

                foreach(uint iIdx in this.elements[(int)currentIndex].childrenIndices) {
                    openList.Add(iIdx);
                }

                apply(this.elements[(int)currentIndex], currentIndex);
            }
        }

        public void markChildrenUntilEndMarker(uint entry) {
            ApplyDelegte innerFnApply = delegate(CausalDagElement element, uint elementIndex) {
                element.marker = true;
            };

            TerminationCriteriaDelegate innerFnTerminationCriteria = delegate(CausalDagElement element, uint elementIndex) {
                return element.markerEnd;
            };

            applyToAllChildrenUntilTerminationCriteria(entry, innerFnApply, innerFnTerminationCriteria);
        }

        public static void unionAllFollowers(ISet<uint> union_, uint entry) {
            ApplyDelegte innerFnApply = delegate(CausalDagElement element, uint elementIndex) {
                union_.Add(elementIndex);
            };

            TerminationCriteriaDelegate innerFnTerminationCriteria = delegate(CausalDagElement element, uint elementIndex) {
                return false;
            };

            applyToAllChildrenUntilTerminationCriteria(entry, innerFnApply, innerFnTerminationCriteria);
        }






        public void markRandomFollowersAsEnd(IList<uint> entryIndices, uint numberOfEndPoints) {
            ISet<uint> candidateIndicesMap = new HashSet<uint>();

            foreach( uint iterationEntryIndex in entryIndices ) {
                unionAllFollowers(candidateIndicesMap, iterationEntryIndex);
            }

            foreach( uint iterationIndexToMark in pickRandomN(candidateIndicesMap.ToList(), numberOfEndPoints) ) {
                this.elements[(int)iterationIndexToMark].markerEnd = true;
            }
        }

        private delegate IList<uint> ChildIndicesOfMarkedNodesDelegateType();

        // fuses all marked elements into one element in the result dag
        public CausalDag fuseMarkedElements() {
            CausalDag result = new CausalDag();

            // points at the coresponding elements in the result dag from the inputDag
            // value can be -1 if there is no coresponding element in the result
            IList<int> indicesToDagElementsInResult = new List<int>();



            // build indicesToDagElementsInResult
            foreach( CausalDagElement iterationElement in this.elements ) {
                if( iterationElement.marker ) {
                    indicesToDagElementsInResult.Add(-1);			
                }
                else {
                    indicesToDagElementsInResult.Add(result.elements.Count);
                    result.elements.Add(new CausalDagElement());
                }
            }

            uint indexOfColapsedElement = (uint)result.elements.Count;
            result.elements.Add(new CausalDagElement()); // add the colapsed element


            ChildIndicesOfMarkedNodesDelegateType innerFnChildIndicesOfMarkedNodes = delegate() {
                IList<uint> childIndicesOfMarkedNodes = new List<uint>();

                foreach( var iElement in this.elements ) {
                    if( iElement.marker ) {
                        // add children
                        foreach( uint iChildrenIndex in iElement.childrenIndices) {
                            childIndicesOfMarkedNodes.Add(iChildrenIndex);
                        }
                    }
                }

                return new HashSet<uint>(childIndicesOfMarkedNodes).ToList(); // get unique elements
            };

            var remapped = innerFnChildIndicesOfMarkedNodes().Select(v => (uint)indicesToDagElementsInResult[(int)v]);
            var remappedChildrenIndicesOfMarkedNodes = new HashSet<uint>(remapped).ToList(); // get unique elements

            result.elements[(int)indexOfColapsedElement].childrenIndices = remappedChildrenIndicesOfMarkedNodes;


            // remap childrenIndices of elements which get transfered
            uint elementIndex = 0;
            foreach( CausalDagElement iterationElement in this.elements ) {
                int remappedIndex = indicesToDagElementsInResult[(int)elementIndex];

                if( remappedIndex == -1 ) {
                    elementIndex++;
                    continue;
                }

                IEnumerable<uint> remappedChildrenIndices = iterationElement.childrenIndices
                    .Select(childrenIndex => indicesToDagElementsInResult[(int)childrenIndex]) // rewire indices
                    .Select(v => v == -1 ? indexOfColapsedElement : (uint)v); // point all children to the colapsed node to the new collapsed node
                remappedChildrenIndices = new HashSet<uint>(remappedChildrenIndices).ToList(); // remove duplicates


                result.elements[remappedIndex].childrenIndices = remappedChildrenIndices.ToArray();

                // we need an backpointer to the elements which corresponds to the remapped element
                //result.elements[remappedIndex].parentIndex = elementIndex; // uncommented because we dont need it jet


                elementIndex++;
            }

            return result;
        }

        Random random = new Random();

        // helper
        IList<uint> pickRandomN(IList<uint> arrIn, uint n) {
            // deep copy
            IList<uint> arr = new List<uint>();
            foreach (uint i in arrIn) {
                arr.Add(i);
            }


            Debug.Assert(arr.Count >= n);

            IList<uint> result = new List<uint>();

            for( int i = 0; i < n; i++ ) {
                uint index = (uint)random.Next(0, arr.Count);
                result.Add(arr[(int)index]);
                arr.RemoveAt((int)index);
            }

            return result;
        }








        // multi level energy minimization




    }
        * 
        * */






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

            /* todo - unittest
    assert(resultDag.elements.length == 2);

    assert(resultDag.elements[0].childrenIndices == []);
    assert(resultDag.elements[1].childrenIndices == [0]); // the collapsed node must point at the last element (which is now the first)
            */
        }

        /*
        static public void Consumer()
        {
            foreach (int i in Integers())
            {
                Console.WriteLine(i.ToString());
            }
        }

        static public IEnumerable<int> Integers()
        {
            yield return 1;
            yield return 2;
            yield return 4;
            yield return 8;
            yield return 16;
            yield return 16777216;
        }
            * */
    }
}
