using System;
using System.Collections.Generic;
using System.Diagnostics;

using WhiteSphereEngine.math;
using WhiteSphereEngine.geometry;
using WhiteSphereEngine.physics.boundingVolume;
using WhiteSphereEngine.misc;
using WhiteSphereEngine.entity;

namespace WhiteSphereEngine.physics.rigidBody {
    public class PhysicsComponent : IComponent {
        public class BoundingVolume {
            public KDop kdop;

            public SpatialVectorDouble[] convexHull; // x,y,z,w of planes describing the convex hull
        }

        public IList<AttachedForce> attachedForces = new List<AttachedForce>();


        private PhysicsComponent() {}
        // inertia tensor can be null for particles which don't rotate
        internal PhysicsComponent(SpatialVectorDouble position, SpatialVectorDouble velocity, double mass, Matrix? inertiaTensor, ulong id) {
            Trace.Assert(position.height == 3 && velocity.height == 3);
            

            this.privateId = id;

            this.mass = mass;
            this.position = position;
            this.internalLastPosition = position; // TODO< minus last time step velocity ? >
            this.velocity = velocity;

            rotation = QuaternionUtilities.makeIdentity();
            eulerLocalAngularVelocity = new SpatialVectorDouble(new double[] { 0, 0, 0 });

            if( inertiaTensor != null ) {
                Trace.Assert(inertiaTensor.Value.height == 3 && inertiaTensor.Value.width == 3);
                this.internalInertiaTensor = new ChangeCallbackCalculateInverse();
                this.internalInertiaTensor.set(inertiaTensor.Value);
            }
        }

        private ulong privateId;

        public ulong id {
            get {
                return privateId;
            }
        }

        public RungeKutta4State rungeKutta4State;
        
        public void updatePosition(SpatialVectorDouble newPosition, SpatialVectorDouble oldPosition) {
            internalLastPosition = oldPosition;
            position = newPosition;
        }

        public SpatialVectorDouble position {
            get {
                return rungeKutta4State.x;
            }
            set {
                rungeKutta4State.x = value;
            }
        }
        
        public SpatialVectorDouble velocity {
            set {
                rungeKutta4State.v = value;
            }
            get {
                return rungeKutta4State.v;
            }
        }

        public SpatialVectorDouble lastPosition {
            get {
                return internalLastPosition;
            }
        }

        public double invMass {
            get {
                return privateInvMass;
            }
        }

        public double mass {
            get {
                return privateMass;
            }

            set {
                Trace.Assert(value > 0.0);
                privateMass = value;
                privateInvMass = 1.0 / value;
            }
        }

        // matrix must be used just for reading!
        public Matrix inertiaTensor {
            get {
                return internalInertiaTensor.get();
            }
        }

        public SpatialVectorDouble forwardVector {
            get {
                // this is an optimized code which effectivly multiplies the forward direction with the rotation matrix
                return new SpatialVectorDouble(MatrixUtilities.extractColumn3(calcLocalToGlobalRotationMatrix(), 0));
            }
        }

        public SpatialVectorDouble upVector {
            get {
                // this is an optimized code which effectivly multiplies the up direction with the rotation matrix
                return new SpatialVectorDouble(MatrixUtilities.extractColumn3(calcLocalToGlobalRotationMatrix(), 1));
            }
        }

        public SpatialVectorDouble sideVector {
            get {
                // this is an optimized code which effectivly multiplies the side direction with the rotation matrix
                return new SpatialVectorDouble(MatrixUtilities.extractColumn3(calcLocalToGlobalRotationMatrix(), 2));
            }
        }


        public SpatialVectorDouble calcAngularAccelerationOfRigidBodyForAppliedForce(SpatialVectorDouble objectLocalPositionOfForce, SpatialVectorDouble objectLocalForce) {
            return PhysicsEngineUtilities.calcAngularAccelerationOfRigidBodyForAppliedForce(internalInertiaTensor, objectLocalPositionOfForce, objectLocalForce);
        }


        public Matrix calcLocalToGlobalRotationMatrix() {
            return QuaternionUtilities.convToRotationMatrix4(rotation);
        }

