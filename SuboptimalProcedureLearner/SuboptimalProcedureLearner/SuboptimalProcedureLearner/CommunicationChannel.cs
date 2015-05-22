using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    class CommunicationChannel
    {
        public void signalExternalOperator(string name, List<Variadic> parameters)
        {
            Console.WriteLine("EXT-OP (" + name + ") parameters [" + convertParametersToString(parameters) + "]");
        }

        private static string convertParametersToString(List<Variadic> parameters)
        {
            int parametersLastIndex;
            int i;
            string result;

            parametersLastIndex = parameters.Count-1;
            
            result = "";

            for( i = 0; i < parameters.Count; i++ )
            {
                result += parameters[i].toString();

                if( i != parametersLastIndex )
                {
                    result += ", ";
                }
            }

            return result;
        }


    }
}
