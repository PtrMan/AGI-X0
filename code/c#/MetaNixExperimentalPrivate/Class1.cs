﻿using AiThisAndThat.prototyping;
using MetaNix.framework.misc;
using MetaNix.framework.representation.x86;
using MetaNix.nars;
using MetaNix.nars.entity;
using MetaNix.nars.inference;
using System;
using System.Collections.Generic;

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

            entry = testUtilityAndSearch1;

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

        Random rng = new Random();



        // tests the "expected utility maximization" tree walk and code manipulation

        // one descision tree node adds instructions and the second node (which is the child) adds and remove instructions
        void testUtilityAndSearch1() {
            mutatedProgram = new X86Program();

            // set the utility function to one which calculates how useful the change of the mutated program is
            calcUtility = calcUtility_forMutatedProgram;

            ///---
            // create and add children UtilityTreeElement

            var childrenPropabilityChangeAndNode = new List<Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>>();

            var changeRecords = new X86ArchRecoverableProgramChange.ChangeRecords();
            var createdChangeRecordAdd = new X86ArchRecoverableProgramChange.ChangeRecord(X86ArchRecoverableProgramChange.ChangeRecord.EnumType.ADD);
            changeRecords.arr.Add(createdChangeRecordAdd);

            X86Instruction createdInstruction = new X86Instruction((X86Instruction.EnumInstructionType)rng.Next(0, X86Instruction.NUMBEROFINSTRUCTIONS));
            createdInstruction.dest = 1; // for testing constant
            createdInstruction.a = 1; // for testing constant
            createdChangeRecordAdd.instructionToAdd = createdInstruction;

            /*
            changeRecords10.arr.Add(new X86ArchRecoverableProgramChange.ChangeRecord(X86ArchRecoverableProgramChange.ChangeRecord.EnumType.ADD));
            changeRecords10.arr[0].instructionToAdd = new X86Instruction((X86Instruction.EnumInstructionType)rng.Next(0, X86Instruction.NUMBEROFINSTRUCTIONS));
            changeRecords10.arr[0].instructionToAdd.dest = 0;
            changeRecords10.arr[0].instructionToAdd.a = 1;
            */

            IArchitectureRecoverable recoverableProgramChange = new X86ArchRecoverableProgramChange(mutatedProgram, changeRecords);
            childrenPropabilityChangeAndNode.Add(
                new Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>(
                    0.5, // TODO< calculate relative propability
                    recoverableProgramChange,
                    new NullUtilityTreeElement() // children of node
                )
            );

            if( false ) { // do we want to add DELETE changes
                changeRecords = new X86ArchRecoverableProgramChange.ChangeRecords();
                changeRecords.arr.Add(new X86ArchRecoverableProgramChange.ChangeRecord(X86ArchRecoverableProgramChange.ChangeRecord.EnumType.REMOVE));
                changeRecords.arr[0].idxSource = 0;

                recoverableProgramChange = new X86ArchRecoverableProgramChange(mutatedProgram, changeRecords);
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

            /*
            var changeRecords0 = new X86ArchRecoverableProgramChange.ChangeRecords();
            changeRecords0.arr.Add(new X86ArchRecoverableProgramChange.ChangeRecord(X86ArchRecoverableProgramChange.ChangeRecord.EnumType.ADD));
            changeRecords0.arr[0].instructionToAdd = new X86Instruction(X86Instruction.EnumInstructionType.ADD_INT);
            changeRecords0.arr[0].instructionToAdd.dest = 0;
            changeRecords0.arr[0].instructionToAdd.a = 1;
            
            var recoverableProgramChange0 = new X86ArchRecoverableProgramChange(mutatedProgram, changeRecords0);

            var changeRecords1 = new X86ArchRecoverableProgramChange.ChangeRecords();
            changeRecords1.arr.Add(new X86ArchRecoverableProgramChange.ChangeRecord(X86ArchRecoverableProgramChange.ChangeRecord.EnumType.ADD));
            changeRecords1.arr[0].instructionToAdd = new X86Instruction(X86Instruction.EnumInstructionType.ADD_INTCONST);
            changeRecords1.arr[0].instructionToAdd.dest = 0;
            changeRecords1.arr[0].instructionToAdd.a = 5;
            

            var recoverableProgramChange1 = new X86ArchRecoverableProgramChange(mutatedProgram, changeRecords1);

            ProgramChangeBranchUtilityTreeElement treeElementRoot = new ProgramChangeBranchUtilityTreeElement(
                1.0, // propability
                new List<Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>>{
                    new Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>(
                        0.5, recoverableProgramChange0, treeElementChildren
                    ),

                    new Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>(
                        0.5, recoverableProgramChange1, treeElementChildren
                    ),

                }
            );
            */

            childrenPropabilityChangeAndNode = new List<Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>>();


            int numberOfAddedInstrCandidates = 1000; // how many (randomly chose) candidate instructions are selected for this node and this run

            for( int iAddedInstrCandidate = 0; iAddedInstrCandidate < numberOfAddedInstrCandidates; iAddedInstrCandidate++ ) {
                changeRecords = new X86ArchRecoverableProgramChange.ChangeRecords();
                createdChangeRecordAdd = new X86ArchRecoverableProgramChange.ChangeRecord(X86ArchRecoverableProgramChange.ChangeRecord.EnumType.ADD);
                changeRecords.arr.Add(createdChangeRecordAdd);

                createdInstruction = new X86Instruction((X86Instruction.EnumInstructionType)rng.Next(0, X86Instruction.NUMBEROFINSTRUCTIONS));
                createdInstruction.dest = 1; // for testing constant
                createdInstruction.a = 1; // for testing constant
                createdChangeRecordAdd.instructionToAdd = createdInstruction;

                recoverableProgramChange = new X86ArchRecoverableProgramChange(mutatedProgram, changeRecords);
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

                // TODO< log to log >
                if( false )   Console.WriteLine(string.Join(",", path));

                double expectedUtility = propability * calcUtilityFunction(path, returnUtility(path));
                if (pathOfHighestExpectedUtility == null || expectedUtility > highestExpectedUtility) {
                    highestExpectedUtility = expectedUtility;
                    pathOfHighestExpectedUtility = path;
                }
            }

            if( pathOfHighestExpectedUtility != null ) {
                Console.WriteLine("found path with an highest utility!");
            }


            
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
