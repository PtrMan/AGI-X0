﻿using System;
using System.Collections.Generic;

using MetaNix;
using MetaNix.datastructures;
using MetaNix.instrumentation;

using MetaNix.resourceManagement.compute;
using MetaNix.report;
using MetaNix.attention;
using MetaNix.search.levin2;
using MetaNix.scheduler;
using MetaNix.nars;

namespace MetaNix {
    class CellularAutomata {
        public static int calc(int rule, uint bits, int value) {
            int result = 0;

            { // optimization for first bit
                int read2 = (value >> ((int)bits-1)) & 1;
                int read1 = (value >> 0) & 1;
                int read0 = (value >> 1) & 1;

                result = applyCaRule(rule, read2, read1, read0);
            }

            { // optimization for last bit
                int read2 = (value >> 0) & 1;
                int read1 = (value >> ((int)bits)) & 1;
                int read0 = (value >> ((int)bits - 1)) & 1;

                result |= (applyCaRule(rule, read2, read1, read0) << (int)bits);
            }

            for(int i = 1; i < bits-1; i++) {
                int read2 = (value >> (i + 1)) & 1;
                int read1 = (value >> (i   )) & 1;
                int read0 = (value >> (i - 1)) & 1;

                int caResult = applyCaRule(rule, read2, read1, read0);

                result |= (caResult << i);
            }

            return result;
        }

        static int applyCaRule(int rule, int value2, int value1, int value0) {
            int value = value2 * 4 + value1 * 2 + value0 * 1;
            return (rule >> value) & 1;
        }
    }

    // reservoir computing with cellular automata
    class CaReservoir {
        public int rule;
        public uint bits;

        public class Trace {
            public int[] numbers;
        }

        public Trace[] traces;

        public void calcTraces() {
            for(int i = 0; i < traces.Length; i++) {
                calcTrace(traces[i]);
            }
        }

        void calcTrace(Trace trace) {
            for(int i = 0; i < trace.numbers.Length-1; i++) {
                // calculate next ca state
                trace.numbers[i + 1] = CellularAutomata.calc(rule, bits, trace.numbers[i]);
            }
        }
    }


    class Program {
        static void caReservoirTest() {
            CaReservoir caReservoir = new CaReservoir();
            caReservoir.rule = 110;
            caReservoir.bits = 5;

            caReservoir.traces = new CaReservoir.Trace[1];
            caReservoir.traces[0] = new CaReservoir.Trace();
            caReservoir.traces[0].numbers = new int[3];

            caReservoir.calcTraces();

        }

        /* uncommented because we have to overhaul this and unify it with the other functional representation
         * and it must be an unittest
        static void interpreterTest0() {
            PrimitiveInstructionInterpreter interpreter = new PrimitiveInstructionInterpreter();

            PrimitiveInterpretationContext interpreterContext = new PrimitiveInterpretationContext();
            interpreterContext.registers = new Register[3];
            interpreterContext.registers[0] = new Register();
            interpreterContext.registers[1] = new Register();
            interpreterContext.registers[2] = new Register();

            ImmutableNodeReferer rootnode0 = ImmutableNodeReferer.makeBranch();
            rootnode0.children = new ImmutableNodeReferer[4];

            rootnode0.children[0] = ImmutableNodeReferer.makeBranch();
            rootnode0.children[0].children = new ImmutableNodeReferer[4];
            rootnode0.children[0].children[0] = Node.makeInstr(Node.EnumType.INSTR_ADD_INT);
            rootnode0.children[0].children[1] = Node.makeAtomic(Variant.makeInt(0));
            rootnode0.children[0].children[2] = Node.makeAtomic(Variant.makeInt(1));
            rootnode0.children[0].children[3] = Node.makeAtomic(Variant.makeInt(2));


            rootnode0.children[1] = ImmutableNodeReferer.makeBranch();
            rootnode0.children[1].children = new ImmutableNodeReferer[2];
            rootnode0.children[1].children[0] = Node.makeInstr(Node.EnumType.INSTR_GOTO);
            rootnode0.children[1].children[1] = Node.makeAtomic(Variant.makeInt(0));




            interpreter.interpret(interpreterContext, rootnode0.children[0], new List<ImmutableNodeReferer>(), new List<string>());
        }
        **/


