package SuboptimalProcedureLearner;

public abstract class AbstractOperatorBase {
    public enum EnumType {
        OPERATOR,
        SCAFFOLD
    }

    public AbstractOperatorBase(final EnumType type) {
        this.type = type;
    }

    final public boolean isScaffold() {
        return type == EnumType.SCAFFOLD;
    }

    final public boolean isOperator() {
        return type == EnumType.SCAFFOLD;
    }

    final public EnumType getType() {
        return type;
    }

    abstract public String getShortName();

    private final EnumType type;
}
