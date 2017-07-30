using System;
using System.Collections.Generic;
using System.Linq;
using WhiteSphereEngine.game;

namespace WhiteSphereEngine.entity {
    public class Entity {
        private Entity(IList<IComponent> components) {
            // inform components that they are added to the entity
            foreach( IComponent iComponent in components ) {
                iComponent.entry(this);
            }

            _components = components;
        }

        public IList<IComponent> components {
            get {
                return _components;
            }
        }

        public IEnumerable<Type> getComponentsByType<Type>() {
            return components.Where(v => v is Type).Select(v => (Type)v);
        }

        public Type getSingleComponentsByType<Type>() {
            IList<Type> componentsByType = new List<Type>(getComponentsByType<Type>());
            if( componentsByType.Count != 1 ) {
                throw new Exception("only one component expected");
            }

            return componentsByType[0];
        }

        public bool doesAnyComponentRequireUpdate() {
            if( privateCachedRequiresUpdate == null ) {
                // we need to recalculate the cache if an component needs an update
                privateCachedRequiresUpdate = components.Any(v => v.requiresUpdate);
            }
            return privateCachedRequiresUpdate.Value;
        }

        public void invalideRequiresUpdateCache() {
            privateCachedRequiresUpdate = null;
        }

        public void update() {
            foreach( IComponent iComponent in components ) {
                iComponent.update(this);
            }
        }

        // access controlled because adding or removing components can change the needsUpdate flag
        internal IList<IComponent> _components = new List<IComponent>();

        bool? privateCachedRequiresUpdate; // null means we have to update if it requires updates

        public static Entity make(IList<IComponent> components) {
            return new Entity(components);
        }
    }
}
