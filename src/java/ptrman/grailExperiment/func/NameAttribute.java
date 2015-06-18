package ptrman.grailExperiment.func;

import com.syncleus.ferma.annotations.Property;

/**
 *
 */
public interface NameAttribute {
    @Property("name")
    void setName(String name);

    @Property("name")
    String getName();
}
