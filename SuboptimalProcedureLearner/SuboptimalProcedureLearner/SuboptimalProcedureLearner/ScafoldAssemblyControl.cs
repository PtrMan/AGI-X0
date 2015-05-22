using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    class ScafoldAssemblyControl
    {
        public enum EnumExamineInputOutputType
        {
            INPUT,
            OUTPUT
        }

        public class OperatorStatistics
        {
            public int positive = 0;
            public int negative = 0;

            public void addBool(bool value)
            {
                if( value )
                {
                    positive++;
                }
                else
                {
                    negative++;
                }
                
            }

        }

        // public for testing
        public List<ExternalOperator> externalOperators = new List<ExternalOperator>();

        // public for testing
        public CommunicationChannel communicationChannel = new CommunicationChannel();

        public OperatorStatistics examineAllAxiomsOfInputOutput(SampleForInputOutput sample, EnumExamineInputOutputType type)
        {
            ExternalOperator chosenOperator;
            OperatorStatistics statistics;

            

            statistics = new OperatorStatistics();
            
            // TODO< abstract the input/output thing into a own class for better usage >
            List<Variadic> values;
            
            // TODO< use the types of the data and select the possible operators based on that type >

            // TODO< iterate through the possible operators >
            chosenOperator = externalOperators[0];

            if( type == EnumExamineInputOutputType.INPUT )
            {
                // TODO< case for the type >

                values = sample.input;
            }
            else if( type == EnumExamineInputOutputType.OUTPUT )
            {
                values = sample.output;
            }
            else
            {
                throw new Exception("Internal Error");
            }

            // TODO< switch for the type >
            
            // iterate through all values
            int i;

            for( i = 0; i < values.Count-1; i++ )
            {
                compareValuesWithNotificationAndStoreResultInStatistics(communicationChannel, statistics, chosenOperator, values[i], values[i + 1]);
            }

            // compare first and last
            compareValuesWithNotificationAndStoreResultInStatistics(communicationChannel, statistics, chosenOperator, values[0], values[values.Count - 1]);

            return statistics;
        }

        private static Variadic callExternalOperatorAndSignalOnCommunicationChannel(CommunicationChannel communicationChannel, ExternalOperator calledOperator, List<Variadic> parameters)
        {
            communicationChannel.signalExternalOperator(calledOperator.getName(), parameters);

            return calledOperator.call(parameters);
        }

        private static void compareValuesWithNotificationAndStoreResultInStatistics(CommunicationChannel communicationChannel, OperatorStatistics statistics, ExternalOperator calledOperator, Variadic left, Variadic right)
        {
            List<Variadic> operatorParameters;
            Variadic operatorResult;

            operatorParameters = new List<Variadic>();
            operatorParameters.Add(left);
            operatorParameters.Add(right);

            operatorResult = callExternalOperatorAndSignalOnCommunicationChannel(communicationChannel, calledOperator, operatorParameters);
            // the result is supposed to be binary
            // TODO< other types? >

            System.Diagnostics.Debug.Assert(operatorResult.type == Variadic.EnumType.BOOL);
            statistics.addBool(operatorResult.valueBool);
        }
    }
}
