using System.Collections.Generic;

namespace WhiteSphereEngine.entity {
    public interface IComponent {
        // \param parentEntity is the entity into which the component got added
        // called before update or event are called
        // usually used to set the parentEntity
        // can be called multiple times
        void entry(Entity parentEntity);

        void update(Entity entity);

        // used to let components of the same entity communicate with each other without knowing their specific type
        void @event(string type, IDictionary<string, object> parameters);

        // does the component require an update, if not the update method is not called
        bool requiresUpdate {
            get;
        }
    }
}
