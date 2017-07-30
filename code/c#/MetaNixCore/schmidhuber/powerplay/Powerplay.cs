using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using MetaNix.schmidhuber.slimRnn;
using MetaNix.common;

namespace MetaNix.schmidhuber.powerplay {
    public class PowerplayMakeParameters {
        //public ITaskBuilder taskBuilder;
        public IPowerplayContext context;
    }

    public class Powerplay {
        public static Powerplay make(PowerplayMakeParameters parameters) {
            //Debug.Assert(parameters.taskBuilder != null);

            Powerplay made = new Powerplay(
                //parameters.taskBuilder,
                parameters.context);
            return made;
        }

        private Powerplay(/*ITaskBuilder taskBuilder, */IPowerplayContext context) {
            //this.taskBuilder = taskBuilder;
            this.context = context;
        }

        public void run() {
            initializeSolver();

            for( uint iteration = 1;; iteration++ ) {
                System.Console.WriteLine("iteration={0}", iteration);

                this.iteration();
            }
        }

        void iteration() {
            ITaskAndSolverModification p;
            ITask task;
            Solver solver;

            for(;;) {
                // basic steps of PowerPlay

                // let a search algorithm come up with p
                p = context.searchForCandidateProgram();

                // task invention
                // let p compute a task
                task = p.computeTask(solverTuples);

                // we bundle the solver modification and correctness demonstration to allow an efficient enumeration of the SLIM-RNN network weight for the task
                bool correctnessDemonstrationSuccess;
                solverModificationAndCorrectnessDemonstration(p, task, out solver, out correctnessDemonstrationSuccess);
                if( correctnessDemonstrationSuccess ) {
                    break;
                }
            }

            // add the modification of the task, solver and the task itself and the solver to our readonly list of tuples
            solverTuples.Add(new SolverTuple(task, solver));
            previousS = solver; // update the program
        }

        // /param correctnessDemonstrationSuccess is true if the task was correctly solved by the (returned) solver and all existing solvers can't solve the task
        void solverModificationAndCorrectnessDemonstration(ITaskAndSolverModification p, ITask task, out Solver solver, out bool correctnessDemonstrationSuccess) {
            var allExistingTasks = solverTuples.Select(v => v.task);
            context.solverModificationAndCorrectnessDemonstration(p, task, /* passing in this parameter is a hack */previousS, allExistingTasks, out solver, out correctnessDemonstrationSuccess);
        }

        void initializeSolver() {
            Debug.Assert(solverTuples.Count == 0); // must be the case because the first solver was never set

            previousS = context.returnInitialProgram();
        }

        IList<SolverTuple> solverTuples = new List<SolverTuple>();

        //readonly ITaskBuilder taskBuilder;
        readonly IPowerplayContext context;

        Solver previousS;
    }

    // solver as described in the paper
    public class Solver {
        public SlimRnn slimRnn;
    }

    public class SolverTuple {
        //public readonly ITaskAndSolverModification p;
        public readonly ITask task;
        public readonly Solver solver;

        public SolverTuple(/*ITaskAndSolverModification p, */ITask task, Solver solver) {
            //this.p = p;
            this.task = task;
            this.solver = solver;
        }
    }

    // called p element P in the paper
    public abstract class ITaskAndSolverModification {
        public abstract ITask computeTask(IList<SolverTuple> existingSolverTuples);
    }

    // used to search for a task and solver modification with some search algorithm
    // and it is also used for correctness demonstration
    public interface IPowerplayContext {
        // searches for an (in the paper called p) candidate program, which is a task and a solver modification
        ITaskAndSolverModification searchForCandidateProgram();

        // /param correctnessDemonstrationSuccess is true if the task was correctly solved by the (returned) solver and all existing solvers can't solve the task
        // /param previousS previous solver, we pass it in because for now we have a single "global" SLIM-RNN which is referenced by Solver
        void solverModificationAndCorrectnessDemonstration(ITaskAndSolverModification p, ITask task, Solver previousS, IEnumerable<ITask> allExistingTasks, out Solver solver, out bool correctnessDemonstrationSuccess);

        Solver returnInitialProgram();
    }




