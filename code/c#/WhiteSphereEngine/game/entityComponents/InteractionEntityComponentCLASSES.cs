using System;
using System.Collections.Generic;

using WhiteSphereEngine.entity;

namespace WhiteSphereEngine.game.entityComponents {
    // component which doesn't do anything
    // useful to consume events for testing
    class DummyComponent : IComponent {
        public void entry(Entity parentEntity) {
        }

        public void @event(string type, IDictionary<string, object> parameters) {
        }

        public bool requiresUpdate => false;
        public void update(Entity entity) {
        }
    }

    // remaps events
    // if an event doesn't have an remapping it's just routed as it is to the calledComponent
    class EventRemapperComponent : IComponent {
        public EventRemapperComponent(IComponent calledComponent) {
            this.calledComponent = calledComponent;
        }

        public bool requiresUpdate => false;
        public void update(Entity entity) {
        }

        public void entry(Entity parentEntity) {
        }

        public void @event(string type, IDictionary<string, object> parameters) {
            if (eventTypeMap.ContainsKey(type)) {
                calledComponent.@event(eventTypeMap[type], parameters);
            }
            else {
                calledComponent.@event(type, parameters);
            }
        }

        public IDictionary<string, string> eventTypeMap = new Dictionary<string, string>();
        public IComponent calledComponent;
    }
}