        static void parsedTest0() {
            Functional.ParseTreeElement parseTree = Functional.parseRecursive("(bAnd (shl value i) 1)");
            NodeRefererEntry node = FunctionalToParseTreeTranslator.translateRecursive(parseTree);

            int debugHere = 1;
        }

        
        static void parseInterpretSurrogateTest0() {
            FunctionalInterpreter functionalInterpreter = new FunctionalInterpreter();
            functionalInterpreter.tracer = new NullFunctionalInterpreterTracer();
            FunctionalInterpretationContext functionalInterpretationContext = new FunctionalInterpretationContext();


            dispatch.FunctionalInterpreterSurrogate interpreterSurrogate = new dispatch.FunctionalInterpreterSurrogate(functionalInterpreter, functionalInterpretationContext);

            // dispatcher which dispatches hidden function calls to the interpreter
            dispatch.SurrogateProvider surrogateProvider = new dispatch.SurrogateProvider();

            // dispatcher which can shadow calls (to the surrogate provider)
            dispatch.ShadowableHiddenDispatcher shadowableHiddenDispatcher = new dispatch.ShadowableHiddenDispatcher(surrogateProvider);
            

            // dispatcher which calls another dispatcher and a number of observers,
            // which is in this case our instrumentation observer
            dispatch.InstrumentationHiddenDispatcher instrHiddenDispatcher = new dispatch.InstrumentationHiddenDispatcher(shadowableHiddenDispatcher);
            dispatch.TimingAndCountHiddenDispatchObserver instrObserver = new dispatch.TimingAndCountHiddenDispatchObserver();
            instrObserver.resetCountersAndSetEnableTimingInstrumentation(true);
            instrHiddenDispatcher.dispatchObservers.Add(instrObserver);

            dispatch.ArgumentBasedDispatcher publicDispatcherByArguments = new dispatch.ArgumentBasedDispatcher(instrHiddenDispatcher);
            // dispatcher which accepts function names
            dispatch.PublicCallDispatcher callDispatcher = new dispatch.PublicCallDispatcher(publicDispatcherByArguments);
            
            Functional.ParseTreeElement parseTree = Functional.parseRecursive("(let [value 4 i 1   read2 (bAnd (shl value (+ i 1)) 1)] read2)");
            NodeRefererEntry rootNode = FunctionalToParseTreeTranslator.translateRecursive(parseTree);

            { // set descriptor to route all public function id's 0 to hidden function id 0
                dispatch.ArgumentBasedDispatcher.FunctionDescriptor fnDescriptor = new dispatch.ArgumentBasedDispatcher.FunctionDescriptor();
                fnDescriptor.wildcardHiddenFunctionId = dispatch.HiddenFunctionId.make(0);
                publicDispatcherByArguments.setFunctionDescriptor(dispatch.PublicFunctionId.make(0), fnDescriptor);
            }

            surrogateProvider.updateSurrogateByFunctionId(dispatch.HiddenFunctionId.make(0), interpreterSurrogate);
            interpreterSurrogate.updateFunctionBody(dispatch.HiddenFunctionId.make(0), rootNode.entry);
            interpreterSurrogate.updateParameterNames(dispatch.HiddenFunctionId.make(0), new List<string>());

            callDispatcher.setFunctionId("test", dispatch.PublicFunctionId.make(0));

            for(int i=0;i<100;i++) {
                callDispatcher.dispatchCallByFunctionName("test", new List<Variant>());
            }

            System.Console.WriteLine(instrObserver.getInstrumentation(dispatch.HiddenFunctionId.make(0)).calltimeMaxInNs);
            System.Console.WriteLine(instrObserver.getInstrumentation(dispatch.HiddenFunctionId.make(0)).calltimeMinInNs);
            System.Console.WriteLine(instrObserver.getInstrumentation(dispatch.HiddenFunctionId.make(0)).calltimeSumInNs);

            //Statistics statistics = new Statistics(instrObserver);
            //statistics.doIt();

            int debugMe = 0;
        }