    /////////////
    // Environment and Context for testing if the Powerplay algorithm works as intended

    public class Environment2dTask : ITask {
        public Environment2dTask() {
            initializeRetina();
        }

        public void simulationIteration(float[] input, ref float[] onlineOutput, out EnumSimulationStepResult simulationStepResult) {
            render();

            float winThreshold = 0.3f;

            int moveDeltaX = 0,
                moveDeltaY = 0;

            // winner takes all input
            if( input[0] > winThreshold) {
                moveDeltaX++;
            }
            else if( input[1] > winThreshold) {
                moveDeltaX--;
            }
            else if (input[2] > winThreshold) {
                moveDeltaY++;
            }
            else if (input[3] > winThreshold) {
                moveDeltaY--;
            }

            // fast movement
            if (input[4] > winThreshold) {
                moveDeltaX *= 3;
                moveDeltaY *= 3;
            }
            else if (input[5] > winThreshold) {
                moveDeltaX *= 9;
                moveDeltaY *= 9;
            }
            else if (input[6] > winThreshold) {
                moveDeltaX *= 27;
                moveDeltaY *= 27;
            }
            else if (input[7] > winThreshold) {
                moveDeltaX *= 81;
                moveDeltaY *= 81;
            }

            if(moveDeltaX == 1) {
                int debugMeHere = 1;
            }

            retinaX += moveDeltaX;
            retinaY += moveDeltaY;

            float[] outputFromRetina = retina.sample(visionMap, retinaY, retinaX);
            onlineOutput = new float[outputFromRetina.Length + 1]; // add one to the size because we need a constant input to the NN
            for( int i = 0; i < outputFromRetina.Length; i++ ) {
                onlineOutput[i] = outputFromRetina[i];
            }
            onlineOutput[onlineOutput.Length - 1] = 1.0f; // constant


            moveObjects();

            simulationStepResult = EnumSimulationStepResult.NOTFINISHED;
        }


        public void resetState() {
            // we just set the retina to the center of the image

            retinaX = 30; // network has to learn to move to the left until it is visible
            retinaY = 64;
        }


        public uint lengthOfOnlineOutput {
            get {
                return (uint)retina.sensors.Count;
            }
        }

        public bool wasTaskSolved {
            get {
                // check if retina position is in the right position

                float diffX = retinaX - 64;
                float diffY = retinaY - 64;
                float distanceToCorrectPosition = (float)Math.Sqrt(diffX*diffX + diffY*diffY);

                return distanceToCorrectPosition < 5.0f;
            }
        }

        void render() {
            visionMap.clear();

            // render the objects

            // for now we just render the stationary test circle

            // TODO< render dynamic objects >

            Map2dFloatRenderer.renderCircleSlowNotAntialiased(visionMap, 64, 64, 4.0f);
        }

        void moveObjects() {
            // TODO< move objects >
        }

        void initializeRetina() {
            retina = new Retina();

            retina.sensors = new List<RetinaSensor>(5*5 + 8);

            // level 0 : center sensors
            for( int relativeY = -2; relativeY <= 2; relativeY++ ) {
                for (int relativeX = -2; relativeX <= 2; relativeX++) {
                    retina.sensors.Add(new RetinaSensor(relativeX, relativeY, 1));
                }
            }

            // level 1 : sensors around the center sensor
            for( int proximityY = -1; proximityY <= 1; proximityY++ ) {
                for( int proximityX = -1; proximityX <= 1; proximityX++ ) {
                    if( proximityX == 0 && proximityY == 0 ) { // the center is already filled with the retina-sensors
                        continue;
                    }

                    retina.sensors.Add(new RetinaSensor(proximityX * 5, proximityX * 5, 5));
                }
            }
        }


        public int retinaX, retinaY;

        Map2dFloat visionMap = new Map2dFloat(128, 128);
        Retina retina;

        Map2dFloatRenderer mapRenderer = new Map2dFloatRenderer();
        
    }

    public class Environment2dTaskAndSolverModification : ITaskAndSolverModification {
        public Environment2dTaskAndSolverModification(Environment2dPowerplayContext context) {
            this.context = context;
        }

