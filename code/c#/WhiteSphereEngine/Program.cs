using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System;
using System.IO;

using WhiteSphereEngine.math;
using WhiteSphereEngine.input;
using WhiteSphereEngine.subsystems.renderer.prototypeFormRenderer;
using WhiteSphereEngine.subsystemCommon;
using WhiteSphereEngine.gameLogic;
using WhiteSphereEngine.geometry;
using WhiteSphereEngine.physics.microTimestepSimulation;
using WhiteSphereEngine.physics.rigidBody;
using WhiteSphereEngine.game;
using WhiteSphereEngine.game.responsibilities;
using WhiteSphereEngine.serialization;
using WhiteSphereEngine.builder;
using WhiteSphereEngine.demo;
using WhiteSphereEngine.entity;
using WhiteSphereEngine.game.entityComponents;
using WhiteSphereEngine.math.solvers;
using WhiteSphereEngine.math.control;
using WhiteSphereEngine.gameLogic.ai.command;

using Button = WhiteSphereEngine.subsystems.gui.Button;
using WhiteSphereEngine.subsystems.gui;
using WhiteSphereEngine.subsystems.gui.input;

namespace WhiteSphereEngine {
    class Program {
        static void Main(string[] args) {
            {
                double[][] arr;

                arr = new double[3][];
                arr[0] = new double[4]{9, 8, 7, 6 };
                arr[1] = new double[4]{42, 13, 7, 2};
                arr[2] = new double[4]{ 101,102, 103, 104};

                MeshAttributeCLASSES meshComponent2 = MeshAttributeCLASSES.makeDouble4(arr);
                double[] readback = meshComponent2.getDouble4Accessor()[1];

                int breakPointHere = 42;
            }



            // test solver for trivial cases
            {
                double one = 1.0;

                double J11 = 1.0, J22 = 1.0, J33 = 1.0, // principal moments of inertia of the body
                    w1 = 0, w2 = 0, w3 = 0, // angular velocity of spacecraft in spacecraft frame

                    // result values
                    q1Dot, q2Dot, q3Dot, q4Dot,
                    w1Dot, w2Dot, w3Dot,

                    // coeefficients, taken from the paper
                    gamma = 0.7,
                    aCoefficient = 1.25,
                    d = 7.5, k = 3.0, betaOne = 0.001;

                Quaternion spacecraftOrientationError;
                // set to identity
                /*
                spacecraftOrientationError.i = 0;
                spacecraftOrientationError.j = 0;
                spacecraftOrientationError.k = 0;
                spacecraftOrientationError.scalar = 1;
                */
                spacecraftOrientationError = QuaternionUtilities.makeFromAxisAndAngle(new SpatialVectorDouble(new double[] { 1, 0, 0 }), 1.0);

                QuaternionFeedbackRegulatorForSpacecraft.calcControl(
                    J11, J22, J33,
                    spacecraftOrientationError.i, spacecraftOrientationError.j, spacecraftOrientationError.k, spacecraftOrientationError.scalar,
                    w1, w2, w3,
                    out q1Dot, out q2Dot, out q3Dot, out q4Dot,
                    out w1Dot, out w2Dot, out w3Dot,

                    aCoefficient,
                    gamma,
                    d, k,
                    betaOne
                );

                // changes must be zero
                Debug.Assert(q1Dot == 0);
                Debug.Assert(q2Dot == 0);
                Debug.Assert(q3Dot == 0);
                Debug.Assert(q4Dot == 0);
                Debug.Assert(w1Dot == 0);
                Debug.Assert(w2Dot == 0);
                Debug.Assert(w3Dot == 0);

                int breakPointHere = 42;
            }
            







            SimplexSolver simplexSolver = new SimplexSolver();
            /* first example from video tutorial, https://www.youtube.com/watch?v=Axg9OhJ4cjg, bottom row is negative unlike in the video
	         */
            simplexSolver.matrix = Matrix.makeByRowsAndColumns(3, 5);
            simplexSolver.matrix[0, 0] = 4;
            simplexSolver.matrix[0, 1] = 8;
            simplexSolver.matrix[0, 2] = 1;
            simplexSolver.matrix[0, 3] = 0;
            simplexSolver.matrix[0, 4] = 24;

            simplexSolver.matrix[1, 0] = 2;
            simplexSolver.matrix[1, 1] = 1;
            simplexSolver.matrix[1, 2] = 0;
            simplexSolver.matrix[1, 3] = 1;
            simplexSolver.matrix[1, 4] = 10;

            simplexSolver.matrix[2, 0] = -3;
            simplexSolver.matrix[2, 1] = -4;
            simplexSolver.matrix[2, 2] = 0;
            simplexSolver.matrix[2, 3] = 0;
            simplexSolver.matrix[2, 4] = 0;

            simplexSolver.iterate();

            // result must be ~16.67
            Debug.Assert(simplexSolver.matrix[2, 4] > 16.6 && simplexSolver.matrix[2, 4] < 16.8);









            // TODO< move into unittest for Matrix >

            Matrix toInverseMatrix = new Matrix(2, 2);
            toInverseMatrix[0, 0] = 2.0f;
            toInverseMatrix[0, 1] = 1.0f;
            toInverseMatrix[1, 0] = 2.0f;
            toInverseMatrix[1, 1] = 2.0f;

            Matrix inversedMatrix = toInverseMatrix.inverse();
            Debug.Assert(System.Math.Abs(inversedMatrix[0, 0] - 1.0f) < 0.001f);
            Debug.Assert(System.Math.Abs(inversedMatrix[0, 1] - -0.5f) < 0.001f);
            Debug.Assert(System.Math.Abs(inversedMatrix[1, 0] - -1.0f) < 0.001f);
            Debug.Assert(System.Math.Abs(inversedMatrix[1, 1] - 1.0f) < 0.001f);




            // test pid controller
            if(false) {
                Pid.Configuration pidConfiguration = new Pid.Configuration();
                pidConfiguration.integral = 0;
                pidConfiguration.derivative = 0.1;
                pidConfiguration.proportial = 0.3;

                Pid pid = Pid.makeByTargetAndConfiguration(0, pidConfiguration);

                double value = 1.0;

                double control;

                control = pid.step(value, 0.5);
                value += control;

                for( int i = 0; i < 10; i++) {
                    control = pid.step(value, 0.5);
                    value += control;

                    Console.WriteLine("value={0} control={1}", value, control);

                }

                int breakPointHere0 = 1;
            }









            EntityManager entityManager = new EntityManager();

            SolidResponsibility solidResponsibility;
            EffectResponsibility effectResponsibility;
            ThrusterResponsibility thrusterResponsibility;
            AttitudeAndAccelerationControlResponsibility attitudeAndAccelerationControlResponsibility;

            thrusterResponsibility = new ThrusterResponsibility();
            attitudeAndAccelerationControlResponsibility = new AttitudeAndAccelerationControlResponsibility(thrusterResponsibility);

            effectResponsibility = new EffectResponsibility();

            solidResponsibility = new SolidResponsibility();
            solidResponsibility.mapping = new PhysicsObjectIdToSolidClusterMapping();
            


            EntityController entityController = new EntityController();



            IKeyboardInputHandler keyboardInputHandler = new PlayerShipControlInputHandler(entityController);
            
            KeyboardInputRemapper keyboardInputRemapper = new KeyboardInputRemapper(keyboardInputHandler);



            SoftwareRenderer softwareRenderer = new SoftwareRenderer();

            PrototypeForm prototypeForm = new PrototypeForm();
            prototypeForm.softwareRenderer = softwareRenderer;
            IGuiRenderer guiRenderer = prototypeForm.softwareGuiRenderer;

            GuiContext guiContext = new GuiContext(guiRenderer);
            prototypeForm.guiContext = guiContext;

            KeyboardEventRouterOfGui keyboardEventRouterOfGui = new KeyboardEventRouterOfGui(guiContext.selectionTracker);
            KeyboardInputRouter keyboardInputRouter = new KeyboardInputRouter(keyboardInputRemapper, keyboardEventRouterOfGui);
            prototypeForm.keyboardInputRouter = keyboardInputRouter;

            prototypeForm.Show();


            PhysicsEngine physicsEngine = new PhysicsEngine();
            solidResponsibility.physicsEngine = physicsEngine;

            physicsEngine.collisionHandlers.Add(new DefaultCollisionHandler()); // add the default collision handler for absorbing all particles


            AttachedForce thruster = new AttachedForce(
                new SpatialVectorDouble(new double[] { 0, 1, 0 }), // localLocation
                new SpatialVectorDouble(new double[] { 1, 0, 0 }) // localDirection
            );

            PhysicsComponent physicsComponent;
            {
                double mass = 1.0;

                Matrix inertiaTensor = InertiaHelper.calcInertiaTensorForCube(mass, 1.0, 1.0, 1.0);
                physicsComponent = physicsEngine.createPhysicsComponent(new SpatialVectorDouble(new double[] { 0, 0, 10 }), new SpatialVectorDouble(new double[] { 0, 0, 0 }), mass, inertiaTensor);
            }
            physicsComponent.attachedForces.Add(thruster);

            MeshComponent meshComponent = new MeshComponent();
            meshComponent.mesh = new MeshWithExplicitFaces();
            meshComponent.mesh.faces = new MeshWithExplicitFaces.Face[1];
            meshComponent.mesh.faces[0] = new MeshWithExplicitFaces.Face();
            meshComponent.mesh.faces[0].verticesIndices = new uint[] { 0, 1, 2 };

            { // create a VerticesWithAttributes with just positions
                MutableMeshAttribute positionMeshAttribute = MutableMeshAttribute.makeDouble4ByLength(4);
                positionMeshAttribute.getDouble4Accessor()[0] = new double[] { -1, -1, 0, 1 };
                positionMeshAttribute.getDouble4Accessor()[1] = new double[] { 1, -1, 0, 1 };
                positionMeshAttribute.getDouble4Accessor()[2] = new double[] { 0, 1, 0, 1 };
                positionMeshAttribute.getDouble4Accessor()[3] = new double[] { 0, 0, 1, 1 };

                VerticesWithAttributes verticesWithAttributes = new VerticesWithAttributes(new AbstractMeshAttribute[]{positionMeshAttribute }, 0);
                meshComponent.mesh.verticesWithAttributes = verticesWithAttributes;
            }
            
            //TransformedMeshComponent transformedMeshComponentForPhysics = new TransformedMeshComponent();
            //transformedMeshComponentForPhysics.meshComponent = meshComponent;

            IList<ColliderComponent> colliderComponents = new List<ColliderComponent>();

            {
                SpatialVectorDouble colliderComponentSize = new SpatialVectorDouble(new double[] { 2, 2, 2 });
                SpatialVectorDouble colliderComponentLocalPosition = new SpatialVectorDouble(new double[] { 0, 0, 0 });
                SpatialVectorDouble colliderComponentLocalRotation = new SpatialVectorDouble(new double[] { 0, 0, 0 });

                ColliderComponent colliderComponent0 = ColliderComponent.makeBox(colliderComponentSize, colliderComponentLocalPosition, colliderComponentLocalRotation);
                colliderComponents.Add(colliderComponent0);
            }


            ///physicsEngine.physicsAndMeshPairs.Add(new PhysicsComponentAndCollidersPair(physicsComponent, colliderComponents));

            // same mesh for the visualization
            TransformedMeshComponent transformedMeshComponentForRendering = new TransformedMeshComponent();
            transformedMeshComponentForRendering.meshComponent = meshComponent;

            ///softwareRenderer.physicsAndMeshPairs.Add(new PhysicsComponentAndMeshPair(physicsComponent, transformedMeshComponentForRendering));


            

            physicsEngine.tick();

            
            // TODO< read object description from json and create physics and graphics objects >

            // TODO< read and create solid description from json >

            // create and store solids of rocket
            // refactored
            /*
            if ( true ) {
                

                SolidCluster solidClusterOfRocket = new SolidCluster();

                // create solid of rocket
                {
                    SpatialVectorDouble solidSize = new SpatialVectorDouble(new double[] { 2, 2, 2 });
                    double massOfSolidInKilogram = 1.0;
                    IList<CompositionFraction> solidCompositionFractions = new List<CompositionFraction>() { new CompositionFraction(new Isotope("Fe56"), massOfSolidInKilogram) };
                    Composition solidComposition = new Composition(solidCompositionFractions);

                    physics.solid.Solid solid = physics.solid.Solid.makeBox(solidComposition, solidSize);

                    SpatialVectorDouble solidLocalPosition = new SpatialVectorDouble(new double[] { 0, 0, 0 });
                    SpatialVectorDouble solidLocalRotation = new SpatialVectorDouble(new double[] { 0, 0, 0 });

                    solidClusterOfRocket.solids.Add(new SolidCluster.SolidWithPositionAndRotation(solid, solidLocalPosition, solidLocalRotation));
                }

                /// TODO, uncommented because it uses the not existing physics object
                /// solidResponsibility.mapping.physicsObjectIdToSolidCluster[rocketPhysicsObject.id] = solidClusterOfRocket;
            }
            */
            

            // add test particle
            if (false) {
                SpatialVectorDouble particlePosition = new SpatialVectorDouble(new double[] { 0, 0, 0 });
                SpatialVectorDouble particleVelocity = new SpatialVectorDouble(new double[] { 0, 0, 1 });

                PhysicsComponent particlePhysicsComponent = physicsEngine.createPhysicsComponent(particlePosition, particleVelocity, 1.0, null);
                physicsEngine.addParticle(particlePhysicsComponent);
            }


            // write game object for TestMissile
            if(true) {
                DemoObjectSerializer.seralizeAndWriteShip();
                DemoObjectSerializer.serializeAndWriteMissile();
            }

            GameObjectBuilder gameObjectBuilder = new GameObjectBuilder();
            gameObjectBuilder.physicsEngine = physicsEngine;
            gameObjectBuilder.softwareRenderer = softwareRenderer;
            gameObjectBuilder.solidResponsibility = solidResponsibility;
            gameObjectBuilder.effectResponsibility = effectResponsibility;
            gameObjectBuilder.thrusterResponsibility = thrusterResponsibility;
            gameObjectBuilder.attitudeAndAccelerationControlResponsibility = attitudeAndAccelerationControlResponsibility;



            // load missile game object from json and construct it
            if (false) {
                GameObjectTemplate deserilizedGameObjectTemplate;

                // load
                {
                    List<string> uriParts = new List<string>(AssemblyDirectory.Uri.Segments);
                    uriParts.RemoveAt(0); // remove first "/"
                    uriParts.RemoveRange(uriParts.Count - 4, 4);
                    uriParts.AddRange(new string[] { "gameResources/", "prototypingMissile.json" });
                    string path = string.Join("", uriParts).Replace('/', '\\').Replace("%20", " ");

                    string fileContent = File.ReadAllText(path);

                    deserilizedGameObjectTemplate = GameObjectTemplate.deserialize(fileContent);
                }

                // build game object from template
                Entity createdEntity;
                {
                    SpatialVectorDouble globalPosition = new SpatialVectorDouble(new double[] { 0, 0, 5 });
                    SpatialVectorDouble globalVelocity = new SpatialVectorDouble(new double[] { 0, 0, 0 });
                    createdEntity = gameObjectBuilder.buildFromTemplate(deserilizedGameObjectTemplate, globalPosition, globalVelocity);
                    entityManager.addEntity(createdEntity);
                }

                // manually add components
                {
                    // TODO< chemical explosive ignition component >
                    DummyComponent chemicalExplosiveIgnitionComponent = new DummyComponent();
                    
                    // remap proximityEnter to explode
                    EventRemapperComponent eventRemapper = new EventRemapperComponent(chemicalExplosiveIgnitionComponent);
                    eventRemapper.eventTypeMap["proximityEnter"] = "explode";


                    // TODO< blacklist somehow other missiles >
                    PhysicsComponent parentPhysicsComponent = createdEntity.getSingleComponentsByType<PhysicsComponent>();
                    ProximityDetectorComponent proximityDetector = ProximityDetectorComponent.makeSphereDetector(physicsEngine, parentPhysicsComponent, 20.0, eventRemapper);
                    entityManager.addComponentToEntity(createdEntity, proximityDetector);


                }
            }

            Entity playerShip;

            if (true) {
                GameObjectTemplate deserilizedGameObjectTemplate;

                // load
                {
                    List<string> uriParts = new List<string>(AssemblyDirectory.Uri.Segments);
                    uriParts.RemoveAt(0); // remove first "/"
                    uriParts.RemoveRange(uriParts.Count - 4, 4);
                    uriParts.AddRange(new string[] { "gameResources/", "prototypingShip.json" });
                    string path = string.Join("", uriParts).Replace('/', '\\').Replace("%20", " ");

                    string fileContent = File.ReadAllText(path);

                    deserilizedGameObjectTemplate = GameObjectTemplate.deserialize(fileContent);
                }

                // build game object from template
                {
                    SpatialVectorDouble globalPosition = new SpatialVectorDouble(new double[] { 0, 0, 10 });
                    SpatialVectorDouble globalVelocity = new SpatialVectorDouble(new double[] { 0, 0, 0 });
                    Entity createdEntity = gameObjectBuilder.buildFromTemplate(deserilizedGameObjectTemplate, globalPosition, globalVelocity);
                    entityManager.addEntity(createdEntity);

                    playerShip = createdEntity;
                }
            }


            Entity aiShip = null;
            EntityController aiShipController = new EntityController();
            VehicleAlignToCommand command = null;

            bool withTestAiShip = true;
            if( withTestAiShip ) {
                GameObjectTemplate deserilizedGameObjectTemplate;

                // load
                {
                    List<string> uriParts = new List<string>(AssemblyDirectory.Uri.Segments);
                    uriParts.RemoveAt(0); // remove first "/"
                    uriParts.RemoveRange(uriParts.Count - 4, 4);
                    uriParts.AddRange(new string[] { "gameResources/", "prototypingShip.json" });
                    string path = string.Join("", uriParts).Replace('/', '\\').Replace("%20", " ");

                    string fileContent = File.ReadAllText(path);

                    deserilizedGameObjectTemplate = GameObjectTemplate.deserialize(fileContent);
                }

                // build game object from template
                {
                    SpatialVectorDouble globalPosition = new SpatialVectorDouble(new double[] { 0, 0, 10 });
                    SpatialVectorDouble globalVelocity = new SpatialVectorDouble(new double[] { 0, 0, 0 });
                    Entity createdEntity = gameObjectBuilder.buildFromTemplate(deserilizedGameObjectTemplate, globalPosition, globalVelocity);
                    entityManager.addEntity(createdEntity);

                    aiShip = createdEntity;
                }

                { // control
                    aiShip.getSingleComponentsByType<VehicleControllerComponent>().controller = aiShipController;

                }

                { // AI initialization
                    double dt = 1.0 / 60.0;
                    SpatialVectorDouble targetDirection = new SpatialVectorDouble(new double[] { 1, 0, 0.1 }).normalized();
                    double targetDerivationDistance = 0.001;
                    ulong targetPhysicsObjectId = aiShip.getSingleComponentsByType<PhysicsComponent>().id;

                    command = VehicleAlignToCommand.makeByGettingPidConfigurationFromAttitudeAndAccelerationControlResponsibility(
                        attitudeAndAccelerationControlResponsibility,
                        physicsEngine,
                        dt,
                        targetDirection,
                        aiShipController,
                        targetPhysicsObjectId,
                        targetDerivationDistance);
                }
            }


            // 
            {

            }







            playerShip.getSingleComponentsByType<VehicleControllerComponent>().controller = entityController;

            

            // create test GUI
            {
                Button button = new subsystems.gui.Button();
                button.position = new SpatialVectorDouble(new double[] { 0.2, 0.2 });
                button.size = new SpatialVectorDouble(new double[] { 0.3, 0.1 });
                button.text = "[Gamesave05-res.save](35)";
                button.textScale = new SpatialVectorDouble(new double[] {0.05, 0.08 });

                guiContext.addGuiElement(button);
            }

            
            ulong frameCounter = 0;

            ulong debugFrameCounter = 0; // used to limit the frames we do for debugging a scenario
            bool isFixedScenarioDebugMode = false;

            // TODO< use logger >
            StreamWriter debugLogFileOfVariables = null; // used to debug variables and coeeficients of the debug scenario

            if (isFixedScenarioDebugMode ) {
                if (!File.Exists("E:\\myLogShipOrientation.txt")) {
                    using (var stream = File.Create("E:\\myLogShipOrientation.txt"));
                }
                debugLogFileOfVariables = File.AppendText("E:\\myLogShipOrientation.txt");
            }
            
            for (;;) {
                Console.WriteLine("frame={0}", frameCounter);

                guiContext.tick();

                physicsEngine.tick();

                if (!isFixedScenarioDebugMode) {
                    prototypeForm.Refresh();
                    
                    Application.DoEvents(); // HACK, EVIL
                }

                // AI
                {
                    // we just do this small AI because we are testing
                    if( false && aiShip != null ) {
                        command.process();
                    }
                }

                // AI - attitude control test
                //  we change the rotation velocity of the spacecraft directly to simulate "perfect" thruster control
                {
                    PhysicsComponent objectOfControlledSpacecraft = aiShip.getSingleComponentsByType<PhysicsComponent>();

                    double
                        // principal moments of inertia of the body
                        // the controller assumes that the body has an inertia tensor like an box, but small derivations are propably fine, too
                        J11 = objectOfControlledSpacecraft.inertiaTensor[0, 0], J22 = objectOfControlledSpacecraft.inertiaTensor[1, 1], J33 = objectOfControlledSpacecraft.inertiaTensor[2, 2], 

                        // angular velocity of spacecraft in spacecraft frame
                        w1 = objectOfControlledSpacecraft.eulerLocalAngularVelocity.x,
                        w2 = objectOfControlledSpacecraft.eulerLocalAngularVelocity.y,
                        w3 = objectOfControlledSpacecraft.eulerLocalAngularVelocity.z,

                        // result values
                        q1Dot, q2Dot, q3Dot, q4Dot,
                        w1Dot, w2Dot, w3Dot,

                        // coeefficients, taken from the paper
                        gamma = 0.7,
                        aCoefficient = 1.25,
                        d = 7.5, k = 3.0, betaOne = 0.001;

                    // calculate the rotation delta to our target orientation from the current orientation
                    double debugMagnitude = objectOfControlledSpacecraft.rotation.magnitude;

                    Quaternion spacecraftOrientationError = objectOfControlledSpacecraft.rotation.difference(QuaternionUtilities.makeFromEulerAngles(0, 0.5, 0));

                    QuaternionFeedbackRegulatorForSpacecraft.calcControl(
                        J11, J22, J33,
                        spacecraftOrientationError.i, spacecraftOrientationError.j, spacecraftOrientationError.k, spacecraftOrientationError.scalar,
                        w1, w2, w3,
                        out q1Dot, out q2Dot, out q3Dot, out q4Dot,
                        out w1Dot, out w2Dot, out w3Dot,

                        aCoefficient,
                        gamma,
                        d, k,
                        betaOne
                    );

                    Console.WriteLine(
                        "w1dot={0}, w2dot={1}, w3dot={2}", w1Dot.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), w2Dot.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), w3Dot.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)
                    );

