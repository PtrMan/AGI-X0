using System;
using System.Collections.Generic;

using Datastructures;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    abstract class ExternalOperator
    {
        abstract public Variadic call(List<Variadic> parameters);

        /**
         * gets the types which are required by call
         * 
         * \param resultType
         * \param parametersTypes output list which will contain the types after the call, the number of the parameters is also returned with this (as the length of the returned list)
         */
        abstract public void inspectTypes(out Variadic.EnumType resultType, List<Variadic.EnumType> parametersTypes);

        abstract public string getName();
    }
}