        public override ITask computeTask(IList<SolverTuple> existingSolverTuples) {
            // TODO< come up with real task and increment levin search till we find one which is not jet solved >

            // TODO< do part of "correctness demonstration"(see Schmidhuber's paper for details) by showing that the current solver can't solve the new task >

            // for now we just return a new task
            return new Environment2dTask();
        }

        readonly Environment2dPowerplayContext context;
    }

    public class Environment2dPowerplayContext : IPowerplayContext {
        public Solver returnInitialProgram() {
            Solver solver = new Solver();
            solver.slimRnn = new SlimRnn();

            SlimRnn rnn = solver.slimRnn;

            // neurons for the retina input
            for( int i = 0; i < 5*5; i++ ) {
                rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron());
            }

            // neurons for periphal vision of the retina
            for( int i = 0; i < 3*3 - 1; i++ ) {
                rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron());
            }

            neuronIndexConstantOne = (uint)rnn.neurons.Count;
            rnn.neurons.Add(SlimRnnNeuron.makeInputNeuron());


            uint neuronTerminationIndex = (uint)rnn.neurons.Count;

            // output neuron for termination
            rnn.neurons.Add(new SlimRnnNeuron(SlimRnnNeuron.EnumType.ADDITIVE));

            uint neuronOutputStartIndex = (uint)rnn.neurons.Count;
            uint numberOfOutputNeurons = 8;

            // output neurons for controlling the retina
            for ( int i = 0; i < numberOfOutputNeurons; i++ ) {
                rnn.neurons.Add(new SlimRnnNeuron(SlimRnnNeuron.EnumType.ADDITIVE));
            }

            rnn.outputNeuronsStartIndex = neuronOutputStartIndex;
            rnn.numberOfOutputNeurons = numberOfOutputNeurons;

            rnn.terminatingNeuronIndex = neuronTerminationIndex;
            rnn.terminatingNeuronThreshold = 0.5f;

            rnn.numberOfInputNeurons = (5*5) + (9-1)   + 1 /* constant neuron */;

            rnn.t_lim = double.MaxValue; // is set by the learning algorithm


            // add and initialize "hidden" neurons

            neuronIndexOfHiddenUnits = (uint)rnn.neurons.Count;

            uint numberOfHiddenNeuronsWtaGroups = 50;
            uint numberOfNeuronsInWtaGroup = 4; // 4 is a good number as chosen by Schmidhuber

            for( uint groupI = 0; groupI < numberOfHiddenNeuronsWtaGroups; groupI++ ) {
                for( int neuronI = 0; neuronI < numberOfNeuronsInWtaGroup; neuronI++ ) {
                    bool isNeuronIEvent = (neuronI % 2) == 0;
                    SlimRnnNeuron.EnumType neuronType = isNeuronIEvent ? SlimRnnNeuron.EnumType.ADDITIVE : SlimRnnNeuron.EnumType.MULTIPLICATIVE;

                    SlimRnnNeuron neuron = new SlimRnnNeuron(neuronType);
                    neuron.winnerTakesAllGroup = groupI;
                    
                    rnn.neurons.Add(neuron);
                }
            }

            rnn.initializeNeurons();

            // set initial network


