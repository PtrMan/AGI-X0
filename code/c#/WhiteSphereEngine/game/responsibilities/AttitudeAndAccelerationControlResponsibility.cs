using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using WhiteSphereEngine.math;
using WhiteSphereEngine.physics.rigidBody;
using WhiteSphereEngine.math.solvers;
using WhiteSphereEngine.math.control;

namespace WhiteSphereEngine.game.responsibilities {
    // maps attitude changes and acceleration to updates of the thrusters
    public class AttitudeAndAccelerationControlResponsibility {
        // holds the pid controller configuration of an entity
        public class PidControlConfigurationOfEntity {
            public Pid.Configuration ofYaw, ofPitch;
        }

        public IDictionary<ulong, PidControlConfigurationOfEntity> PidControlConfigurationByPhysicsObjectId = new Dictionary<ulong, PidControlConfigurationOfEntity>();

        public AttitudeAndAccelerationControlResponsibility(ThrusterResponsibility thrusterResponsibility) {
            this.thrusterResponsibility = thrusterResponsibility;
        }

        public void controlNamedPair(PhysicsComponent @object, string name, float relative) {
            IList<ThrusterResponsibility.ThrusterBinding> thrusterBindings;
            if( !thrusterResponsibility.physicsObjectIdToThrusters.TryGetValue(@object.id, out thrusterBindings)) {
                return;
            }
            
            foreach( var iThrusterBinding in thrusterBindings.Where(v => v.tag == name + "+")) {
                iThrusterBinding.relative += relative;
            }
            foreach (var iThrusterBinding in thrusterBindings.Where(v => v.tag == name + "-")) {
                iThrusterBinding.relative -= relative;
            }
        }

        // generalized way to rotate in a specific direction
        public void controlSolve(PhysicsComponent @object, float roll, float pitch, float yaw) {
            IList<ThrusterResponsibility.ThrusterBinding> thrusterBindings;
            if (!thrusterResponsibility.physicsObjectIdToThrusters.TryGetValue(@object.id, out thrusterBindings)) {
                return;
            }

            SpatialVectorDouble rotationTargetVector = new SpatialVectorDouble(new double[] {roll, yaw, pitch}); // vulkan coordinate system rotation

            double maximalAngularAccelerationMagnitude = 0.0;
            foreach (var iThrusterBinding in thrusterBindings) {
                maximalAngularAccelerationMagnitude = System.Math.Max(maximalAngularAccelerationMagnitude, iThrusterBinding.additionalInformation.cachedAngularAccelerationOnObject.length);
            }


            foreach (var iThrusterBinding in thrusterBindings) {
                SpatialVectorDouble normalizedAngularAccelerationOnObject;
                if(iThrusterBinding.additionalInformation.cachedAngularAccelerationOnObject.length < double.Epsilon) {
                    normalizedAngularAccelerationOnObject = new SpatialVectorDouble(new double[] { 0, 0, 0 });
                }
                else {
                    SpatialVectorDouble cachedAngularAccelerationOnObject = iThrusterBinding.additionalInformation.cachedAngularAccelerationOnObject;
                    double normalizedMangitude = cachedAngularAccelerationOnObject.length / maximalAngularAccelerationMagnitude;
                    normalizedAngularAccelerationOnObject = cachedAngularAccelerationOnObject.normalized().scale(normalizedMangitude);
                }

                // calculate the relative thrust by the dot product because we want to rotate the best way in the wished rotation acceleration (given by roll, pitch, yaw)
                double relativeThrust = SpatialVectorDouble.dot(normalizedAngularAccelerationOnObject, rotationTargetVector);
                relativeThrust = System.Math.Max(relativeThrust, 0); // thruster can only thrust positivly

                iThrusterBinding.relative += (float)relativeThrust; // now we add to not cancel away the effects of the other thrusters
            }
        }


