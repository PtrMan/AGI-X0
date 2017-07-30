using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using MetaNix.misc;

namespace MetaNix.schmidhuber.slimRnn {
    // "Universal SLIM NN Search" as described by Schmidhuber https://arxiv.org/pdf/1210.0118.pdf 
    // chapter   3.2 (Incremental Adaptive) Universal Search for SLIM NNs
    public class UniversalSlimRnnSearch : ISlimRnnLearningAlgorithm {
        public class WeightWithPropability {
            public WeightWithPropability(float weight, double propability) {
                this.weight = weight;
                this.propability = propability;
            }

            public readonly float weight;
            public double propability; // normalized
        }

        public ITaskSolvedAndVerifiedTester tester;

        public UniversalSlimRnnSearch(SlimRnn slimRnn, ITaskSolvedAndVerifiedTester tester) {
            this.slimRnn = slimRnn;
            this.tester = tester;
        }

        // /param mustHalt has the SLIM-RNN to halt to be a valid solution?
        public void search(uint maximalIteration, bool mustHalt, out bool wasSolved, out SlimRnn solutionRnn) {
            wasSolved = false;
            solutionRnn = null;

            slimRnn.learningAlgorithm = this;

            for( uint levinSearchIteration = 1; levinSearchIteration <= maximalIteration; levinSearchIteration++ ) {
                iteration(levinSearchIteration, mustHalt, out wasSolved, out solutionRnn);
                if( wasSolved ) {
                    goto reset;
                }
            }

            reset:
            slimRnn.learningAlgorithm = null;
        }

        // /param mustHalt has the SLIM-RNN to halt to be a valid solution?
        void iteration(uint levinSearchIteration, bool mustHalt, out bool wasSolved, out SlimRnn solutionRnn) {
            depthFirstSearch(levinSearchIteration, mustHalt, out wasSolved, out solutionRnn);
        }


        // depth first search of the changes of the weights of the SLIM-RNN
        // 
        // this version doesn't timeshare the computations (to save RAM)