                    if (isFixedScenarioDebugMode) {
                        debugLogFileOfVariables.WriteLine("w1dot={0}, w2dot={1}, w3dot={2}", w1Dot.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), w2Dot.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), w3Dot.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture));
                        debugLogFileOfVariables.Flush();
                    }

                    // access directly
                    objectOfControlledSpacecraft.eulerLocalAngularVelocity.x += (w1Dot * (1.0 / 60.0));
                    objectOfControlledSpacecraft.eulerLocalAngularVelocity.y += (w2Dot * (1.0 / 60.0));
                    objectOfControlledSpacecraft.eulerLocalAngularVelocity.z += (w3Dot * (1.0 / 60.0));
                }

                attitudeAndAccelerationControlResponsibility.resetAllThrusters();
                
                entityManager.updateAllEntities();

                // order after update is important
                attitudeAndAccelerationControlResponsibility.limitAllThrusters();
                attitudeAndAccelerationControlResponsibility.transferThrusterForce();

                //thruster.forceInNewton = 0.5 * entityController.inputAcceleration;
                
                if( !isFixedScenarioDebugMode ) {
                    Thread.Sleep((int)((1.0 / 60.0) * 1000.0));
                }

                if( isFixedScenarioDebugMode ) {
                    debugLogFileOfVariables.WriteLine("{0},", aiShip.getSingleComponentsByType<PhysicsComponent>().rotation.j.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture));
                    debugLogFileOfVariables.Flush();
                }
                //File.AppendText("E:\\myLogShipOrientation.txt").WriteLine("{0},", aiShip.getSingleComponentsByType<PhysicsComponent>().rotation.y);

                if (isFixedScenarioDebugMode && debugFrameCounter >= 8000) { // 30000
                    break;
                }

                debugFrameCounter++;

                frameCounter++;

            }
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