        static void agentTest0() {
            ImmutableNodeReferer orginalArray = ImmutableNodeRefererManipulatorHelper.makeImmutableNodeRefererForArray(new List<Variant> {Variant.makeInt(5),Variant.makeInt(2),Variant.makeInt(7) });

            StochasticBag stochasticBag = new StochasticBag();
            stochasticBag.propabilities = new List<float> { 0.2f, 0.6f, 0.2f };

            Random random = new Random();

            for(int i = 0; i < 15; i++) {
                Console.WriteLine("choice={0}", stochasticBag.getValueByUniformRandomVariable((float)random.NextDouble()));

                // TODO< manipulate array and print to console >
            }

            int deb = 0;
        }


        static void Main(string[] args) {
            ReasonerInstanceConfiguration configuration = new ReasonerInstanceConfiguration();
            configuration.k = 1.0f; // TODO< select right value >
            configuration.maximalTermComplexity = 20; // TODO< select right value >

            // test nars
            ReasonerInstance reasoner = new ReasonerInstance(configuration);



            // test Levin search

            //Program2.interactiveTestEnumeration();

            Scheduler scheduler = new Scheduler();
            
            SparseArrayProgramDistribution sparseArrayProgramDistribution = new SparseArrayProgramDistribution();
            AdvancedAdaptiveLevinSearchTaskProvider levinSearchTaskProvider = new AdvancedAdaptiveLevinSearchTaskProvider(scheduler, sparseArrayProgramDistribution);

            // linked list problem
            if (false) { // append one element in reg1 to linked list, precondition is that the array index is already past the end of the array
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 6;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16;/*because no call*/

                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/6 + 1/* might be off by one*/;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());

                // append
                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { null, 6 };
                levinSearchProblem.trainingSamples[0].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[0].answerRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[0].answerArray = new List<int> { 3, 1, 6 };

                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 3, 1, 6 };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { null, 5 };
                levinSearchProblem.trainingSamples[1].questionArrayIndex = 3;
                levinSearchProblem.trainingSamples[1].answerRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 3, 1, 6, 3, 1, 5 };

                levinSearchProblem.trainingSamples[2].questionArray = new List<int> { 3, 1, 6, 9 };
                levinSearchProblem.trainingSamples[2].questionRegisters = new int?[] { null, 7 };
                levinSearchProblem.trainingSamples[2].questionArrayIndex = 4;
                levinSearchProblem.trainingSamples[2].answerRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[2].answerArray = new List<int> { 3, 1, 6, 9, 3, 1, 7 };

                levinSearchProblem.trainingSamples[3].questionArray = new List<int> { 4, 1, 6, 9, 3, 1, 7 };
                levinSearchProblem.trainingSamples[3].questionRegisters = new int?[] { null, 10 };
                levinSearchProblem.trainingSamples[3].questionArrayIndex = 7;
                levinSearchProblem.trainingSamples[3].answerRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[3].answerArray = new List<int> { 4, 1, 6, 9, 3, 1, 7, 3, 1, 10 };