        // /param mustHalt has the SLIM-RNN to halt to be a valid solution?
        void depthFirstSearch(uint levinSearchIteration, bool mustHalt, out bool wasSolved, out SlimRnn solutionRnn) {
            solutionRnn = null;
            wasSolved = false;

            WeightChangeTreeElement weightChangeTreeRoot = WeightChangeTreeElement.makeRoot();

            currentWeightChangeTreeElement = null; // to ignore calls to ISlimRnnLearningAlgorithm.opportunityToAdjustWeight()

            // scan all weights for eligable weights
            List<SlimRnnNeuronWithWeight> eligibleWeights = new List<SlimRnnNeuronWithWeight>();
            foreach( SlimRnnNeuron iNeuron in slimRnn.neurons ) {
                eligibleWeights.AddRange(iNeuron.outNeuronsWithWeights.Where(v => v.isEligable));
            }

            // build tree of the possible weight changes
            //
            // by iterating over all elements in the trace and adding all possible weight values to the weightChangeTree
            {
                List<WeightChangeTreeElement>
                    weightChangeTreeLeafElements = new List<WeightChangeTreeElement> { weightChangeTreeRoot },
                    nextWeightChangeTreeLeafElements = new List<WeightChangeTreeElement>();

                foreach (SlimRnnNeuronWithWeight iTrace in eligibleWeights) {
                    foreach (WeightChangeTreeElement iWeightChangeTreeElement in weightChangeTreeLeafElements) {
                        /*
                        for (
                            uint weightWithPropabilityTableIndex = 0;
                            weightWithPropabilityTableIndex < weightWithPropabilityTable.Count;
                            weightWithPropabilityTableIndex++
                        ) {

                            WeightChangeTreeElement createdWeightChangeTreeElement = WeightChangeTreeElement.make(iTrace, weightWithPropabilityTableIndex, *parent**iWeightChangeTreeElement);
                            iWeightChangeTreeElement.children.Add(createdWeightChangeTreeElement);
                            iWeightChangeTreeElement.childrenConnectionNeuronIndices.Add(new Tuple<uint, uint>(iTrace.source.neuronIndex, iTrace.target.neuronIndex));

                            nextWeightChangeTreeLeafElements.Add(createdWeightChangeTreeElement); // keep track of new leaf elements of the weight change tree
                        }*/
                        nextWeightChangeTreeLeafElements.AddRange(createWeightChangeTreeElementsForConnectionAndAddToParent(iTrace, iWeightChangeTreeElement));
                    }

                    weightChangeTreeLeafElements = nextWeightChangeTreeLeafElements;
                    nextWeightChangeTreeLeafElements = new List<WeightChangeTreeElement>();
                }
            }

            // set all weights of the trace to zero
            // we do this because the connections are this way inactive and this avoids any call to  ISlimRnnLearningAlgorithm.opportunityToAdjustWeight() for 
            // connections which are already inside the weightChangeTree

            foreach (var iNeuronWithWeight in eligibleWeights) {
                iNeuronWithWeight.weight = 0.0f;
            }

            // depth-first-search iterate and update the weightChange tree as necessary

            List<DepthFirstSearchStackElement> stack = new List<DepthFirstSearchStackElement>();
            stack.Clear();
            stack.push(DepthFirstSearchStackElement.make(weightChangeTreeRoot));

            while (!stack.isEmpty()) {
                DepthFirstSearchStackElement topStackElement = stack.pop();

                // calls to ISlimRnnLearningAlgorithm.opportunityToAdjustWeight() have to modify the tree
                currentWeightChangeTreeElement = topStackElement.treeElement;

                if (!currentWeightChangeTreeElement.isRoot) {

                    // do modification of SLIM-RNN

                    //    OPTIMIZATION TODO< check if we have to do this recursivly or if it leads to the right answer with the nonrecursive code, the recursive code is correct >
                    //    nonrecursive code:
                    //    currentWeightChangeTreeElement.neuronWithWeight.weight = weightWithPropabilityTable[(int)currentWeightChangeTreeElement.weightWithPropabilityTableIndex].weight;

                    //    recursive code

                    WeightChangeTreeElement currentWeightUpdateElement = currentWeightChangeTreeElement;
                    for (;;) {
                        if (/*unnecessary   currentWeightUpdateElement == null ||*/ currentWeightUpdateElement.isRoot) {
                            break;
                        }

                        currentWeightUpdateElement.neuronWithWeight.weight = weightWithPropabilityTable[(int)currentWeightUpdateElement.weightWithPropabilityTableIndex].weight;

                        currentWeightUpdateElement = currentWeightUpdateElement.parent;
                    }

                    //   we need to label all connections which got already adapted
                    currentWeightUpdateElement = currentWeightChangeTreeElement;
                    for (;;) {
                        if (/*unnecessary   currentWeightUpdateElement == null ||*/ currentWeightUpdateElement.isRoot) {
                            break;
                        }

                        var connectionTuple = new Tuple<uint, uint>(currentWeightUpdateElement.neuronWithWeight.source.neuronIndex, currentWeightUpdateElement.neuronWithWeight.target.neuronIndex);
                        Debug.Assert(!globalConnectionNeuronIndices.Contains(connectionTuple));
                        globalConnectionNeuronIndices.Add(connectionTuple);

                        currentWeightUpdateElement = currentWeightUpdateElement.parent;
                    }


                    // debug network
                    //SlimRnnDebug.debugConnections(slimRnn);
                    

                    double tLim = calcTimebound(levinSearchIteration, topStackElement.treeElement);
                    bool slimRnnSolvedTask = tester.doesSlimRnnSolveTask(slimRnn, mustHalt, tLim);
                    
                    if( slimRnnSolvedTask ) {

                        // the task has been solved with this network
                        wasSolved = true;
                        solutionRnn = slimRnn;
                        return;
                    }

                    // reset all touched connections to 0.0 to avoid any sideeffects

                    // OPTIMIZATION TODO< in the recursive version of depth-first-search we don't need to do this because we modify the network with each successive call,
                    //                    so in this version we don't have to reset the whole connections to null >

                    currentWeightUpdateElement = currentWeightChangeTreeElement;
                    for (;;) {
                        if (/*unnecessary   currentWeightUpdateElement == null ||*/ currentWeightUpdateElement.isRoot) {
                            break;
                        }

                        currentWeightUpdateElement.neuronWithWeight.weight = 0.0f;

                        currentWeightUpdateElement = currentWeightUpdateElement.parent;
                    }


                    //   we need to unlabel all connections which got already adapted
                    currentWeightUpdateElement = currentWeightChangeTreeElement;
                    for (;;) {
                        if (/*unnecessary   currentWeightUpdateElement == null ||*/ currentWeightUpdateElement.isRoot) {
                            break;
                        }

                        var connectionTuple = new Tuple<uint, uint>(currentWeightUpdateElement.neuronWithWeight.source.neuronIndex, currentWeightUpdateElement.neuronWithWeight.target.neuronIndex);
                        Debug.Assert(globalConnectionNeuronIndices.Contains(connectionTuple));
                        globalConnectionNeuronIndices.Remove(connectionTuple);

                        currentWeightUpdateElement = currentWeightUpdateElement.parent;
                    }

                }

                // push all children for depth-first-search
                foreach (var iTreeChildren in topStackElement.treeElement.children) {
                    stack.push(DepthFirstSearchStackElement.make(iTreeChildren));
                }
            }
        }

