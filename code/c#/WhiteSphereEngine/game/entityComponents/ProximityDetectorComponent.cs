using System.Collections.Generic;
using System.Linq;

using WhiteSphereEngine.entity;
using WhiteSphereEngine.physics.rigidBody;
using WhiteSphereEngine.math;

namespace WhiteSphereEngine.game.entityComponents {
    // component which manages a proximity detector bound to the entity and reacts to proximity detections
    public class ProximityDetectorComponent : IComponent {
        // \param detector the proximity detector of the physics engine
        // \param calledComponent component which is informed of the events
        private ProximityDetectorComponent(ProximityDetector detector, IComponent calledComponent) {
            this.detector = detector;
            this.calledComponent = calledComponent;
            detector.objectHandlers.Add(this.objectHandler);
        }


        public void entry(Entity parentEntity) {
            // do nothing
        }

        public void @event(string type, IDictionary<string, object> parameters) {
            // do nothing
        }

        public bool requiresUpdate => true;
        public void update(Entity entity) {
            // moves physics proximity detector


            PhysicsComponent physicsComponentOfEntity = entity.getSingleComponentsByType<PhysicsComponent>();
            
            // TODO< move to object local position >
            detector.setCenter(physicsComponentOfEntity.position);
        }

        // called by physics engine to handle changes of the proximity detector of the physics engine
        // the events are very raw and the real proximity (between vertices/surfaces to vertices/surfaces) must be calculated by this
        void objectHandler(ProximityDetector.EnumEventType eventType, PhysicsComponent @object) {
            if( physicsComponentBlacklist.Contains(@object) ) {
                return;
            }

            // TODO< calculate detailed distance checks >


            detailedHandleProximity(eventType, @object);
        }

        // called when all prefiltering and detailed distance checks succeeded
        void detailedHandleProximity(ProximityDetector.EnumEventType eventType, PhysicsComponent @object) {
            switch( eventType ) {
                case ProximityDetector.EnumEventType.ENTER:
                calledComponent.@event("proximityEnter", new Dictionary<string, object> {{"object", @object}});
                break;

                case ProximityDetector.EnumEventType.EXIT:
                calledComponent.@event("proximityExit", new Dictionary<string, object> { { "object", @object } });
                break;

                case ProximityDetector.EnumEventType.INSIDE:
                calledComponent.@event("proximityInside", new Dictionary<string, object> { { "object", @object } });
                break;
            }
        }

        public IList<PhysicsComponent> physicsComponentBlacklist = new List<PhysicsComponent>();
        ProximityDetector detector;
        IComponent calledComponent;

        public static ProximityDetectorComponent makeSphereDetector(PhysicsEngine physicsEngine, PhysicsComponent parentPhysicsComponent, double radius, IComponent calledComponent) {
            SpatialVectorDouble position = new SpatialVectorDouble(new double[] { 0, 0, 0 }); // set it to null because we don't know the position of the entity to which the detector is located
            ProximityDetector detector = ProximityDetector.makeSphere(position, radius, false);

            // ProximityDetectorComponent must be created before adding it to the physics engine, else we miss the initial enter events
            ProximityDetectorComponent createdProximityDetectorComponent = new ProximityDetectorComponent(detector, calledComponent);
            createdProximityDetectorComponent.physicsComponentBlacklist.Add(parentPhysicsComponent); // add it to the blacklist to filter out useless proximity detection events
            physicsEngine.addProximityDetector(detector);

            return createdProximityDetectorComponent;
        }
    }
}