        /* UNCOMMENTED BECAUSE the simplex algorithm doesn't solve our problems

        // TODO< handle negative cases, negative roll, pitch, yaw >

        // uses simplex solver to calculate the ideal thruster combination for the requested acceleration
        // \param thrusterSimplexEquations are vectors which are the equations used to solve with the help of the simple algorithm
        public void controlRotationAccelerationSolve(PhysicsComponent @object, IList<Matrix> thrusterSimplexEquations, float roll, float pitch, float yaw) {
            IList<ThrusterResponsibility.ThrusterBinding> thrusterBindings;
            if (!thrusterResponsibility.physicsObjectIdToThrusters.TryGetValue(@object.id, out thrusterBindings)) {
                return;
            }


            Trace.Assert(thrusterSimplexEquations.Count == thrusterBindings.Count);


            simplexSolver.matrix = Matrix.makeByRowsAndColumns(thrusterSimplexEquations.Count, 6 + 1);
            transferThrusterSimplexEquationVectorsToTabelauMatrix(simplexSolver.matrix, thrusterSimplexEquations);

            int lastRowIndex = (int)simplexSolver.matrix.rows - 1;


            // transfer target angular acceleration 
            // we negate the values because our simplex solves works with reversed values
            simplexSolver.matrix[lastRowIndex, 0] = -roll;
            simplexSolver.matrix[lastRowIndex, 1] = -pitch;
            simplexSolver.matrix[lastRowIndex, 2] = -yaw;

            // null other values
            simplexSolver.matrix[lastRowIndex, 3] = 0;
            simplexSolver.matrix[lastRowIndex, 4] = 0;
            simplexSolver.matrix[lastRowIndex, 5] = 0;

            simplexSolver.matrix[lastRowIndex, 6] = 0;

            // solve
            simplexSolver.iterate();

            // now we can read out and interpret the result
            // the values in the last column are the thrust values of the thrusters (relative between -1 to 1)
            for( int thrusterBindingI = 0; thrusterBindingI < thrusterBindings.Count; thrusterBindingI++ ) {
                thrusterBindings[thrusterBindingI].relative += (float)simplexSolver.matrix[thrusterBindingI, 6];
            }
        }

        private static void transferThrusterSimplexEquationVectorsToTabelauMatrix(Matrix tabelauMatrix, IList<Matrix> thrusterSimplexEquations) {
            for (int thrusterSimplexEquationsI = 0; thrusterSimplexEquationsI < thrusterSimplexEquations.Count; thrusterSimplexEquationsI++) {
                // copy row to column
                for (int columnI = 0; columnI < thrusterSimplexEquations[thrusterSimplexEquationsI].columns; columnI++) {
                    tabelauMatrix[thrusterSimplexEquationsI, columnI] = thrusterSimplexEquations[thrusterSimplexEquationsI][0, columnI];
                }
            }
        }*/

        // resets the force information of all thrusters
        public void resetAllThrusters() {
            foreach( ulong iPhysicsObjectKey in thrusterResponsibility.physicsObjectIdToThrusters.Keys) {
                var thrusterBindings = thrusterResponsibility.physicsObjectIdToThrusters[iPhysicsObjectKey];
                foreach( var iThrusterBinding in thrusterBindings ) {
                    iThrusterBinding.relative = 0.0f;
                }
            }
        }

        public void limitAllThrusters() {
            foreach (ulong iPhysicsObjectKey in thrusterResponsibility.physicsObjectIdToThrusters.Keys) {
                var thrusterBindings = thrusterResponsibility.physicsObjectIdToThrusters[iPhysicsObjectKey];
                foreach (var iThrusterBinding in thrusterBindings) {
                    iThrusterBinding.relative = Math.clamp(iThrusterBinding.relative, 0.0f, 1.0f);
                }
            }
        }

        public void transferThrusterForce() {
            foreach (ulong iPhysicsObjectKey in thrusterResponsibility.physicsObjectIdToThrusters.Keys) {
                var thrusterBindings = thrusterResponsibility.physicsObjectIdToThrusters[iPhysicsObjectKey];
                foreach (var iThrusterBinding in thrusterBindings) {
                    iThrusterBinding.attachedForce.forceInNewton = (double)iThrusterBinding.relative * iThrusterBinding.maximalForce;
                }
            }
        }
        

        ThrusterResponsibility thrusterResponsibility;
        SimplexSolver simplexSolver = new SimplexSolver();
    }
}