        // called from the SLIM RNN if it reads a weight which was not jet used
        // we can here decide to change the weight and to which value we have to change it
        void ISlimRnnLearningAlgorithm.opportunityToAdjustWeight(SlimRnnNeuronWithWeight neuronWithWeight) {
            if( currentWeightChangeTreeElement == null ) {
                // do nothing if the current weight change tree element is not set
                return;
            }

            Debug.Assert(!currentWeightChangeTreeElement.isRoot); // root node doesn't have any changed weights, so is invalid

            // do not add a new tree element if this connection was already added
            Tuple<uint, uint> connectionNeuronIndexTupleToCheck = new Tuple<uint, uint>(
                currentWeightChangeTreeElement.neuronWithWeight.source.neuronIndex,
                currentWeightChangeTreeElement.neuronWithWeight.target.neuronIndex);
            if (globalConnectionNeuronIndices.Contains(connectionNeuronIndexTupleToCheck) ) {
                return;
            }

            // do not add a new tree element if this connection was already added as a children of the current tree element
            if (currentWeightChangeTreeElement.childrenConnectionNeuronIndices.Contains(connectionNeuronIndexTupleToCheck)) {
                return;
            }

            // add new tree elements for the connection
            createWeightChangeTreeElementsForConnectionAndAddToParent(neuronWithWeight, currentWeightChangeTreeElement);

            // add connection to global
            // commented because it seems to be wrong, because it is not undone by the depth-first search    globalConnectionNeuronIndices.Add(connectionNeuronIndexTupleToCheck);
        }

        // we propably don't need this method at all in the whole C# program
        bool ISlimRnnLearningAlgorithm.checkForceTermination() {
            return false;
        }


        // table with the weights to choose from to set in the RNN
        // with the propability to calculate the timebound for the levin search of potential networks
        public IList<WeightWithPropability> weightWithPropabilityTable; // must be set

        // used to keep track of connections which got allready adapted by the learning algorithm for the given trail in the depth-first-search
        ISet<Tuple<uint, uint>> globalConnectionNeuronIndices = new HashSet<Tuple<uint, uint>>();


