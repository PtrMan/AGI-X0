using System;
using System.Collections.Generic;

using WhiteSphereEngine.math;
using WhiteSphereEngine.physics.boundingVolume;

namespace WhiteSphereEngine.physics.rigidBody {
    public class ProximityDetector {
        public enum EnumShapeType {
            SPHERE,
            BOX,
        }

        public enum EnumEventType {
            ENTER,
            EXIT,
            INSIDE,
        }

        public SpatialVectorDouble dimensions {
            get {
                return privateDimensions;
            }
        }

        public SpatialVectorDouble center {
            get {
                return privateCenter;
            }
        }

        private ProximityDetector(EnumShapeType shape, SpatialVectorDouble center, SpatialVectorDouble dimensions) {
            this.privateShapeType = shape;
            this.privateCenter = center;
            this.privateDimensions = dimensions;
        }

        SpatialVectorDouble privateCenter;

        public void setCenter(SpatialVectorDouble center) {
            privateCenter = center;
            recalcBoundingVolume();
        }

        protected void recalcBoundingVolume() {
            if(privateShapeType == EnumShapeType.SPHERE) {
                cachedKDop = KDopUtilities.makeAabbKDopByCenterAndRadius(privateCenter, dimensions[0]);
            }
            else {
                throw new NotImplementedException("Shape not implemented");
            }
        }
        
        internal void handleEnterEventForObject(PhysicsComponent physicsComponent) {
            foreach (ObjectHandlerDelegateType iHandler in objectHandlers) {
                iHandler(EnumEventType.ENTER, physicsComponent);
            }
        }

        internal void handleExitEventForObject(PhysicsComponent physicsComponent) {
            foreach (ObjectHandlerDelegateType iHandler in objectHandlers) {
                iHandler(EnumEventType.EXIT, physicsComponent);
            }
        }

        internal void handleInsideEventForObject(PhysicsComponent physicsComponent) {
            foreach (ObjectHandlerDelegateType iHandler in objectHandlers) {
                iHandler(EnumEventType.INSIDE, physicsComponent);
            }
        }

        public bool handleParticles; // does the proximity detector handle particles

        SpatialVectorDouble privateDimensions;
        private EnumShapeType privateShapeType;

        public delegate void ObjectHandlerDelegateType(EnumEventType eventType, PhysicsComponent object_);
        public IList<ObjectHandlerDelegateType> objectHandlers = new List<ObjectHandlerDelegateType>();

        public delegate void ParticleHandlerDelegateType(EnumEventType eventType, PhysicsComponent particle);
        public IList<ParticleHandlerDelegateType> particleHandlers = new List<ParticleHandlerDelegateType>();

        internal KDop cachedKDop; // must not be null
        
        internal IList<PhysicsComponent> objectsInside = new List<PhysicsComponent>();
        internal IList<PhysicsComponent> particlesInside = new List<PhysicsComponent>();

        public static ProximityDetector makeSphere(SpatialVectorDouble center, double radius, bool handleParticles = false) {
            ProximityDetector detector = new ProximityDetector(EnumShapeType.SPHERE, center, new SpatialVectorDouble(new double[] { radius }));
            detector.cachedKDop = KDopUtilities.makeAabbKDopByCenterAndRadius(center, radius);
            detector.handleParticles = handleParticles;
            return detector;
        }
    }
}
