package ptrman.agix0.src.java.SuboptimalProcedureLearner;

import ptrman.Datastructures.Variadic;
import ptrman.misc.Reference;

import java.util.List;

abstract public class ExternalOperator {
    abstract public Variadic call(List<Variadic> parameters);

    /**
     * gets the types which are required by call
     *
     * \param resultType
     * \param parametersTypes output list which will contain the types after the call, the number of the parameters is also returned with this (as the length of the returned list)
     */
    abstract public void inspectTypes(Reference<Variadic.EnumType> resultType, List<Variadic.EnumType> parametersTypes);

    abstract public String getName();
}