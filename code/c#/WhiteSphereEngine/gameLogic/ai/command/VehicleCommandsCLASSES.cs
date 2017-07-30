using System.Diagnostics;

using WhiteSphereEngine.physics.rigidBody;
using WhiteSphereEngine.math;
using WhiteSphereEngine.math.control;
using WhiteSphereEngine.ai.command;
using WhiteSphereEngine.game.responsibilities;

namespace WhiteSphereEngine.gameLogic.ai.command {
    // TODO< handle negative direction case >
    
    // command to align a vehicle to an vector
    public class VehicleAlignToCommand : ICommand {
        public static VehicleAlignToCommand makeByGettingPidConfigurationFromAttitudeAndAccelerationControlResponsibility(AttitudeAndAccelerationControlResponsibility attitudeAndAccelerationControlResponsibility, PhysicsEngine physicsEngine, double dt, SpatialVectorDouble targetDirection, EntityController controller, ulong controlledObjectId, double targetDerivationDistance) {
            Pid.Configuration yawPidConfiguration, pitchPidConfiguration;
            
            bool keyExists = attitudeAndAccelerationControlResponsibility.PidControlConfigurationByPhysicsObjectId.ContainsKey(controlledObjectId);
            if( !keyExists ) {
                throw new System.Exception("AttitudeAndAccelerationControlResponsibility has to know the controlled object Id!");
            }

            AttitudeAndAccelerationControlResponsibility.PidControlConfigurationOfEntity pidControlConfigurationOfEntity = attitudeAndAccelerationControlResponsibility.PidControlConfigurationByPhysicsObjectId[controlledObjectId];
            yawPidConfiguration = pidControlConfigurationOfEntity.ofYaw;
            pitchPidConfiguration = pidControlConfigurationOfEntity.ofPitch;

            return new VehicleAlignToCommand(physicsEngine, dt, targetDirection, controller, controlledObjectId, targetDerivationDistance, yawPidConfiguration, pitchPidConfiguration);
        }

        // \param dt the time diference of one simulation step, can be changed at runtime
        // \param targetDirection should be normalized
        // \param controller controller of the controlled entity
        public VehicleAlignToCommand(PhysicsEngine physicsEngine, double dt, SpatialVectorDouble targetDirection, EntityController controller,  ulong controlledObjectId, double targetDerivationDistance, Pid.Configuration yawPidConfiguration, Pid.Configuration pitchPidConfiguration) {
            this.physicsEngine = physicsEngine;
            this.dt = dt;
            this.controller = controller;
            this.targetDirection = targetDirection;
            this.controlledObjectId = controlledObjectId;
            this.targetDerivationDistance = targetDerivationDistance;

            Trace.Assert(physicsEngine.existObjectById(controlledObjectId)); // object must exist hold by the physics engine

            yawPid = Pid.makeByTargetAndConfiguration(0, yawPidConfiguration);
            pitchPid = Pid.makeByTargetAndConfiguration(0, pitchPidConfiguration);
        }


        public double dt;

        public void beginExecution() {
        }

        public EnumNonPreemptiveTaskState process() {
            

            bool objectExists;
            PhysicsComponent controlledObject = tryGetControlledObject(out objectExists);
            if (!objectExists) {
                return EnumNonPreemptiveTaskState.FINISHEDSUCCESSFUL; // we finished if the object we have to align doesn't exist anymore
            }


            SpatialVectorDouble
                forwardVector = controlledObject.forwardVector,
                upVector = controlledObject.upVector,
                sideVector = controlledObject.sideVector;
            
            double
                dotOfUpVectorAndTargetDirection = SpatialVectorDouble.dot(upVector, targetDirection),
                dotOfSideVectorAndTargetDirection = SpatialVectorDouble.dot(sideVector, targetDirection),
                dotOfForwardVectorAndTargetDirection = SpatialVectorDouble.dot(forwardVector, targetDirection);

            //if (processingBegun) {
                //pitchPid.reset(dotOfUpVectorAndTargetDirection);
                //yawPid.reset(dotOfSideVectorAndTargetDirection);
            //}
            //processingBegun = false;


            // the dot product results are like our rotation delta of the different axis
            // now we need to put these into our PID's for the different axis to get the control value(s)

            double currentPitchDerivative, currentYawDerivative;
            double pitchControl = pitchPid.step(dotOfUpVectorAndTargetDirection, dt, out currentPitchDerivative);
            double yawControl = yawPid.step(dotOfSideVectorAndTargetDirection, dt, out currentYawDerivative);


            // send it to the controller
            controller.inputPitch = (float)pitchControl;
            controller.inputYaw = (float)yawControl;

            // check for termination criterium of this Command
            if( Math.dist2FromZero(currentPitchDerivative, currentYawDerivative) < targetDerivationDistance ) {
                return EnumNonPreemptiveTaskState.FINISHEDSUCCESSFUL;
            }
            return EnumNonPreemptiveTaskState.INPROGRESS;
        }
        
        PhysicsComponent tryGetControlledObject(out bool objectExists) {
            objectExists = physicsEngine.existObjectById(controlledObjectId);
            if (objectExists) {
                return physicsEngine.getObjectById(controlledObjectId);
            }
            else {
                return null;
            }
        }

        bool processingBegun = true;

        PhysicsEngine physicsEngine;
        SpatialVectorDouble targetDirection;
        ulong controlledObjectId;
        double targetDerivationDistance;

        Pid yawPid, pitchPid;
        EntityController controller;
    }
}