                levinSearchTaskProvider.problems.Add(levinSearchProblem);


            }

            // linked list problem
            if (true) { // disable/free element in linked list, precondition is that the array index is already pointing at the element
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 5;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16;/*because no call*/

                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/6 + 1/* might be off by one*/;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());

                // append
                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { 3, 1, 6 };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { 42, 11 };
                levinSearchProblem.trainingSamples[0].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[0].answerRegisters = new int?[] { null, 11 };
                levinSearchProblem.trainingSamples[0].answerArray = new List<int> { 3, 0, 6 };
                levinSearchProblem.trainingSamples[0].answerArrayIndex = 0;
                
                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 3, 1, 6, 3, 1, 5 };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { 42, 11 };
                levinSearchProblem.trainingSamples[1].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[1].answerRegisters = new int?[] { null, 11 };
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 3, 0, 6, 3, 1, 5 };
                levinSearchProblem.trainingSamples[1].answerArrayIndex = 0;

                levinSearchProblem.trainingSamples[2].questionArray = new List<int> { 3, 1, 6, 3, 1, 5 };
                levinSearchProblem.trainingSamples[2].questionRegisters = new int?[] { 42, 11 };
                levinSearchProblem.trainingSamples[2].questionArrayIndex = 3;
                levinSearchProblem.trainingSamples[2].answerRegisters = new int?[] { null, 11 };
                levinSearchProblem.trainingSamples[2].answerArray = new List<int> { 3, 1, 6, 3, 0, 5 };
                levinSearchProblem.trainingSamples[2].answerArrayIndex = 3;

                
                levinSearchProblem.trainingSamples[3].questionArray = new List<int> { 4, 1, 6, 9, 3, 1, 7, 3, 1, 10 };
                levinSearchProblem.trainingSamples[3].questionRegisters = new int?[] { 42, 11 };
                levinSearchProblem.trainingSamples[3].questionArrayIndex = 7;
                levinSearchProblem.trainingSamples[3].answerRegisters = new int?[] { null, 11 };
                levinSearchProblem.trainingSamples[3].answerArray = new List<int> { 4, 1, 6, 9, 3, 1, 7, 3, 0, 10 };
                levinSearchProblem.trainingSamples[3].answerArrayIndex = 7;

                levinSearchProblem.trainingSamples[4].questionArray = new List<int> { 4, 1, 6, 9, 3, 1, 7, 3, 1, 10 };
                levinSearchProblem.trainingSamples[4].questionRegisters = new int?[] { 42, 11 };
                levinSearchProblem.trainingSamples[4].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[4].answerRegisters = new int?[] { null, 11 };
                levinSearchProblem.trainingSamples[4].answerArray = new List<int> { 4, 0, 6, 9, 3, 1, 7, 3, 1, 10 };
                levinSearchProblem.trainingSamples[4].answerArrayIndex = 0;

                levinSearchProblem.trainingSamples[5].questionArray = new List<int> { 4, 1, 6, 9, 3, 1, 7, 3, 1, 10 };
                levinSearchProblem.trainingSamples[5].questionRegisters = new int?[] { 42, 11 };
                levinSearchProblem.trainingSamples[5].questionArrayIndex = 4;
                levinSearchProblem.trainingSamples[5].answerRegisters = new int?[] { null, 11 };
                levinSearchProblem.trainingSamples[5].answerArray = new List<int> { 4, 1, 6, 9, 3, 0, 7, 3, 1, 10 };
                levinSearchProblem.trainingSamples[5].answerArrayIndex = 4;


                levinSearchTaskProvider.problems.Add(levinSearchProblem);


            }

            if (false) { // add one to reg0 if reg1 appear in array at current position, advance
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 6;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16;/*because no call*/

                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/6 + 1/* might be off by one*/;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());

                // at end of array it should return the answer flag false because it is at the end
                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[0].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[0].answerFlag = false;

                // count the counter up if it appeared
                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 1 };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { 1, 1 };
                levinSearchProblem.trainingSamples[1].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[1].answerRegisters = new int?[] { 2, null };
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 1 };
                levinSearchProblem.trainingSamples[1].answerArrayIndex = 1;
                levinSearchProblem.trainingSamples[1].answerFlag = false;

                // we can't count the counter up if it doesn't appear
                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 1 };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { 1, 3 };
                levinSearchProblem.trainingSamples[1].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[1].answerRegisters = new int?[] { 1, null };
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 1 };
                levinSearchProblem.trainingSamples[1].answerArrayIndex = 1;
                levinSearchProblem.trainingSamples[1].answerFlag = false;

                // flag should be false if it couldn't advance
                levinSearchProblem.trainingSamples[2].questionArray = new List<int> { 1 };
                levinSearchProblem.trainingSamples[2].questionRegisters = new int?[] { 2, 1 };
                levinSearchProblem.trainingSamples[2].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[2].answerRegisters = new int?[] { 3, null };
                levinSearchProblem.trainingSamples[2].answerArray = new List<int> { 1 };
                levinSearchProblem.trainingSamples[2].answerArrayIndex = 1;
                levinSearchProblem.trainingSamples[2].answerFlag = false;

                // flag should be true if it could advance
                levinSearchProblem.trainingSamples[3].questionArray = new List<int> { 1, 3 };
                levinSearchProblem.trainingSamples[3].questionRegisters = new int?[] { 2, 1 };
                levinSearchProblem.trainingSamples[3].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[3].answerRegisters = new int?[] { 3, null };
                levinSearchProblem.trainingSamples[3].answerArray = new List<int> { 1, 3 };
                levinSearchProblem.trainingSamples[3].answerArrayIndex = 1;
                levinSearchProblem.trainingSamples[3].answerFlag = true;

                levinSearchTaskProvider.problems.Add(levinSearchProblem);

                /* should return
                    arrayValid arr0
                    jumpIfNotFlag a
                    arrayCompare reg1
                    addFlag reg0, 1
                    arrayIdxFlag arr0 +1
                a:  (ret)
                */
            }

            // append reg1 to array[0]
            if(false) {
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 6;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16;/*because no call*/

                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/6 + 1/* might be off by one*/;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());

                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { null, 0 };
                levinSearchProblem.trainingSamples[0].answerArray = new List<int> { 0 };
                levinSearchProblem.trainingSamples[0].answerRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[0].answerArrayIndex = 0;

                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 4};
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { null, 1 };
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 4, 1 };
                levinSearchProblem.trainingSamples[1].answerRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[1].answerArrayIndex = 1;

                levinSearchProblem.trainingSamples[2].questionArray = new List<int> { 9, 3};
                levinSearchProblem.trainingSamples[2].questionRegisters = new int?[] { null, 2 };
                levinSearchProblem.trainingSamples[2].answerArray = new List<int> { 9, 3, 2 };
                levinSearchProblem.trainingSamples[2].answerRegisters = new int?[] { null, null };
                levinSearchProblem.trainingSamples[2].answerArrayIndex = 2;

                levinSearchTaskProvider.problems.Add(levinSearchProblem);
            }

            // disabled because too complicated without bias
            // we need to use another program here
            if(false) { // append reg0 times reg1
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 6;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16;/*because no call*/

                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/6 + 1/* might be off by one*/;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());

                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { 0, 5 };
                levinSearchProblem.trainingSamples[0].answerArray = new List<int> { };
                levinSearchProblem.trainingSamples[0].answerRegisters = new int?[] { null, null };

                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { 1, 5 };
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 5 };
                levinSearchProblem.trainingSamples[1].answerRegisters = new int?[] { null, null };

                levinSearchProblem.trainingSamples[2].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[2].questionRegisters = new int?[] { 2, 5 };
                levinSearchProblem.trainingSamples[2].answerArray = new List<int> { 5, 5 };
                levinSearchProblem.trainingSamples[2].answerRegisters = new int?[] { null, null };

                levinSearchProblem.trainingSamples[3].questionArray = new List<int> { 1 };
                levinSearchProblem.trainingSamples[3].questionRegisters = new int?[] { 2, 5 };
                levinSearchProblem.trainingSamples[3].answerArray = new List<int> { 1, 5, 5 };
                levinSearchProblem.trainingSamples[3].answerRegisters = new int?[] { null, null };

                levinSearchProblem.trainingSamples[4].questionArray = new List<int> { 3 };
                levinSearchProblem.trainingSamples[4].questionRegisters = new int?[] { 2, 9 };
                levinSearchProblem.trainingSamples[4].answerArray = new List<int> { 3, 9, 9 };
                levinSearchProblem.trainingSamples[4].answerRegisters = new int?[] { null, null };

                levinSearchProblem.trainingSamples[5].questionArray = new List<int> { 3 };
                levinSearchProblem.trainingSamples[5].questionRegisters = new int?[] { 3, 9 };
                levinSearchProblem.trainingSamples[5].answerArray = new List<int> { 3, 9, 9, 9 };
                levinSearchProblem.trainingSamples[5].answerRegisters = new int?[] { null, null };

                levinSearchTaskProvider.problems.Add(levinSearchProblem);

                /*
                **(too complicated without bias)**

                    ```
                    arraySetIdx - 1
                    cmp reg0 0
                    jumpNotFlag 1
                    ret                 5
                    arrayInsert reg1    6
                    sub reg0 1
                    jmp - 7
                    ```
                */
            }

            if(false) { // problem : append reg2 to end and reg1 to beginning
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 5;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16/*because no call*/;
                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/5 */*number of elements in array*/5;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { null, 7, 3 }; // search for 7
                levinSearchProblem.trainingSamples[0].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[0].answerArray = new List<int> { 7, 3 };
                levinSearchProblem.trainingSamples[0].answerArrayIndex = 1; // result index

                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 1, 2, };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { null, 7, 3 }; // search for 7
                levinSearchProblem.trainingSamples[1].questionArrayIndex = 1;
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 7, 1, 2, 3 };
                levinSearchProblem.trainingSamples[1].answerArrayIndex = 3; // result index

                levinSearchProblem.trainingSamples[2].questionArray = new List<int> { };
                levinSearchProblem.trainingSamples[2].questionRegisters = new int?[] { null, 5, 9 }; // search for 7
                levinSearchProblem.trainingSamples[2].answerArray = new List<int> { 5, 9 };
                levinSearchProblem.trainingSamples[2].answerArrayIndex = 1; // result index

                levinSearchProblem.trainingSamples[3].questionArray = new List<int> { 1, };
                levinSearchProblem.trainingSamples[3].questionRegisters = new int?[] { null, 5, 9 }; // search for 7
                levinSearchProblem.trainingSamples[3].answerArray = new List<int> { 5, 1, 9 };
                levinSearchProblem.trainingSamples[3].answerArrayIndex = 2; // result index

                levinSearchTaskProvider.problems.Add(levinSearchProblem);

                /* should return

                	arraySetIdx 0
	                arrayInsert reg1
	                arraySetIdx -1
	                arrayInsert reg2
	                (ret)
                 */
            }

            if (false) { // problem : find value in reg1 with value in array[0] and stay with index
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 5;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16/*because no call*/;
                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/5 */*number of elements in array*/5;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { 5, 8, 3, 7 };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { null, 7 }; // search for 7
                levinSearchProblem.trainingSamples[0].answerArray = new List<int> { 5, 8, 3, 7 }; // don't change array
                levinSearchProblem.trainingSamples[0].answerArrayIndex = 3; // result index must be 3

                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 7, 8, 3, 2 };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { null, 7 }; // search for 7
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 7, 8, 3, 2 }; // don't change array
                levinSearchProblem.trainingSamples[1].answerArrayIndex = 0; // result index must be 3

                levinSearchTaskProvider.problems.Add(levinSearchProblem);

                /* should return
                 
                    compareArray reg1       3
                    jumpIfFlag    +2       
                    arrayIdx +1             1
                    jmp           -4
                    ret                    (5)
                 */
            }

            if (false) { // problem : remove at current array[0] position reg0 times
                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.enumerationMaxProgramLength = 6;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16;/*because no call*/
                
                levinSearchProblem.maxNumberOfRetiredInstructions = /*length of program*/5 */*number of elements in array*/5;

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;

                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples.Add(new TrainingSample());
                levinSearchProblem.trainingSamples[0].questionArray = new List<int> { 5, 8, 3, 7 };
                levinSearchProblem.trainingSamples[0].questionRegisters = new int?[] { 1, null }; // remove 1 element
                levinSearchProblem.trainingSamples[0].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[0].answerArray = new List<int> {  8, 3, 7 };
                levinSearchProblem.trainingSamples[0].answerArrayIndex = 0;

                levinSearchProblem.trainingSamples[1].questionArray = new List<int> { 5, 8, 3, 7 };
                levinSearchProblem.trainingSamples[1].questionRegisters = new int?[] { 1, null }; // remove 1 element
                levinSearchProblem.trainingSamples[1].questionArrayIndex = 1;
                levinSearchProblem.trainingSamples[1].answerArray = new List<int> { 5, 3, 7 };
                levinSearchProblem.trainingSamples[1].answerArrayIndex = 1;

                levinSearchProblem.trainingSamples[2].questionArray = new List<int> { 5, 8, 3, 7 };
                levinSearchProblem.trainingSamples[2].questionRegisters = new int?[] { 1, null }; // remove 1 element
                levinSearchProblem.trainingSamples[2].questionArrayIndex = 1;
                levinSearchProblem.trainingSamples[2].answerArray = new List<int> { 5, 3, 7 };
                levinSearchProblem.trainingSamples[2].answerArrayIndex = 1;

                levinSearchProblem.trainingSamples[3].questionArray = new List<int> { 5, 8, 3, 7 };
                levinSearchProblem.trainingSamples[3].questionRegisters = new int?[] { 2, null }; // remove 2 element
                levinSearchProblem.trainingSamples[3].questionArrayIndex = 1;
                levinSearchProblem.trainingSamples[3].answerArray = new List<int> { 5, 7 };
                levinSearchProblem.trainingSamples[3].answerArrayIndex = 1;

                levinSearchProblem.trainingSamples[4].questionArray = new List<int> { 5, 8, 3, 7 };
                levinSearchProblem.trainingSamples[4].questionRegisters = new int?[] { 0, null }; // remove 1 element
                levinSearchProblem.trainingSamples[4].questionArrayIndex = 0;
                levinSearchProblem.trainingSamples[4].answerArray = new List<int> { 5, 8, 3, 7 };
                levinSearchProblem.trainingSamples[4].answerArrayIndex = 0;

                levinSearchTaskProvider.problems.Add(levinSearchProblem);

                /* should return
                    compare 0 reg0
                    jumpIfFlag 3
                    dec reg0
                    arrayRemove
                    jmp -5
                    ret
                */
            }

            


            levinSearchTaskProvider.submitFirstTask();
            
            for (;;) {
                scheduler.process();
            }







            ComputeContext computeContext = new ComputeContext();

            ComputeExecutor computeExecutor = new ComputeExecutor(computeContext);
            

            computeContext.computeBudgetedTasks.list.Add(new ComputeNoOperationPerformedBugetedTask(Budget.makeByPriorityAndDecayExponent(1.0, -0.9), "testA"));
            computeContext.computeBudgetedTasks.list.Add(new ComputeNoOperationPerformedBugetedTask(Budget.makeByPriorityAndDecayExponent(0.5, -2.4), "testB"));


            //ComputeContextResourceRecorder computeResourceRecorder = new ComputeContextResourceRecorder(computeContext);

            for(int i=0;i<40;i++) {
                computeExecutor.run(0.1);
            }

            computeContext.computeBudgetedTasks.list.Add(new ComputeNoOperationPerformedBugetedTask(Budget.makeByPriorityAndDecayExponent(0.5, -2.4), "testCdyn"));

            for (int i = 0; i < 20; i++) {
                computeExecutor.run(0.1);
            }

            computeContext.computeBudgetedTasks.list.Add(new ComputeNoOperationPerformedBugetedTask(Budget.makeByPriorityAndDecayExponent(0.5, -2.4), "testDdyn"));

            for (int i = 0; i < 50; i++) {
                computeExecutor.run(0.1);
            }

            MathematicaReportGenerator mathematicaReportGenerator = new MathematicaReportGenerator(computeExecutor.recorder);
            
            
            Console.WriteLine(mathematicaReportGenerator.generate().getContentAsString());


            agentTest0();

            for (;;) {
                //double n = addX(5.0, 2.0, 3.0);

                //int x = (int)n;
            }
        }

        /*

        // http://www.wildml.com/2015/10/recurrent-neural-networks-tutorial-part-3-backpropagation-through-time-and-vanishing-gradients/
        void bptt(float[] y) {
            int T = y.Length;


            // results from forward propagation
            // TODO< actual forward propagation >
            Matrix s, o;



            Matrix dLdV;
            Matrix dLdW;

            Matrix[] delta_o = o.copy();
            // TODO subtract 1 from delta_o

            // for each output backwards
            for( int t = T-1; t >= 0; t-- ) {
                dLdV = dLdV.add(Matrix.outer(delta_o[t], s[t].transpose()));

                // initial delta calculation: dL/dz
                Matrix delta_t = this.V.transposed().dot(delta_o[t]) * (1.0f - (s[t].square()));

                // backpropagation through time (for at most this.bptt_truncate steps)
                for ( int bptt_step = t+1; bptt_step >= Math.Max(0, t-this.bptt_truncate); bptt_step-- ) {
                    // print "Backpropagation step t=%d bptt step=%d " % (t, bptt_step)

                    //Add to gradients at each previous step
                    dLdW = dLdW.add(Matrix.outer(delta_t, s[bptt_step - 1]));

                    /// i think i translated this correctly from python, but im not so sure
                    // python dLdU[:, x[bptt_step]] += delta_t
                    for( int ix = 0; ix < bptt_step; ix++ ) {
                        for( int iy = 0; iy; iy++ ) {
                            dLdU[ix, iy] += delta_t[ix, 0];
                        }
                    }

                    // Update delta for next step dL/dz at t-1
                    delta_t = this.W.transpose().dot(delta_t) * (1.0f - s[bptt_step - 1].square());
                }
                
            }
            
        }

        */

        int bptt_truncate; // how many steps are used for truncating the bptt
    }

    class Matrix {
        float[,] values;

        public Matrix(int sizeX, int sizeY) {
            values = new float[sizeX, sizeY];
        }

        public Matrix add(Matrix other) {
            if( values.GetLength(0) != other.values.GetLength(0) || values.GetLength(1) != other.values.GetLength(1) ) {
                throw new Exception();
            }

            Matrix result = new Matrix(values.GetLength(0), values.GetLength(1));
            for(int ix=0; ix < values.GetLength(0); ix++) {
                for (int iy = 0; iy < values.GetLength(1); iy++) {
                    result.values[ix, iy] = values[ix, iy] + other.values[ix, iy];
                }
            }
            
            return result;
        }

        // calculates the square of each number
        public Matrix square() {
            Matrix result = new Matrix(values.GetLength(0), values.GetLength(1));
            for(int ix=0;ix < values.GetLength(0);ix++) {
                for(int iy=0;iy < values.GetLength(1);iy++) {
                    result.values[ix, iy] = values[ix, iy] * values[ix, iy];
                }
            }
            return result;
        }

        // outer product
        public static Matrix outer(Matrix a, Matrix b) {
            if( a.values.GetLength(0) != b.values.GetLength(0) || a.values.GetLength(1) != 1 || b.values.GetLength(1) != 1 ) {
                throw new Exception();
            }

            int size = a.values.GetLength(0);

            Matrix result = new Matrix(size, size);

            for(int x = 0; x < size; x++) {
                for(int y = 0; y < size;y++ ) {
                    result.values[x, y] = a[y, 0] * b[x, 0];
                }
            }

            return result;
        }

        public float this[int x, int y] {
            get {
                return values[x, y];
            }
            set {
                values[x, y] = value;
            }
        }

        // slice by x
        public Matrix this[int x] {
            get {
                Matrix result = new Matrix(1, values.GetLength(1));
                for (int iy = 0; iy < values.GetLength(0); iy++ ) {
                    result.values[0, iy] = values[x, iy];
                }

                return result;
            }
            
            // set not implemented
        }
    }
}
