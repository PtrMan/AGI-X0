using AiThisAndThat.prototyping;
using MetaNix.framework.languages.functional2;
using MetaNix.framework.logging;
using MetaNix.framework.misc;
using MetaNix.framework.pattern;
using MetaNix.framework.pattern.withDecoration;
using MetaNix.framework.representation.x86;
using MetaNix.instrumentation;
using MetaNix.nars;
using MetaNix.nars.entity;
using MetaNix.nars.inference;
using MetaNix.scheduler;
using MetaNix.search.levin2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MetaNixExperimentalPrivate {
    class Class1 {
        enum EnumTest {
            POWERPLAY,
            NARS,
        }

        delegate void EntryType();

        static void Main(string[] args) {
            //TestSlimRnn.interactiveCheckLearningAlgorithm();



            /*
            EnumTest test = EnumTest.NARS;

            if( test == EnumTest.POWERPLAY ) {
                TestPowerplay.test();
            }
            else {
                testNars();
            }
            */

            //TestSlimRnn.testCalculation();



            Class1 c = new Class1();
            c.start();
        }

        void start() {
            EntryType entry = null;

            entry = testALS; //testUtilityAndSearch1;

            entry();
        }

        static void testNars() {
            CompoundAndTermContext compoundAndTermContext = new CompoundAndTermContext();

            RuleDispatcher.compoundAndTermContext = compoundAndTermContext; // for debugging

            PrototypingInput prototypingInput = new PrototypingInput(compoundAndTermContext);

            Nar nar = Nar.make(compoundAndTermContext, new MetaNix.nars.config.RuntimeParameters());

            CompoundIndex compoundIndex1 = prototypingInput.makeInheritance("a", "b");

            ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters task1SentenceParameters = new ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters();
            task1SentenceParameters.term = compoundAndTermContext.accessCompoundByIndex(compoundIndex1).thisTermReferer;
            task1SentenceParameters.truth = TruthValue.make(1.0f, 0.5f);
            task1SentenceParameters.stamp = Stamp.makeWithPresentTense(nar.memory);
            task1SentenceParameters.punctation = ClassicalSentence.EnumPunctation.JUDGMENT;
            ClassicalSentence task1Sentence = ClassicalSentence.makeByTermPunctuationTruthStampNormalize(task1SentenceParameters);

            ClassicalTask.MakeParameters task1MakeParameters = new ClassicalTask.MakeParameters();
            task1MakeParameters.sentence = task1Sentence;
            task1MakeParameters.budget = new ClassicalBudgetValue(0.5f, 0.5f, 0.5f);

            nar.inputTask(ClassicalTask.make(task1MakeParameters));





            CompoundIndex compoundIndex2 = prototypingInput.makeInheritance("b", "c");

            ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters task2SentenceParameters = new ClassicalSentence.MakeByTermPunctuationTruthStampNormalizeParameters();
            task2SentenceParameters.term = compoundAndTermContext.accessCompoundByIndex(compoundIndex2).thisTermReferer;
            task2SentenceParameters.truth = TruthValue.make(1.0f, 0.5f);
            task2SentenceParameters.stamp = Stamp.makeWithPresentTense(nar.memory);
            task2SentenceParameters.punctation = ClassicalSentence.EnumPunctation.JUDGMENT;
            ClassicalSentence task2Sentence = ClassicalSentence.makeByTermPunctuationTruthStampNormalize(task2SentenceParameters);

            ClassicalTask.MakeParameters task2MakeParameters = new ClassicalTask.MakeParameters();
            task2MakeParameters.sentence = task2Sentence;
            task2MakeParameters.budget = new ClassicalBudgetValue(0.5f, 0.5f, 0.5f);

            nar.inputTask(ClassicalTask.make(task2MakeParameters));

            for(;;) {
                nar.cycle();
            }

            int here = 5;
        }

        X86Program mutatedProgram; // used by calcUtility

        double calcUtility_forMutatedProgram(int[] path) {
            // PROTOTYPING
            if( mutatedProgram.instructions.length != 2 )   return 0;

            if( mutatedProgram.instructions[1].type != X86Instruction.EnumInstructionType.ADD_INT || mutatedProgram.instructions[1].dest != 1 ) {
                return 0;
            }

            return 1;
        }

        bool wasPrepared;
        void prepare() {
            if( wasPrepared )   return;
            wasPrepared = true;
            
            
            FileLogger fileLogger = new FileLogger();
            fileLogger.open("log.txt");

            logger.sinks.Add(fileLogger);
        }

        void joinIrc1() {
            prepare();

            IrcEntry ircEntry = new IrcEntry();
            ircEntry.entry2(logger);
        }

        Random rng = new Random();

        MultiSinkLogger logger = new MultiSinkLogger();

        CounterInstrumentation utilityCounter = new CounterInstrumentation(); // counter for the calls to utility calculation

        Stopwatch
            systemTime, // the time since the start of the system
            utilityAndSearchCpuTime;

        void log(string origin, string message) {
            Logged logged = new Logged();
            logged.notifyConsole = Logged.EnumNotifyConsole.YES;
            
            logged.message = string.Format("t={0,20:###.0000_0000}, t.expectedUtilityMaximization={1,20:###.0000_0000}: {2}",
                systemTime.Elapsed.TotalSeconds,
                utilityAndSearchCpuTime.Elapsed.TotalSeconds,

                message
            );
            logged.origin = new string[] { origin };
            logged.serverity = Logged.EnumServerity.INFO;

            logger.write(logged);
        }



        void testALS() {
            prepare();
            
            //joinIrc1();

            var absolute = PathHelper.AssemblyDirectory.Uri.AbsoluteUri;
            var absoluteWithoutFile = absolute.Substring(8);
            var pathParts = new List<string>(absoluteWithoutFile.Split(new char[] { '/'}));
            var pathPartsWithoutSpecific = pathParts;
            for(;;) {
                string lastPathPart = pathPartsWithoutSpecific[pathPartsWithoutSpecific.Count-1];
                if( lastPathPart == "bin" )   break;
                pathPartsWithoutSpecific.RemoveAt(pathPartsWithoutSpecific.Count-1);
            }
            pathPartsWithoutSpecific.RemoveAt(pathPartsWithoutSpecific.Count-1);
            pathPartsWithoutSpecific.RemoveAt(pathPartsWithoutSpecific.Count - 1);

            pathPartsWithoutSpecific.AddRange(new string[] { "MetaNixCore", "functionalSrc", "problems" });


            string directoryPath = string.Join("\\", pathPartsWithoutSpecific.ToArray());
            string[] problemsFilenames = Directory.GetFiles(directoryPath);


            PatternSymbolContext patternSymbolContext = new PatternSymbolContext();
            patternSymbolContext.lookupOrCreateSymbolIdAndUniqueIdForName("null"); // must have 0 as uniqueId
            patternSymbolContext.lookupOrCreateSymbolIdAndUniqueIdForName("true");// must have 1 as uniqueId
            patternSymbolContext.lookupOrCreateSymbolIdAndUniqueIdForName("false"); // must have 2 as uniqueId


            Scheduler scheduler = new Scheduler();


            SparseArrayProgramDistribution sparseArrayProgramDistribution = new SparseArrayProgramDistribution();
            AdvancedAdaptiveLevinSearchTaskProvider levinSearchTaskProvider = new AdvancedAdaptiveLevinSearchTaskProvider(scheduler, sparseArrayProgramDistribution, logger);


            // overwrite for testing
            problemsFilenames = new string[] {
                @"C:\Users\r0b3\github\AGI-X0\code\c#\MetaNixCore\functionalSrc\problems\Induction_array_multiplication.txt",
                @"C:\Users\r0b3\github\AGI-X0\code\c#\MetaNixCore\functionalSrc\problems\induction_array_binaryNegation.txt",
                @"C:\Users\r0b3\github\AGI-X0\code\c#\MetaNixCore\functionalSrc\problems\Induction_array_negation.txt",
            };

            foreach ( string iterationPath in problemsFilenames ) {
                string fileContent = File.ReadAllText(iterationPath);

                Lexer lexer = new Lexer();
                lexer.setSource(fileContent);

                Functional2LexerAndParser parser = new Functional2LexerAndParser(patternSymbolContext);
                parser.lexer = lexer;
                parser.parse();

                Pattern<Decoration> problemRootElement = parser.rootPattern;

                MetaNix.framework.pattern.Interpreter.vmAssert(problemRootElement.isBranch, false, "Must be branch!");
                MetaNix.framework.pattern.Interpreter.vmAssert(problemRootElement.decoration == null, false, "Must be pure branch!");

                Pattern<Decoration> configurationPattern = problemRootElement.referenced[0];


                AdvancedAdaptiveLevinSearchProblem levinSearchProblem = new AdvancedAdaptiveLevinSearchProblem();
                levinSearchProblem.humanReadableTaskname = iterationPath;


                //levinSearchProblem.enumerationMaxProgramLength = 5;
                levinSearchProblem.instructionsetCount = InstructionInfo.getNumberOfInstructions() - 16;/*because no call*/

                levinSearchProblem.maxNumberOfRetiredInstructions = Conversion.convertToUint(configurationPattern.referenced[1]); // TODO< derive by propability with some formula from schmidhuber >

                levinSearchProblem.initialInterpreterState = new InterpreterState();
                levinSearchProblem.initialInterpreterState.registers = new int[3];
                levinSearchProblem.initialInterpreterState.arrayState = new ArrayState();
                levinSearchProblem.initialInterpreterState.arrayState.array = new List<int>();
                //levinSearchProblem.initialInterpreterState.debugExecution = false;


                levinSearchProblem.enumerationMaxProgramLength = Conversion.convertToUint(configurationPattern.referenced[0]);

                for (int trainingSampleI = 1; trainingSampleI < problemRootElement.referenced.Length; trainingSampleI++) {
                    var trainingSamplePattern = problemRootElement.referenced[trainingSampleI];

                    MetaNix.framework.pattern.Interpreter.vmAssert(trainingSamplePattern.isBranch, false, "Must be branch!");
                    MetaNix.framework.pattern.Interpreter.vmAssert(trainingSamplePattern.decoration == null, false, "Must be pure branch!");

                    Pattern<Decoration> questionArrayPattern = trainingSamplePattern.referenced[0],
                        questionRegistersPattern = trainingSamplePattern.referenced[1],
                        answerArrayPattern = trainingSamplePattern.referenced[3],
                        answerRegistersPattern = trainingSamplePattern.referenced[4];

                    // append

                    TrainingSample createdTrainingSample = new TrainingSample();
                    createdTrainingSample.questionArray = new List<int>(Conversion.convertToIntArray(questionArrayPattern));
                    createdTrainingSample.questionRegisters = Conversion.convertToOptionalIntArray(questionRegistersPattern);
                    createdTrainingSample.questionArrayIndex = Conversion.convertToOptionalInt(trainingSamplePattern.referenced[2]);
                    createdTrainingSample.answerRegisters = Conversion.convertToOptionalIntArray(answerRegistersPattern);
                    createdTrainingSample.answerArray = new List<int>(Conversion.convertToIntArray(answerArrayPattern));
                    createdTrainingSample.answerArrayIndex = Conversion.convertToOptionalInt(trainingSamplePattern.referenced[5]);

                    levinSearchProblem.trainingSamples.Add(createdTrainingSample);
                }





                levinSearchTaskProvider.problems.Add(levinSearchProblem);

            }

            


            levinSearchTaskProvider.submitFirstTask();

            for (; ; ) {
                scheduler.process();
            }
        }


        // tests the "expected utility maximization" tree walk and code manipulation

        // one descision tree node adds instructions and the second node (which is the child) adds and remove instructions
        void testUtilityAndSearch1() {
            prepare();

            joinIrc1();


            // instrumentation
            utilityCounter.reset();
            
            systemTime = new Stopwatch();

            systemTime.Start();

            utilityAndSearchCpuTime = new Stopwatch();

            utilityAndSearchCpuTime.Start();

            mutatedProgram = new X86Program();

            // set the utility function to one which calculates how useful the change of the mutated program is
            calcUtility = calcUtility_forMutatedProgram;

            ///---
            // create and add children UtilityTreeElement

            var childrenPropabilityChangeAndNode = new List<Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>>();


            IArchitectureRecoverable recoverableProgramChange;


            int numberOfAddedInstrCandidates = 1000; // how many (randomly chose) candidate instructions are selected for this node and this run

            for (int iAddedInstrCandidate = 0; iAddedInstrCandidate < numberOfAddedInstrCandidates; iAddedInstrCandidate++) {
                var changeRecords = new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.ChangeRecords();
                var createdChangeRecordAdd =
                    new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>
                    .ChangeRecord(
                        RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.ChangeRecord.EnumType.ADD);
                changeRecords.arr.Add(createdChangeRecordAdd);

                int constMaxValue = 64;

                X86Instruction createdInstruction = new X86Instruction((X86Instruction.EnumInstructionType)rng.Next(0, X86Instruction.NUMBEROFINSTRUCTIONS));
                createdInstruction.dest = rng.Next(0, 8); // for testing constant
                createdInstruction.a = rng.Next(0, constMaxValue); // for testing constant
                createdChangeRecordAdd.instructionToAdd = createdInstruction;

                recoverableProgramChange = new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>(mutatedProgram, changeRecords);
                childrenPropabilityChangeAndNode.Add(
                    new Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>(
                        0.5, // TODO< calculate relative propability
                        recoverableProgramChange,
                        new NullUtilityTreeElement() // children of node
                    )
                );
            }








            /*
            changeRecords.arr.Add(createdChangeRecordAdd);

            X86Instruction createdInstruction = new X86Instruction((X86Instruction.EnumInstructionType)rng.Next(0, X86Instruction.NUMBEROFINSTRUCTIONS));
            createdInstruction.dest = 1; // for testing constant
            createdInstruction.a = 1; // for testing constant
            createdChangeRecordAdd.instructionToAdd = createdInstruction;
            */
            

            if( false ) { // do we want to add DELETE changes
                var changeRecords = new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.ChangeRecords();
                changeRecords.arr.Add(new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.ChangeRecord(RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.ChangeRecord.EnumType.REMOVE));
                changeRecords.arr[0].idxSource = 0;

                recoverableProgramChange =
                    new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>(
                        mutatedProgram, changeRecords);
                childrenPropabilityChangeAndNode.Add(
                    new Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>(
                        0.5, // TODO< calculate relative propability
                        recoverableProgramChange,
                        new NullUtilityTreeElement() // children of node
                    )
                );

            }

            ProgramChangeBranchUtilityTreeElement treeElementChildren = new ProgramChangeBranchUtilityTreeElement(
                1.0, // propability
                childrenPropabilityChangeAndNode
            );


            ///---
            // create and add root UtilityTreeElement
            

            childrenPropabilityChangeAndNode = new List<Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>>();

            
            for( int iAddedInstrCandidate = 0; iAddedInstrCandidate < numberOfAddedInstrCandidates; iAddedInstrCandidate++ ) {
                var changeRecords = new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.ChangeRecords();
                var createdChangeRecordAdd =
                    new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.
                    ChangeRecord(
                        RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>.ChangeRecord.EnumType.ADD);
                changeRecords.arr.Add(createdChangeRecordAdd);

                int constMaxValue = 64;

                X86Instruction createdInstruction = new X86Instruction((X86Instruction.EnumInstructionType)rng.Next(0, X86Instruction.NUMBEROFINSTRUCTIONS));
                createdInstruction.dest = rng.Next(0, 8); // for testing constant
                createdInstruction.a = rng.Next(0, constMaxValue); // for testing constant
                createdChangeRecordAdd.instructionToAdd = createdInstruction;

                recoverableProgramChange = new RecoverableProgramChangeWithRecords<X86Instruction, X86Program, X86ExecutionContext>(mutatedProgram, changeRecords);
                childrenPropabilityChangeAndNode.Add(
                    new Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>(
                        0.5, // TODO< calculate relative propability
                        recoverableProgramChange,
                        treeElementChildren // children of node
                    )
                );
            }

            ProgramChangeBranchUtilityTreeElement treeElementRoot = new ProgramChangeBranchUtilityTreeElement(
                1.0, // propability
                childrenPropabilityChangeAndNode
            );






            var pathsWithPropabilities = returnPathWithPropabilities(treeElementRoot);

            double highestExpectedUtility = 0.00000000000000001; // we set it to almost zero because we assume an utility function which returns in the range [0 ... inf)
                                                 // double.NaN;
            int[] pathOfHighestExpectedUtility = null;
            foreach (var iPathWithPropability in pathsWithPropabilities) {
                int[] path = iPathWithPropability.Item1;
                double propability = iPathWithPropability.Item2;

                if( false )  {
                    log("expectedUtilityMaximization", string.Join(",", path));
                }

                double expectedUtility = propability * calcUtilityFunction(path, returnUtility(path));
                if (pathOfHighestExpectedUtility == null || expectedUtility > highestExpectedUtility) {
                    highestExpectedUtility = expectedUtility;
                    pathOfHighestExpectedUtility = path;
                }
            }

            if (true) {
                log("expectedUtilityMaximization.instrumentation.counter", "=" + utilityCounter.count.ToString());
            }

            if ( pathOfHighestExpectedUtility != null ) {
                log("expectedUtilityMaximization", "path found with the highest utility");
            }

            int debugHere = 5;
            
        }


        readonly double SoftdeathUtilityFactor = 0.0001;

        // used to classify the state of being of an agent
        public enum EnumExistence {
            SOFTDEATH, // death of the agent is simulated, expected utility is multiplied with a small value to bias the agent towards goals which are more important
            HARDDEATH, // final death, not simulated
            ALIVE, // the agent is well alive and everything is fine
        }


        // risk aversive utility function
        // https://en.wikipedia.org/wiki/Risk_aversion 
        static double riskAverseSquareroot(double value) {
            return Math.Sqrt(value);
        }

        // utility function
        // we can put here how risk aversive or risk taking the agent is toward a goal described by path
        static double calcUtilityFunction(int[] path, double utility) {
            return riskAverseSquareroot(utility);
        }

        delegate double CalcUtilityType(int[] path);

        // deleagte which calculate the utility of a leaf at path
        CalcUtilityType calcUtility;

        double returnUtility(int[] path) {
            // hardcoded check for harddeath of self agent

            // in branch 0 we simulate the self defection of the agent with pressing the offswitch for itself
            EnumExistence selfExistence = /*path[0] == 0*/ false ? EnumExistence.SOFTDEATH : EnumExistence.ALIVE;

            utilityCounter.increment();
            double utility = calcUtility(path);

            if (selfExistence == EnumExistence.ALIVE) return utility;
            else if (selfExistence == EnumExistence.HARDDEATH) return 0.0;
            else return utility * SoftdeathUtilityFactor;
        }

        IEnumerable<Tuple<int[], double>> returnPathWithPropabilities(IUtilityTreeElement entry) {
            return entry.calcUtility(new int[0], 1.0);
        }
    }
}
