package SuboptimalProcedureLearner;

import Datastructures.Variadic;
import ptrman.misc.Assert;

import java.util.ArrayList;
import java.util.List;

public class ScafoldAssemblyControl {
    public enum EnumExamineInputOutputType {
        INPUT,
        OUTPUT
    }
    public static class OperatorStatistics {
        public int positive = 0;
        public int negative = 0;

        public void addBool(boolean value) {
            if (value) {
                positive++;
            }
            else {
                negative++;
            } 
        }
    
    }

    // public for testing
    public List<ExternalOperator> externalOperators = new ArrayList<>();
    // public for testing
    public CommunicationChannel communicationChannel = new CommunicationChannel();
    public OperatorStatistics examineAllAxiomsOfInputOutput(SampleForInputOutput sample, EnumExamineInputOutputType type) {
        ExternalOperator chosenOperator;
        OperatorStatistics statistics;
        statistics = new OperatorStatistics();
        // TODO< abstract the input/output thing into a own class for better usage >
        List<Variadic> values = new ArrayList<>();
        // TODO< use the types of the data and select the possible operators based on that type >
        // TODO< iterate through the possible operators >
        chosenOperator = externalOperators.get(0);
        if (type == EnumExamineInputOutputType.INPUT) {
            // TODO< case for the type >
            values = sample.input;
        }
        else if (type == EnumExamineInputOutputType.OUTPUT) {
            values = sample.output;
        }
        else {
            throw new RuntimeException("Internal Error");
        }

        // TODO< switch for the type >
        // iterate through all values
        for( int i = 0;i < values.size() - 1;i++ ) {
            compareValuesWithNotificationAndStoreResultInStatistics(communicationChannel, statistics, chosenOperator, values.get(i), values.get(i + 1));
        }
        // compare first and last
        compareValuesWithNotificationAndStoreResultInStatistics(communicationChannel, statistics, chosenOperator, values.get(0), values.get(values.size() - 1));
        return statistics;
    }

    private static Variadic callExternalOperatorAndSignalOnCommunicationChannel(CommunicationChannel communicationChannel, ExternalOperator calledOperator, List<Variadic> parameters) {
        communicationChannel.signalExternalOperator(calledOperator.getName(), parameters);
        return calledOperator.call(parameters);
    }

    private static void compareValuesWithNotificationAndStoreResultInStatistics(CommunicationChannel communicationChannel, OperatorStatistics statistics, ExternalOperator calledOperator, Variadic left, Variadic right) {
        List<Variadic> operatorParameters = new ArrayList<>();
        operatorParameters.add(left);
        operatorParameters.add(right);
        Variadic operatorResult = callExternalOperatorAndSignalOnCommunicationChannel(communicationChannel, calledOperator, operatorParameters);
        // the result is supposed to be binary
        // TODO< other types? >
        Assert.Assert(operatorResult.type == Variadic.EnumType.BOOL, "");
        statistics.addBool(operatorResult.valueBool);
    }
}
