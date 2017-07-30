namespace WhiteSphereEngine.misc {
    // translation of https://github.com/PtrMan/SpaceSimCore/blob/master/include/ChangeCallback.hpp

    // templated abstraction for a coupled action to a changed value
    // can be used for
    // * recalculating a hash
    // * invalidating cache
    // * recalculating normals, inverse of matrix, etc
    public abstract class ChangeCallback<Type> {
        public ChangeCallback() {}

        public void set(Type value) {
		    currentValue = value;
            this.changed();
        }

        public Type get() {
            return currentValue;
        }

        protected abstract void changed();

    	private Type currentValue;
    }
}
