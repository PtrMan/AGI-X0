package ptrman.grailExperiment.func;

import com.syncleus.ferma.annotations.Property;

/**
 * Binding which connects usage sites with variables, constants, etc.
 */
public interface ValueBindingAttribute {
    // can be set
    @Property("site")
    public void setSite(String site);

    @Property("site")
    public String getSite();
}