        IList<WeightChangeTreeElement> createWeightChangeTreeElementsForConnectionAndAddToParent(SlimRnnNeuronWithWeight connection, WeightChangeTreeElement parent) {
            IList<WeightChangeTreeElement> createdWeightChangeElements = new List<WeightChangeTreeElement>(weightWithPropabilityTable.Count);
            
            for (
                uint weightWithPropabilityTableIndex = 0;
                weightWithPropabilityTableIndex < weightWithPropabilityTable.Count;
                weightWithPropabilityTableIndex++
            ) {

                WeightChangeTreeElement createdWeightChangeTreeElement = WeightChangeTreeElement.make(connection, weightWithPropabilityTableIndex, parent);
                parent.children.Add(createdWeightChangeTreeElement);
                parent.childrenConnectionNeuronIndices.Add(new Tuple<uint, uint>(connection.source.neuronIndex, connection.target.neuronIndex));

                createdWeightChangeElements.Add(createdWeightChangeTreeElement);
            }

            return createdWeightChangeElements;
        }
        
        
        // calculates the timebound
        // see chapter   3.2 (Incremental Adaptive) Universal Search for SLIM NNs
        // algorithm "Univeral SLIM NN Search"
        double calcTimebound(uint levinSearchIteration, WeightChangeTreeElement treeElement) {
            Debug.Assert(treeElement != null);
            Debug.Assert(!treeElement.isRoot); // root node doesn't have any changed weights, so is invalid

            double propabilityProduct = 1.0;
            
            // walk down the tree and multiply the propabilities of the selected connections
            WeightChangeTreeElement currentTreeElement = treeElement;
            for (;;) {
                if (/*unnecessary   currentTreeElement == null ||*/ currentTreeElement.isRoot) {
                    break;
                }

                propabilityProduct *= weightWithPropabilityTable[(int)currentTreeElement.weightWithPropabilityTableIndex].propability;

                currentTreeElement = currentTreeElement.parent;
            }

            Debug.Assert(propabilityProduct <= 1.0 && propabilityProduct >= 0.0);

            return ((double)(calcTimeboundFactor(levinSearchIteration))) * propabilityProduct;
        }

        double calcTimeboundFactor(uint levinSearchIteration) {
            return 1ul << (int)levinSearchIteration;
        }

        // used to update the tree in ISlimRnnLearningAlgorithm.opportunityToAdjustWeight()
        // if it is null ISlimRnnLearningAlgorithm.opportunityToAdjustWeight() is doing nothing
        WeightChangeTreeElement currentWeightChangeTreeElement = null;



        SlimRnn slimRnn;
    }
    
    // used to test and verify solution
    public interface ITaskSolvedAndVerifiedTester {
        bool doesSlimRnnSolveTask(SlimRnn slimRnn, bool mustHalt, double tLim);
    }

    // used for "Universal SLIM NN Search" for backtracking in a depth first fashion of the program trace which is a prefix (tree)
    class DepthFirstSearchStackElement {
        //public readonly bool isRoot;

        public readonly WeightChangeTreeElement treeElement; // not complete trace

        private DepthFirstSearchStackElement(WeightChangeTreeElement treeElement/*, bool isRoot*/) {
            this.treeElement = treeElement;
            //this.isRoot = isRoot;
        }

        public static DepthFirstSearchStackElement make(WeightChangeTreeElement traceElement) {
            return new DepthFirstSearchStackElement(traceElement);//, false);
        }

        //public static DepthFirstSearchStackElement makeRoot() {
        //    return new DepthFirstSearchStackElement(null);//, true);
        //}
    }

    // used to traverse the possible SLIM RNN weight modifications
    class WeightChangeTreeElement {
        public readonly bool isRoot;

        public readonly SlimRnnNeuronWithWeight neuronWithWeight;
        public readonly uint weightWithPropabilityTableIndex; // index into UniversalSlimRnnSearch.weightWithPropabilityTable

        public IList<WeightChangeTreeElement> children = new List<WeightChangeTreeElement>();

        // all indices of the connections between neurons of the children (not recursive)
        public ISet<Tuple<uint, uint>> childrenConnectionNeuronIndices = new HashSet<Tuple<uint, uint>>();

        public WeightChangeTreeElement parent = null;

        private WeightChangeTreeElement(bool isRoot, SlimRnnNeuronWithWeight neuronWithWeight, uint weightWithPropabilityTableIndex, WeightChangeTreeElement parent) {
            this.isRoot = isRoot;
            this.neuronWithWeight = neuronWithWeight;
            this.weightWithPropabilityTableIndex = weightWithPropabilityTableIndex;
            this.parent = parent;
        }

        public static WeightChangeTreeElement makeRoot() {
            return new WeightChangeTreeElement(true, null, uint.MaxValue, null);
        }

        public static WeightChangeTreeElement make(SlimRnnNeuronWithWeight neuronWithWeight, uint weightWithPropabilityTableIndex, WeightChangeTreeElement parent = null) {
            return new WeightChangeTreeElement(false, neuronWithWeight, weightWithPropabilityTableIndex, parent);
        }
    }
}
