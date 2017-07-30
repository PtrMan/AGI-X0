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

        static void Main(string[] args) {
            //TestSlimRnn.interactiveCheckLearningAlgorithm();

            EnumTest test = EnumTest.NARS;

            if( test == EnumTest.POWERPLAY ) {
                TestPowerplay.test();
            }
            else {
                testNars();
            }

            //TestSlimRnn.testCalculation();
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
    }
}
