package ptrman.grailExperiment.func;

import com.syncleus.ferma.annotations.Property;

/**
 * Created by r0b3 on 16.06.15.
 */
public interface OperationAttribute {
    @Property("operation")
    String getOperation();

    @Property("operation")
    void setOperation(String operation);
}