        public Matrix calcGlobalToLocalRotationMatrix() {
            return QuaternionUtilities.convToRotationMatrix4(rotation).inverse();
        }

        public Matrix calcLocalToGlobalTranslationMatrix() {
            return MatrixUtilities.calcLocalToGlobalTranslationMatrix(position);
        }

        public Matrix calcGlobalToLocalTranslationMatrix() {
            return MatrixUtilities.calcGlobalToLocalTranslationMatrix(position);
        }

        public Matrix calcGlobalToLocalRotationAndTranslationMatrix() {
            return calcGlobalToLocalTranslationMatrix() * calcGlobalToLocalRotationMatrix();
        }

        public Matrix calcLocalToGlobalRotationAndTranslationMatrix() {
            return calcLocalToGlobalTranslationMatrix() * calcLocalToGlobalRotationMatrix();
        }

        public BoundingVolume boundingVolume;

        public void recalcBoundingVolume(IList<SpatialVectorDouble> vertices) {
            recalcBoundingVolumeKDop14(vertices);
            recalcConvexHull();
            checkConvexHull(vertices);
        }

        void recalcBoundingVolumeKDop14(IList<SpatialVectorDouble> vertices) {
            boundingVolume = new BoundingVolume();
            boundingVolume.kdop = KDop.calculateKdopFromVertices(vertices, 14);
        }

        // recalculates the convex hull based on the convex hull
        void recalcConvexHull() {
            boundingVolume.convexHull = new SpatialVectorDouble[boundingVolume.kdop.k];

            for ( int baseVectorI = 0; baseVectorI < boundingVolume.kdop.baseVectors.Length; baseVectorI++ ) {
                SpatialVectorDouble baseVector = boundingVolume.kdop.baseVectors[baseVectorI];

                // the plane is the normal and w of the plane is the dot product of the point on the plane and the normal, which is the min/max value of the k-dop
                // the normal of the plane is the base vector of the k-dop for max and the negative normal for min

                boundingVolume.convexHull[baseVectorI] = new SpatialVectorDouble(new double[] {-baseVector[0], -baseVector[1], -baseVector[2], boundingVolume.kdop.min[baseVectorI]});
                boundingVolume.convexHull[boundingVolume.kdop.k/2 + baseVectorI] = new SpatialVectorDouble(new double[] { baseVector[0], baseVector[1], baseVector[2], -boundingVolume.kdop.max[baseVectorI] });
            }
        }

        // just for debugging, checks if all points are contained in the convex hull
        void checkConvexHull(IList<SpatialVectorDouble> vertices) {
            foreach(SpatialVectorDouble iterationVertex in vertices) {
                foreach(SpatialVectorDouble iterationConvexHullPlane in boundingVolume.convexHull) {
                    double wOfPoint = Plane.dot3(iterationVertex, iterationConvexHullPlane) + iterationConvexHullPlane.w;
                    bool isAbovePlane = wOfPoint > 0.0;
                    Debug.Assert(!isAbovePlane);
                }
            }
        }

        public bool requiresUpdate => false;
        public void update(Entity entity) {
            // does nothing
        }

        public void entry(Entity parentEntity) {
            // does nothing
        }

        public void @event(string type, IDictionary<string, object> parameters) {
            // does nothing
        }

        // rotation
        public Quaternion rotation;
        public SpatialVectorDouble eulerLocalAngularVelocity; // length 3

        internal SpatialVectorDouble eulerAngularAcceleration; // used by physics engine
        internal ChangeCallbackCalculateInverse internalInertiaTensor; // can be null if it is a particle

        internal SpatialVectorDouble internalLastPosition; // length 3

        private double privateMass, privateInvMass;

        ///internal SpatialVectorDouble linearVelocityDelta; // used by physics engine

        // TODO< remove initalization >
        internal SpatialVectorDouble linearAcceleration = new SpatialVectorDouble(new double[]{0,0,0 }); // used by physics engine to keep track of the acceleration of this object over an timestep
    }
}
