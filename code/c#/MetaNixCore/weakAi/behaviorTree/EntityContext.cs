using MetaNix.common;

namespace MetaNix.weakAi.behaviorTree {
    // ripped and and translated to C# from my gameengine
    /** \brief Class that tells the behavior tree something about the context of usage, contains needed variables
     *
     */
    public abstract class EntityContext {
        protected EntityContext(string type) {
		    this.protectedType = type;
        }

        public string type {
            get {
                return protectedType;
            }
        }

        private string protectedType;
    }

    public abstract class EntityContextWithBlackboard : EntityContext {
        protected EntityContextWithBlackboard(string type) : base(type) {
        }

        public Blackboard blackboard = new Blackboard();
    }
}