            { // wire the central input sensor to the termination
                int retinaX = 0;
                int retinaY = 0;

                int absoluteRetinaIndexX = 2 + retinaX;
                int absoluteRetinaIndexY = 2 + retinaY;

                int retinaInputNeuronIndex = 0 + 5 * absoluteRetinaIndexY + absoluteRetinaIndexX;

                rnn.neurons[retinaInputNeuronIndex].outNeuronsWithWeights.Add(new SlimRnnNeuronWithWeight(rnn.neurons[retinaInputNeuronIndex], rnn.neurons[(int)neuronTerminationIndex], 0.55f, 0));

            }
            return solver;
        }

        public ITaskAndSolverModification searchForCandidateProgram() {
            // TODO< do part of the "correctness demonstration", show that the new task can't be solved by the current solver >

            return new Environment2dTaskAndSolverModification(this);
        }

        public void solverModificationAndCorrectnessDemonstration(ITaskAndSolverModification pParameter, ITask taskParameter, Solver previousS, IEnumerable<ITask> allExistingTasks, out Solver solver, out bool correctnessDemonstrationSuccess) {
            var p = (Environment2dTaskAndSolverModification)pParameter;
            var task = (Environment2dTask)taskParameter;

            correctnessDemonstrationSuccess = false;
            solver = null;

            UniversalSlimRnnSearch slimRnnSearch = new UniversalSlimRnnSearch(previousS.slimRnn, new Environment2dPowerplayNetworkTester(task, allExistingTasks));

            slimRnnSearch.weightWithPropabilityTable = returnNormalizedPropabilitiesOfTable();

            { // solver modification

                // hard coded modification
                // make a connection between the 1.0 input neuron and the neuron which causes a move to the right
                previousS.slimRnn.neurons[(int)neuronIndexConstantOne].outNeuronsWithWeights.Add(
                    new SlimRnnNeuronWithWeight(previousS.slimRnn.neurons[(int)neuronIndexConstantOne], previousS.slimRnn.neurons[(int)previousS.slimRnn.outputNeuronsStartIndex], 0.0f, 0, true));


                // TODO< not hardcoded modification >
            }


            // let the SLIM-RNN search-algorithm search for a network which solves the task the fastest way
            uint maximumIteration = 3;
            bool mustHalt = true;
            bool wasSolved;
            SlimRnn solutionRnn;

            // we have to set the world because UniversalSlimRnnSearch.search() uses the world
            ///slimRnnSearch.world = new TaskSlimRnnWorld(task);

            // the search is doing "solver modification" _and_ "correctness demonstration"
            slimRnnSearch.search(maximumIteration, mustHalt, out wasSolved, out solutionRnn);

            // rollback changes
            // if it was solved then keep used eligable weights
            if( wasSolved ) {
                int breakpointHere = 1;

                // we have to rollback/remove eligable weights which didn't get used
                foreach (SlimRnnNeuron iNeuron in previousS.slimRnn.neurons) {
                    iNeuron.outNeuronsWithWeights = new List<SlimRnnNeuronWithWeight>(
                        iNeuron.outNeuronsWithWeights.Where(v => !v.isEligable || (v.isEligable && (v.weight != 0.0f)))
                    );
                }

                // set all used eligable connections to normal connections
                foreach (SlimRnnNeuron iNeuron in previousS.slimRnn.neurons) {
                    foreach( var iConnection in iNeuron.outNeuronsWithWeights ) {
                        Debug.Assert(!iConnection.isEligable || (iConnection.isEligable && iConnection.weight != 0.0f));
                        iConnection.isEligable = false;
                    }

                }
            }
            else {
                // we have to rollback/remove all eligable weights
                foreach ( SlimRnnNeuron iNeuron in previousS.slimRnn.neurons ) {
                    iNeuron.outNeuronsWithWeights = new List<SlimRnnNeuronWithWeight>(iNeuron.outNeuronsWithWeights.Where(v => !v.isEligable));
                }
            }

            // if we solved the task with the SLIM-RNN then we have to create a solver which represents our new problem solver
            // Solver has of course the SLIM-RNN which solved the problem
            if( wasSolved ) {
                solver = new Solver();
                solver.slimRnn = solutionRnn;
            }

            if( !wasSolved ) {
                // if it was not solved we can't demonstrate the correctness

                correctnessDemonstrationSuccess = false;
                return;
            }
            
            correctnessDemonstrationSuccess = true;
        }

        // returns the candidates of the weights/strength for the connections between neurons
        static IList<UniversalSlimRnnSearch.WeightWithPropability> returnNormalizedPropabilitiesOfTable() {
            IList<UniversalSlimRnnSearch.WeightWithPropability> table = new List<UniversalSlimRnnSearch.WeightWithPropability>();

            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-10.0f * 5, 0.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(10.0f * 5, 0.5));

            // add 10's of steps
            for ( int i = 1; i < 5; i++ ) {
                table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-10.0f * i, 1.0));
                table.Add(new UniversalSlimRnnSearch.WeightWithPropability(10.0f * i, 1.0));
            }

            // add 1's of steps
            for (int i = 1; i <= 9; i++) {
                table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-1.0f * i, 1.0));
                table.Add(new UniversalSlimRnnSearch.WeightWithPropability(1.0f * i, 1.0));
            }

            // add fine grained steps
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-0.99f, 1.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-0.98f, 1.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-0.95f, 1.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-0.92f, 1.5));

            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(0.99f, 1.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(0.98f, 1.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(0.95f, 1.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(0.92f, 1.5));

            // add not so fine steps between 0.95 and 0.5 in 0.5 steps
            for( float i = 0.95f; i > 0.5f - 0.02f/*epsilon*/; i -= 0.5f ) {
                table.Add(new UniversalSlimRnnSearch.WeightWithPropability(i, 1.5));
                table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-i, 1.5));
            }

            // add superfine steps
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(0.05f, 0.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-0.05f, 0.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(0.02f, 0.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-0.02f, 0.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(0.01f, 0.5));
            table.Add(new UniversalSlimRnnSearch.WeightWithPropability(-0.01f, 0.5));

            SlimRnnPropabilityNormalizer.normalize(table);
            return table;
        }
        
        // for the enumeration of possible tasks we use simple levin search which calculates a problem description after which we can build the Task
        internal LevinSearchContext taskCounter = new LevinSearchContext(2);

        uint neuronIndexOfHiddenUnits;

        uint numberOfHiddenNeuronsWtaGroups;
        uint numberOfNeuronsInWtaGroup;

        uint neuronIndexConstantOne;
    }

    // used to test the current SlimRnn against the task for Powerplay
    class Environment2dPowerplayNetworkTester : ITaskSolvedAndVerifiedTester {
        public Environment2dPowerplayNetworkTester(Environment2dTask currentTask, IEnumerable<ITask> allExistingTasks) {
            this.currentTask = currentTask;
            this.allExistingTasks = allExistingTasks;
        }

        public bool doesSlimRnnSolveTask(SlimRnn slimRnn, bool mustHalt, double tLim) {
            // NOTE< we ignore must halt because we hardcoded to check for termination >

            instrumentationNewSolveTrial();

            // simulate slimRnn in environment and correctness demonstration

            //    we don't have to show that the old solver doesn't solve the task because we did the step already in searchForCandidateProgram()
            //    we just have to show that the new solver can solve the new task and all old tasks

            IList<SlimRnnNeuronWithWeight> traceResult;
            double requiredTime;
            bool wasTerminated;

            { // test new task
                slimRnn.world = new TaskSlimRnnWorld(currentTask);

                currentTask.resetState();

                slimRnn.t_lim = tLim;
                slimRnn.spread(out traceResult, out requiredTime, out wasTerminated);

                bool thisTaskSolvedAndTerminated = wasTerminated && currentTask.wasTaskSolved;
                if (!thisTaskSolvedAndTerminated) {
                    return false;
                }
            }

            // test all old tasks
            foreach( ITask iOldTask in allExistingTasks ) {
                slimRnn.world = new TaskSlimRnnWorld(iOldTask);

                iOldTask.resetState();

                slimRnn.t_lim = tLim;
                slimRnn.spread(out traceResult, out requiredTime, out wasTerminated);

                bool thisTaskSolvedAndTerminated = wasTerminated && iOldTask.wasTaskSolved;
                if ( !thisTaskSolvedAndTerminated ) {
                    return false;
                }
            }

            return true;
        }
        
        void instrumentationNewSolveTrial() {
            Console.WriteLine("RNN trail");

            // TODO< increment counter >
        }

        Environment2dTask currentTask;
        IEnumerable<ITask> allExistingTasks; // used for correctness demonstration
    }

    // interacts with the task as a world for the SLIM-RNN search algorithm
    class TaskSlimRnnWorld : ISlimRnnWorld {
        public TaskSlimRnnWorld(ITask task) {
            this.task = task;

            // init for first invocation of retriveInputVector()
            nextStepInputForRnn = new float[task.lengthOfOnlineOutput+1];
        }

        ITask task;

        public float[] retriveInputVector() {
            return nextStepInputForRnn;
        }

        public void executeEnvironmentChangingActions(bool[] outputNeuronActivations, ref double time) {
            uint baseNumberOfSteps = 60; // how many steps would a standard test consume

            // calculate how much time we advance, we assume that the search algorithm chooses two connections with avaerage propability
            // this is why we multiple 
            double averageConnectionPropability = 0.022;
            double advancedTime = averageConnectionPropability * averageConnectionPropability * (1.0 / (double)baseNumberOfSteps);


            time += advancedTime; // advance time to prevent a task from looping for eternity

            EnumSimulationStepResult simulationStepResult; // ignored
            task.simulationIteration(boolToFloatArr(outputNeuronActivations), ref nextStepInputForRnn, out simulationStepResult);
        }

        // TODO< move to misc >
        static float[] boolToFloatArr(bool[] arr) {
            float[] resultArr = new float[arr.Length];
            for( int i = 0; i < arr.Length; i++ ) {
                resultArr[i] = arr[i] ? 1.0f : 0.0f;
            }
            return resultArr;
        }

        float[] nextStepInputForRnn;
    }

    // levin search helper
    public class LevinSearchContext {
        public LevinSearchContext(uint numberOfSymbols) {
            this.numberOfSymbols = numberOfSymbols;
        }

        public void increment() {
            // slow increment algorithm

            int i = 0;
            bool carry;
            for( carry = true; carry ; i++) {
                carry = ++program[i] == numberOfSymbols;
                if( carry ) {
                    // reset
                    program[i] = 0;
                }
            }

            // if we stil carry then we need to extend the program
            program.Add(0);
        }

        public IList<uint> program = new List<uint>{0};

        readonly uint numberOfSymbols;
    }


    // the environment, which consists out of a screen with a retina and objects which move around

    public class Retina {
        public float[] sample(Map2dFloat map, int y, int x) {
            float[] arr = new float[sensors.Count];
            for( int i = 0; i < sensors.Count; i++ ) {
                arr[i] = sensors[i].sample(map, y, x);
            }
            return arr;
        }

        public IList<RetinaSensor> sensors;
    }

    // calculate the average of the image over the rensed area
    public class RetinaSensor {
        public RetinaSensor(int relativeOffsetOfTopLeftCornerX, int relativeOffsetOfTopLeftCornerY, uint size) {
            this.relativeOffsetOfTopLeftCornerX = relativeOffsetOfTopLeftCornerX;
            this.relativeOffsetOfTopLeftCornerY = relativeOffsetOfTopLeftCornerY;
            this.size = (int)size;
        }

        public float sample(Map2dFloat map, int offsetY, int offsetX) {
            float sum = 0.0f;

            for( int relativeY = 0; relativeY < size; relativeY++ ) {
                for (int relativeX = 0; relativeX < size; relativeX++) {
                    int x = offsetX + relativeOffsetOfTopLeftCornerX + relativeX;
                    int y = offsetY + relativeOffsetOfTopLeftCornerY + relativeY;

                    if( map.isInRange(y, x) ) {
                        sum += map[y, x];
                    }
                }
            }

            sum = sum / (float)(size * size);
            return sum;
        }

        public int relativeOffsetOfTopLeftCornerX, relativeOffsetOfTopLeftCornerY;

        public readonly int size;
    }

    public class Map2dFloatRenderer {
        public static void renderCircleSlowNotAntialiased(Map2dFloat map, int x, int y, float radius) {
            for( int relativeY = (int)-radius; relativeY < (int)radius; relativeY++ ) {
                for (int relativeX = (int)-radius; relativeX < (int)radius; relativeX++) {
                    float distance = (float)Math.Sqrt(relativeX * relativeX + relativeY * relativeY);

                    // anti aliasing
                    float value = distance < radius ? 1.0f : 0.0f; // no antialiasing

                    if( value == 0.0f ) {
                        continue;
                    }

                    if( !map.isInRange(y + relativeY, x + relativeX) ) {
                        continue;
                    }

                    map[y + relativeY, x + relativeX] = value;
                }
            }
        }
    }
    
}
