using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using WhiteSphereEngine.serialization;
using WhiteSphereEngine.math.control;

namespace WhiteSphereEngine.demo {
    // serializes predefined objects for the demonstration to json
    public class DemoObjectSerializer {
        public static void seralizeAndWriteShip() {
            GameObjectTemplate gameObject = new GameObjectTemplate();

            double[] mainShapeDimensions = new double[] { 2, 0.8, 0.8 };
            double mainShapeMassInKilogram = 1.0;

            gameObject.mainMassShapeType = "box";
            gameObject.mainMass = mainShapeMassInKilogram;
            gameObject.mainMassDimensions = mainShapeDimensions;
            gameObject.meshPath = "meta:fromMainShape";

            gameObject.colliders = new List<Collider>();
            gameObject.colliders.Add(new Collider());
            gameObject.colliders[0].localPosition = new double[] { 0, 0, 0 };
            gameObject.colliders[0].localRotation = new double[] { 0, 0, 0 };
            gameObject.colliders[0].size = mainShapeDimensions;
            gameObject.colliders[0].shapeType = "box";

            gameObject.specialAttributes = new List<SpecialAttribute>();
            gameObject.specialAttributes.Add(new SpecialAttribute());
            gameObject.specialAttributes[0].type = "withVehicleControllerComponent";

            double pitchAndYawSidelengthOfThruster = 1.0; // should be > 0.0 for the normal 

            gameObject.thrusters = new List<Thruster>();
            gameObject.thrusters.Add(new Thruster());
            gameObject.thrusters[0].direction = new double[] { 0, 1, 0 };
            gameObject.thrusters[0].locationPosition = new double[] { pitchAndYawSidelengthOfThruster, -1, 0 };
            gameObject.thrusters[0].maximalForce = 1.0; // TODO< realistic value >

            
            gameObject.thrusters.Add(new Thruster());
            gameObject.thrusters[1].direction = new double[] { 1, 0, 0 };
            gameObject.thrusters[1].locationPosition = new double[] { -1, 0, 0 };
            gameObject.thrusters[1].maximalForce = 1.0; // TODO< realistic value >
            gameObject.thrusters[1].tag = "accelerate+";
            
            gameObject.thrusters.Add(new Thruster());
            gameObject.thrusters[2].direction = new double[] { 0, -1, 0 };
            gameObject.thrusters[2].locationPosition = new double[] { pitchAndYawSidelengthOfThruster, 1, 0 };
            gameObject.thrusters[2].maximalForce = 1.0; // TODO< realistic value >

            gameObject.thrusters.Add(new Thruster());
            gameObject.thrusters[3].direction = new double[] { 0, 0, -1 };
            gameObject.thrusters[3].locationPosition = new double[] { pitchAndYawSidelengthOfThruster, 0, 1};
            gameObject.thrusters[3].maximalForce = 1.0; // TODO< realistic value >

            gameObject.thrusters.Add(new Thruster());
            gameObject.thrusters[4].direction = new double[] { 0, 0, 1 };
            gameObject.thrusters[4].locationPosition = new double[] { pitchAndYawSidelengthOfThruster, 0, -1 };
            gameObject.thrusters[4].maximalForce = 1.0; // TODO< realistic value >

            // PID controller configuration for AI control
            {
                Pid.Configuration pidConfiguration = new Pid.Configuration();
                // -0.0004 shows an odd effect
                //pidConfiguration.integral = -0.0002; // -0.0001 -0.0002
                //pidConfiguration.derivative = 0.0005; // -0.000005
                //pidConfiguration.proportial = 0.0005; // 0.005

                //pidConfiguration.integral = -0.00015; // -0.0001 -0.0002
                //pidConfiguration.derivative = 0.0002; // 0.0005
                //pidConfiguration.proportial = 0.0005; // 0.0005

                //pidConfiguration.integral = -0.000015; // -0.0001 -0.0002
                //pidConfiguration.derivative = 0.00001; // 0.0005
                //pidConfiguration.proportial = 0.0001; // 0.0005

                //pidConfiguration.integral = -0.0001; // -0.0001 -0.0002
                //pidConfiguration.derivative = 0.0002; // 0.0005
                //pidConfiguration.proportial = 0.0001; // 0.0005


                //pidConfiguration.integral = -0.0000016; // -0.0001 -0.0002
                //pidConfiguration.derivative = 0.00001; // 0.0001
                //pidConfiguration.proportial = 0.00001; // 0.0005

                // works with a precision of up to 0.001 in 7000 steps
                pidConfiguration.integral = -0.0000016; // -0.0001 -0.0002
                pidConfiguration.derivative = 0.00001; // 0.0001
                pidConfiguration.proportial = 0.0001; // 0.0005
                


                gameObject.pidControllerConfiguration = new game.responsibilities.AttitudeAndAccelerationControlResponsibility.PidControlConfigurationOfEntity();
                gameObject.pidControllerConfiguration.ofYaw = pidConfiguration;
                gameObject.pidControllerConfiguration.ofPitch = pidConfiguration;
            }
            



            List <string> uriParts = new List<string>(AssemblyDirectory.Uri.Segments);
            uriParts.RemoveAt(0); // remove first "/"
            uriParts.RemoveRange(uriParts.Count - 4, 4);
            uriParts.AddRange(new string[] { "gameResources/", "prototypingShip.json" });
            string path = string.Join("", uriParts).Replace('/', '\\').Replace("%20", " ");

            string serializedJson = GameObjectTemplate.serialize(gameObject);
            File.WriteAllText(path, serializedJson);

            int debugMeHere = 1;
        }

