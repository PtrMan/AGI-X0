namespace MetaNix.weakAi.behaviorTree {
    // ripped and and translated to C# from my gameengine
    /** \brief is a task from the behaviour tree which does some AI related stuff
     *
     */
    public abstract class Task {
        public enum EnumReturn {
            SUCCESS,
            FAILURE,
            RUNNING, // should be called the next tick again
        }

        /** \brief is called from outer code and it executes the task
         *
         * \param Context the Context of the entity
         * \return Status code of the execution
         */
        public abstract EnumReturn run(EntityContext context);

        /** \brief resets the variables to its defaults
         *
         */
        public abstract void reset();

        /** \brief is cloning the objeect on which it got called
         *
         * \return ...
         */
        public abstract Task clone();

        /** \brief returns the minmal ticks between updates
         *
         * \return ...
         */
        public uint getRefreshtimeInTicks() {
            // TODO< add a variable for this >
            return 1;
        }
    }
}
