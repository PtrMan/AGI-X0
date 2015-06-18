package ptrman.grailExperiment.func;

import com.syncleus.ferma.annotations.Property;

/**
 *
 */
public interface ConstantAttribute {
    @Property("type")
    void setType(String type);

    @Property("type")
    String getType();

    @Property("value")
    void setValue(String value);

    @Property("value")
    String getValue();
}
