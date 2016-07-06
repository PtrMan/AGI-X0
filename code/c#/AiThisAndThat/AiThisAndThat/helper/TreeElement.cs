using System;

namespace helper {
    abstract class TreeElement<TypeType> {
        protected TreeElement(TypeType type) {
            this.type = type;
        }

        public TypeType type;
    }
}