        public static void serializeAndWriteMissile() {
            GameObjectTemplate gameObject = new GameObjectTemplate();

            double[] mainShapeDimensions = new double[] { 2, 0.8, 0.8 };
            double mainShapeMassInKilogram = 1.0;

            gameObject.mainMassShapeType = "box";
            gameObject.mainMass = mainShapeMassInKilogram;
            gameObject.mainMassDimensions = mainShapeDimensions;
            gameObject.meshPath = "meta:fromMainShape";

            gameObject.colliders = new List<Collider>();
            gameObject.colliders.Add(new Collider());
            gameObject.colliders[0].localPosition = new double[] { 0, 0, 0 };
            gameObject.colliders[0].localRotation = new double[] { 0, 0, 0 };
            gameObject.colliders[0].size = mainShapeDimensions;
            gameObject.colliders[0].shapeType = "box";

            gameObject.solids = new List<Solid>();
            gameObject.solids.Add(new Solid());
            gameObject.solids[0].size = mainShapeDimensions;
            gameObject.solids[0].fractionMass = mainShapeMassInKilogram;
            gameObject.solids[0].fractionIsotopeName = "Fe56";

            gameObject.solids[0].shapeType = "box";
            gameObject.solids[0].localPosition = new double[] { 0, 0, 0 };
            gameObject.solids[0].localRotation = new double[] { 0, 0, 0 };


            gameObject.effects = new List<Effect>();
            gameObject.effects.Add(new Effect());
            gameObject.effects[0].effectType = "explosion";
            gameObject.effects[0].localPosition = new double[] { 0, 0, 0 };


            gameObject.thrusters = new List<Thruster>();
            gameObject.thrusters.Add(new Thruster());
            gameObject.thrusters[0].direction = new double[] { 1, 0, 0 };
            gameObject.thrusters[0].locationPosition = new double[] { -1, 0, 0 };
            gameObject.thrusters[0].maximalForce = 1.0; // TODO< realistic value >
            gameObject.thrusters[0].tag = "accelerate+";

            List <string> uriParts = new List<string>(AssemblyDirectory.Uri.Segments);
            uriParts.RemoveAt(0); // remove first "/"
            uriParts.RemoveRange(uriParts.Count - 4, 4);
            uriParts.AddRange(new string[] { "gameResources/", "prototypingMissile.json" });
            string path = string.Join("", uriParts).Replace('/', '\\').Replace("%20", " ");

            string serializedJson = GameObjectTemplate.serialize(gameObject);
            File.WriteAllText(path, serializedJson);

            int debugMeHere = 1;
        }

        // from http://stackoverflow.com/a/283917
        public static UriBuilder AssemblyDirectory {
            get {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                return uri;
            }
        }

        public static string AssemblyDirectoryAsString {
            get {
                UriBuilder uri = AssemblyDirectory;
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
