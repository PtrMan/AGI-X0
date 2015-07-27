package ptrman.agix0.SuboptimalProcedureLearner;

import ptrman.Datastructures.Variadic;

import java.util.List;

public class CommunicationChannel {
    public void signalExternalOperator(String name, List<Variadic> parameters) {
        System.out.println("EXT-OP (" + name + ") parameters [" + convertParametersToString(parameters) + "]");
    }

    private static String convertParametersToString(List<Variadic> parameters) {
        int parametersLastIndex = parameters.size() - 1;
        String result = "";
        for( int i = 0;i < parameters.size(); i++ ) {
            result += parameters.get(i).toString();
            if (i != parametersLastIndex) {
                result += ", ";
            }
             
        }
        return result;
    }

}
