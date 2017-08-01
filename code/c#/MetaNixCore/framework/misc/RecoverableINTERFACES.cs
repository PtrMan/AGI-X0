namespace MetaNix.framework.misc {
    public interface IRecoverable {
        // do specific changes
        void commit();

        // revert changes
        void rollback();
    }

    // used to store and rollback (global) changes of the architecture or subarchitecture(detail of the components) of the Agent/AI
    public interface IArchitectureRecoverable : IRecoverable {
    }
}
