using System.Collections.Generic;
using Helper;
using Helper.Input;
using Helper.Physics;
using Helper.Physics.PhysicsObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Helper.Lighting;
using System;
using Helper.Camera;
using Game;
using JigLibX.Collision;
using System.IO;
using JigLibX.Geometry;
using Helper.Physics.PhysicObjects;
using Helper.Camera.Cameras;


namespace RoboGame
{
    // Wiki: https://github.com/colbybhearn/3DPhysics/wiki
    public class RoboGame : BaseGame
    {
        
        #region Properties / Fields
        
        GameplayModes gameplaymode = GameplayModes.Rover;

        Model roverModel, wheelModel, landerModel;
        RoverObject myRover;
        Model terrainModel;
        Texture2D moon;
        Model cubeModel;
        Model sphereModel;
        LunarVehicle lander;
        Planet planet;
        Model roverRadar;
        Model RotArm;
        Model roverCam;
        Model Pole;

        Texture2D radar;
        Texture2D radar_icon;
        Texture2D laser_icon;
        Texture2D energy;

        // Sound fx
        SoundEffect spawn;
        SoundEffect motor;
        SoundEffect radar_noise;
        SoundEffect solar_wind;

        // Looping sounds (requires soundfx)
        SoundEffectInstance solar_wind_loop;
        SoundEffectInstance motor_running;
        SoundEffectInstance radar_noise_loop;
        
        Texture2D BlankBackground;
        #endregion

        #region Enumerations
        public enum InputGroups
        {

            Communication,
            Rover,
            Lander,
            Interface,
        }
        public enum AssetTypes
        {
            Rover,
            Radar1Pickup,
            Laser1Pickup,
            Lander,
        }
        public enum GameplayModes
        {
            Rover,
            Lander,
            Spectate,
        }
        public enum Sounds
        {
            SolarWind, // not a real thing, but is still awesome
            RoverMotor,
            RadarNoise, 
            RoverSpawn,
        }
        public enum CameraModes
        {
            FreeLook,
            ObjectWatch,
            RoverFirstPerson,
            ObjectChase
        }
        #endregion

        #region Initialization
        public RoboGame()
        {
            name = "RoBo Game";

            
        }
        public override void InitializeContent()
        {
            base.InitializeContent();

            try
            {
                cubeModel = Content.Load<Model>("Cube");
                sphereModel = Content.Load<Model>("Sphere");
                wheelModel = Content.Load<Model>("wheel");
                moon = Content.Load<Texture2D>("Moon");
                terrainModel = Content.Load<Model>("terrain2");
                roverModel = Content.Load<Model>("Rover2");
                wheelModel = Content.Load<Model>("wheel");
                landerModel = Content.Load<Model>("Lunar Lander");
                chatFont = Content.Load<SpriteFont>("debugFont");
                roverRadar = Content.Load<Model>("RoverRadar");
                roverCam = Content.Load<Model>("RoverCam");
                RotArm = Content.Load<Model>("RotArm");
                Pole = Content.Load<Model>("Pole");

                radar = Content.Load<Texture2D>("radar");
                radar_icon = Content.Load<Texture2D>("radar_icon");
                laser_icon = Content.Load<Texture2D>("laser_icon");
                energy = Content.Load<Texture2D>("Energy");

                ChatManager = new Chat(chatFont);
                ChatMessageReceived += new Helper.Handlers.ChatMessageEH(ChatManager.ReceiveMessage);

                spawn = Content.Load<SoundEffect>("spawn");
                motor = Content.Load<SoundEffect>("motor");
                radar_noise = Content.Load<SoundEffect>("radar_noise");
                solar_wind = Content.Load<SoundEffect>("solar_wind");


                #region Initialize Assets
                assetManager.AddAsset(AssetTypes.Rover, CreateRover);
                assetManager.AddAsset(AssetTypes.Laser1Pickup, CreateHighFrictionCube);
                assetManager.AddAsset(AssetTypes.Radar1Pickup, CreateSmallSphere);
                assetManager.AddAsset(AssetTypes.Lander, CreateLunarLander);
                #endregion
            }
            catch (Exception E)
            {

            }
        }
        public override void InitializeMultiplayer()
        {
            base.InitializeMultiplayer();

            if (isServer)
                SpawnPickups();
        }
        public override void InitializeEnvironment()
        {
            bool useCustomTerrain = false;

            if (useCustomTerrain)
            {
                try
                {
                    physicsManager.PhysicsSystem.Gravity = Vector3.Zero;
                    physicsManager.AddGravityController(new Vector3(0, -101, 0), 100, 10);
                    /*terrain = new Terrain(new Vector3(0, -15, 0), // position
                        //new Vector3(100f, .1f, 100f),  // X with, possible y range, Z depth 
                                            new Vector3(15000f, .55f, 15000f),  // X with, possible y range, Z depth 
                                            100, 100, graphicsDevice, moon);*/
                    planet = new Planet(new Vector3(0, -101, 0), // Position
                        new Vector3(100, 100, 100), // Radius
                        0, Math.PI / 8, 2, graphicsDevice, moon);


                    objectsToAdd.Add(planet.ID, planet);
                    //objectsToAdd.Add(terrain.ID, terrain);
                }
                catch (Exception E)
                {
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
                }
            }
            else
            {
                try
                {
                    // some video cards can't handle the >16 bit index type of the terrain
                    HeightmapObject heightmapObj = new HeightmapObject(terrainModel, Vector2.Zero, new Vector3(0, 0, 0));
                    objectsToAdd.Add(heightmapObj.ID, heightmapObj);
                }
                catch (Exception E)
                {
                    // if that happens just create a ground plane 
                    //planeObj = new PlaneObject(planeModel, 0.0f, new Vector3(0, -15, 0), "");
                    //newObjects.Add(planeObj.ID, planeObj);
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
                }
            }

            
        }
        public override void InitializeCameras()
        {
            cameraManager.AddCamera((int)CameraModes.FreeLook, new FreeCamera());
            cameraManager.AddCamera((int)CameraModes.RoverFirstPerson, new BaseCamera());
        }
        public override List<ViewProfile> GetViewProfiles()
        {
            List<ViewProfile> profiles = base.GetViewProfiles();
            profiles.Add(new ViewProfile((int)CameraModes.RoverFirstPerson,
                                        (int)AssetTypes.Rover, new Vector3(-.45f, 1.4f, .05f), .25f, new Vector3(0, (float)-Math.PI / 2.0f, 0), 1.0f));
            return profiles;
        }
        public override void InitializeInputs()
        {
            inputManager = new InputManager(this.name, GetDefaultControls());

            inputManager.AddInputMode(InputMode.Chat, (ChatDelegate)ChatCallback);
            UpdateInputs();
        }
        public override KeyMapCollection GetDefaultControls()
        {
            KeyMapCollection defControls = base.GetDefaultControls();
            defControls.Game = this.name;


            List<KeyBinding> cameraDefaults = new List<KeyBinding>();
            cameraDefaults.Add(new KeyBinding("Forward", Keys.NumPad8, false, false, false, KeyEvent.Down, CameraMoveForward));
            cameraDefaults.Add(new KeyBinding("Left", Keys.NumPad4, false, false, false, KeyEvent.Down, CameraMoveLeft));
            cameraDefaults.Add(new KeyBinding("Backward", Keys.NumPad5, false, false, false, KeyEvent.Down, CameraMoveBackward));
            cameraDefaults.Add(new KeyBinding("Right", Keys.NumPad6, false, false, false, KeyEvent.Down, CameraMoveRight));
            cameraDefaults.Add(new KeyBinding("Speed Increase", Keys.NumPad7, false, false, false, KeyEvent.Pressed, CameraMoveSpeedIncrease));
            cameraDefaults.Add(new KeyBinding("Speed Decrease", Keys.NumPad1, false, false, false, KeyEvent.Pressed, CameraMoveSpeedDecrease));
            cameraDefaults.Add(new KeyBinding("Height Increase", Keys.NumPad9, false, false, false, KeyEvent.Down, CameraMoveHeightIncrease));
            cameraDefaults.Add(new KeyBinding("Height Decrease", Keys.NumPad3, false, false, false, KeyEvent.Down, CameraMoveHeightDecrease));

            cameraDefaults.Add(new KeyBinding("Change Mode", Keys.Decimal, false, false, false, KeyEvent.Pressed, CameraModeCycle));
            cameraDefaults.Add(new KeyBinding("Home", Keys.Multiply, false, false, false, KeyEvent.Pressed, CameraMoveHome));
            //
            cameraDefaults.Add(new KeyBinding("Toggle Debug Info", Keys.F1, false, false, false, KeyEvent.Pressed, ToggleDebugInfo));
            cameraDefaults.Add(new KeyBinding("Toggle Physics Debug", Keys.F2, false, false, false, KeyEvent.Pressed, TogglePhsyicsDebug));
            KeyMap camControls = new KeyMap(GenericInputGroups.Camera.ToString(), cameraDefaults);

            List<KeyBinding> ClientDefs = new List<KeyBinding>();
            ClientDefs.Add(new KeyBinding("Escape", Keys.Escape, false, false, false, KeyEvent.Pressed, Stop));
            KeyMap clientControls = new KeyMap(GenericInputGroups.Client.ToString(), ClientDefs);


            // Car
            List<KeyBinding> roverDefaults = new List<KeyBinding>();
            //careDefaults.Add(new KeyBinding("Spawn", Keys.R, false, true, false, KeyEvent.Pressed, SpawnCar));
            roverDefaults.Add(new KeyBinding("Forward", Keys.Up, false, false, false, KeyEvent.Down, Accelerate));
            roverDefaults.Add(new KeyBinding("Left", Keys.Left, false, false, false, KeyEvent.Down, SteerLeft));
            roverDefaults.Add(new KeyBinding("Backward", Keys.Down, false, false, false, KeyEvent.Down, Deccelerate));
            roverDefaults.Add(new KeyBinding("Right", Keys.Right, false, false, false, KeyEvent.Down, SteerRight));
            roverDefaults.Add(new KeyBinding("Laser", Keys.B, false, false, false, KeyEvent.Down, UseLaser));
            roverDefaults.Add(new KeyBinding("Pan Camera Left", Keys.J, false, false, false, KeyEvent.Down, RoverCamPanLeft));
            roverDefaults.Add(new KeyBinding("Pan Camera Right", Keys.L, false, false, false, KeyEvent.Down, RoverCamPanRight));
            roverDefaults.Add(new KeyBinding("Pan Camera Up", Keys.I, false, false, false, KeyEvent.Down, RoverCamPanUp));
            roverDefaults.Add(new KeyBinding("Pan Camera Down", Keys.K, false, false, false, KeyEvent.Down, RoverCamPanDown));            
            KeyMap roverControls = new KeyMap(InputGroups.Rover.ToString(),roverDefaults);

            // player 

            // Spheres
            //cardefaults.Add(new KeyBinding("SpawnSpheres", Keys.N, false, true, false, KeyEvent.Pressed, SpawnSpheres));

            
            //Lunar Lander
            List<KeyBinding> landerDefaults = new List<KeyBinding>();
            //landerDefaults.Add(new KeyBinding("Spawn", Keys.Decimal, false, false, false, KeyEvent.Pressed, SpawnLander));
            landerDefaults.Add(new KeyBinding("Thrust Up", Keys.Space, false, false, false, KeyEvent.Down, LunarThrustUp));
            landerDefaults.Add(new KeyBinding("Pitch Up", Keys.NumPad5, false, false, false, KeyEvent.Down, LunarPitchUp));
            landerDefaults.Add(new KeyBinding("Pitch Down", Keys.NumPad8, false, false, false, KeyEvent.Down, LunarPitchDown));
            landerDefaults.Add(new KeyBinding("Roll Left", Keys.NumPad4, false, false, false, KeyEvent.Down, LunarRollLeft));            
            landerDefaults.Add(new KeyBinding("Roll Right", Keys.NumPad6, false, false, false, KeyEvent.Down, LunarRollRight));
            landerDefaults.Add(new KeyBinding("Yaw Left", Keys.NumPad7, false, false, false, KeyEvent.Down, LunarYawLeft));
            landerDefaults.Add(new KeyBinding("Yaw Right", Keys.NumPad9, false, false, false, KeyEvent.Down, LunarYawRight));
            KeyMap landerControls = new KeyMap(InputGroups.Lander.ToString(), landerDefaults);
            
            // Chat
            List<KeyBinding> commDefaults = new List<KeyBinding>();
            commDefaults.Add(new KeyBinding("Chat ", Keys.Enter, false, false, false, KeyEvent.Pressed, ChatKeyPressed));
            KeyMap commControls = new KeyMap(InputGroups.Communication.ToString(), commDefaults);

            // Interface
            List<KeyBinding> interfaceDefaults = new List<KeyBinding>();
            //interfaceDefaults.Add(new KeyBinding("Enter / Exit Vehicle", Keys.E, false, true, false, KeyEvent.Pressed, EnterExitVehicle));
            interfaceDefaults.Add(new KeyBinding("Spawn Lander", Keys.L, false, true, false, KeyEvent.Pressed, SpawnLander));
            interfaceDefaults.Add(new KeyBinding("Spawn Rover", Keys.R, false, true, false, KeyEvent.Pressed, Request_Rover));
            KeyMap interfaceControls = new KeyMap(InputGroups.Interface.ToString(), interfaceDefaults);


            defControls.AddMap(camControls);
            defControls.AddMap(clientControls);
            defControls.AddMap(roverControls);
            defControls.AddMap(landerControls);
            defControls.AddMap(commControls);
            defControls.AddMap(interfaceControls);
            Vector3 res = new Vector3();
            Vector3 l = Vector3.Left;
            Vector3 n = Vector3.Zero;

            Vector3.Subtract(ref l, ref n, out res);

            return defControls;
        }
        public override void InitializeSound()
        {
            base.InitializeSound();

            soundManager.AddSoundEffect(Sounds.RoverSpawn.ToString(), spawn, false);
            soundManager.AddSound(Sounds.RoverMotor.ToString(), motor, false, .3f, Helper.Audio.SoundTypes.Effect);
            soundManager.AddSound(Sounds.RadarNoise.ToString(), radar_noise, true, .5f, Helper.Audio.SoundTypes.Effect);
            soundManager.AddSound(Sounds.SolarWind.ToString(), solar_wind, true, .5f, Helper.Audio.SoundTypes.Effect);

            // Let's play this right away;  should not play on the server through
            if (isClient)
                soundManager.Play(Sounds.SolarWind.ToString());

            //CameraModeCycle();
            //cameraManager.currentCamera.TargetPosition = new Vector3(10, -8, 5);
        }

        public void CameraModeCycle()
        {
            cameraManager.NextCamera();
            //cameraManager.SetGobjectList(cameraMode.ToString(), new List<Gobject> { currentSelectedObject });
        }

        #region Assets
        // Standard and Static callback create methods for the asset manager
        private Gobject CreateRover()
        {
            RoverObject r = null;
            try
            {
                Vector3 pos = Vector3.Zero;
                float maxSteerAngle = 30.0f;
                float steerRate = 5.0f;
                float wheelSideFriction = 4.7f;
                float wheelFwdFriction = 5.0f;
                float wheelTravel = 0.2f;
                float wheelRadius = 0.4f;
                float wheelZOffset = 0.05f;
                float wheelRestingFrac = 0.45f;
                float wheeldampingFrac = 0.3f;
                int wheelNumRays = 1;
                float driveTorque = 200.0f;

                r = new RoverObject(0, pos, roverModel, wheelModel, roverRadar, cubeModel, RotArm, roverCam, Pole, maxSteerAngle, steerRate,
                    wheelSideFriction, wheelFwdFriction, wheelTravel, wheelRadius, wheelZOffset, wheelRestingFrac, wheeldampingFrac, wheelNumRays,
                    driveTorque, physicsManager.PhysicsSystem.Gravity.Length());
                r.Rover.EnableCar();
                r.Rover.Chassis.Body.AllowFreezing = false;
                
                if (isServer)
                    r.AddCollisionCallback(CollisionSkin_callbackFn);
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
            return r;
        }
        private Gobject CreateHighFrictionCube()
        {
            Vector3 size = new Vector3(1, 1, 1);
            // position of box was upper leftmost corner
            // body has world position
            // skin is relative to the body
            Box boxPrimitive = new Box(-.5f * size, Matrix.Identity, size); // relative to the body, the position is the top left-ish corner instead of the center, so subtract from the center, half of all sides to get that point.

            Gobject box = new Gobject(
                Vector3.Zero, // position can be setup just following call to assetManager.GetNewInstance
                size / 2,
                boxPrimitive,
                MaterialTable.MaterialID.NotBouncyRough,
                cubeModel,
                0 // asset name is set automatically by asset manager when requested
                );
            return box;
        }
        private Gobject CreateSmallSphere()
        {
            float radius = .4f;
            Sphere spherePrimitive = new Sphere(Vector3.Zero, radius);
            Gobject sphere = new Gobject(
                Vector3.Zero,
                Vector3.One * radius,
                spherePrimitive,
                sphereModel,
                true,
                0);
            return sphere;
        }
        private Gobject CreateLunarLander()
        {
            Vector3 scale = new Vector3(2, 2, 2);
            LunarVehicle lander = new LunarVehicle(
                Vector3.Zero,
                scale,
                Matrix.Identity,
                landerModel,
                0
                );

            return lander;
        }

        // Flexible create methods for the game 
        private RoverObject GetRover(Vector3 pos)
        {
            RoverObject rover = assetManager.GetNewInstance(AssetTypes.Rover) as RoverObject;
            rover.MoveTo(pos, Matrix.Identity);
            return rover;
        }
        private Gobject GetLaserPickup(Vector3 pos)
        {
            Gobject laser = assetManager.GetNewInstance(AssetTypes.Laser1Pickup);
            laser.Position = pos;
            return laser;
        }
        private Gobject GetRadarPickup(Vector3 pos)
        {
            Gobject radar = assetManager.GetNewInstance(AssetTypes.Radar1Pickup);
            radar.Position = pos;
            return radar;
        }
        public LunarVehicle GetLunarLander(Vector3 pos, Matrix orient)
        {
            LunarVehicle lander = assetManager.GetNewInstance(AssetTypes.Lander) as LunarVehicle;
            lander.Position = pos;
            lander.Orientation = orient;
            return lander;
        }
        #endregion

        #endregion

        #region Methods
        public override void Stop()
        {
            base.Stop();
        }

        private void EnterExitVehicle()
        {

            switch (gameplaymode)
            {
                case GameplayModes.Rover:
                    gameplaymode = GameplayModes.Spectate;
                    break;
                case GameplayModes.Lander:
                    gameplaymode = GameplayModes.Spectate;
                    break;
                case GameplayModes.Spectate:
                    if (currentSelectedObject == null)
                        return;
                    // turn on only those appropriate to the current Game mode
                    if (currentSelectedObject is CarObject)
                        gameplaymode = GameplayModes.Rover;
                    if (currentSelectedObject is LunarVehicle)
                        gameplaymode = GameplayModes.Lander;
                        break;
                default:
                    break;
            }

            UpdateInputs();
        }
        private void UpdateInputs()
        {
            // turn off all
            inputManager.DisableAllKeyMaps();
            // turn on always needed inputs
            inputManager.EnableKeyMap(GenericInputGroups.Client.ToString());
            inputManager.EnableKeyMap(GenericInputGroups.Camera.ToString());
            inputManager.EnableKeyMap(InputGroups.Communication.ToString());
            inputManager.EnableKeyMap(InputGroups.Interface.ToString());

            switch (gameplaymode)
            {
                case GameplayModes.Rover:
                    inputManager.EnableKeyMap(InputGroups.Rover.ToString());
                    break;
                case GameplayModes.Lander:
                    inputManager.EnableKeyMap(InputGroups.Lander.ToString());
                    break;
                case GameplayModes.Spectate:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// CLIENT SIDE  and  SERVER SIDE
        /// Called when a client receives an object update for an object it does not know about, instantiate one!
        /// Called when a server goes to add an object requested by a client
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="asset"></param>
        public override void AddNewObject(int objectid, int asset)
        {
            if (assetManager == null)
                return;
            // if our client is already using this object id for some reason 
            if (assetManager.isObjectIdInUse(objectid))
                // forget it, the next update will prompt it again, it will get added when it's safe.
                return;

            if (Content == null)
                return;
            
            Gobject newobject = assetManager.GetNewInstance((AssetTypes)asset);
            newobject.isOnClient = isClient;
            newobject.isOnServer = isServer;
            newobject.ID = objectid; // override whatever object ID the assetManager came up with, if it is safe to do so
            physicsManager.AddNewObject(newobject);
        }

        /// <summary>
        /// CLIENT SIDE
        /// 
        /// </summary>
        private void Request_Rover()
        {
            if(commClient!=null)
                // send a request to the server for an object of asset type "car"
                commClient.SendObjectRequest((int)AssetTypes.Rover);

            //spawn.Play();
            soundManager.Play(Sounds.RoverSpawn.ToString());
        }

        /// <summary>
        /// CLIENT SIDE
        /// client should do something oriented to the specific game here, like player bullets or cars.
        /// The server has granted the object request and this is where the client handle the response the server has sent back 
        /// This is called from the Network code, thus in the Network threads
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="asset"></param>
        public override void ProcessObjectAdded(int ownerid, int objectid, int asset)
        {
            Gobject newobject = assetManager.GetNewInstance((AssetTypes)asset);
            newobject.ID = objectid;
            physicsManager.AddNewObject(newobject);
            if (ownerid == MyClientID) // Only select the new car if its OUR new car
            {
                if (newobject is RoverObject)
                {
                    myRover = (RoverObject)newobject;
                    SelectGameObject(myRover);
                }
            }
        }

        private void SpawnPickups()
        {
            Random r = new Random((int)DateTime.Now.ToOADate());
            float x, z;
            
            for (int i = 0; i < 10; i++)
            {
                x = (float)(r.NextDouble()-.5);
                z = (float)(r.NextDouble()-.5);

                x= x*250;
                z= z*250;

                //Gobject box = physicsManager.GetBoxHighFriction(new Vector3(x, 3.0f, z), new Vector3(1.0f, 1.0f, 1.0f), Matrix.Identity, cubeModel, true);
                Gobject box = GetLaserPickup(new Vector3(x, 3.0f, z));
                physicsManager.AddNewObject(box);
            }


            for (int i = 0; i < 10; i++)
            {
                x = (float)(r.NextDouble() - .5);
                z = (float)(r.NextDouble() - .5);

                x = x * 250;
                z = z * 250;

                
                Gobject sphere = GetRadarPickup(new Vector3(x, 3.0f, z));
                physicsManager.AddNewObject(sphere);
            }

        }

        public override void UpdateCamera()
        {
            base.UpdateCamera();
            if (myRover == null)
                return;

            if (!(cameraManager.currentCamera is FreeCamera))
            {
                cameraManager.currentCamera.TargetPosition = myRover.GetCamPosition();
                cameraManager.currentCamera.CurrentPosition = myRover.GetCamPosition();
                //            cameraManager.currentCamera.Orientation = Quaternion.CreateFromRotationMatrix(myRover.GetRoverCamWorldMatrix());
                //cameraManager.currentCamera.LookInDirection(myRover.GetCameraDirection(), myRover.GetCameraUp());

                cameraManager.currentCamera.SetCurrentOrientation(myRover.GetCameraOrientation());
            }
        }

        #region Lunar
        private void LunarThrustUp()
        {
            if (lander == null)
                return;
            lander.SetVertJetThrust(.9f);
        }
        private void LunarPitchDown()
        {
            if (lander == null)
                return;
            lander.SetRotJetXThrust(-.4f);
            
        }
        private void LunarRollLeft()
        {
            if (lander == null)
                return;
            lander.SetRotJetZThrust(-.4f);
        }
        private void LunarPitchUp()
        {
            if (lander == null)
                return;
            lander.SetRotJetXThrust(.4f);
        }
        private void LunarRollRight()
        {
            if (lander == null)
                return;
            lander.SetRotJetZThrust(.4f);
        }
        private void LunarYawLeft()
        {
            if (lander == null)
                return;
            lander.SetRotJetYThrust(.4f);
        }
        private void LunarYawRight()
        {
            if (lander == null)
                return;
            lander.SetRotJetYThrust(-.4f);
        }
        #endregion

        #region Rover
        private void Accelerate()
        {
            if (myRover == null)
                return;
            myRover.SetAcceleration(1.0f);

            soundManager.Play(Sounds.RoverMotor.ToString());
            if (motor_running != null)
            {
                motor_running.Volume = 0.3f;

                motor_running.Play();
            }

        }
        private void Deccelerate()
        {
            if (myRover == null)
                return;
            myRover.SetAcceleration(-1.0f);
        }
        private void SteerLeft()
        {
            if (myRover == null)
                return;
            myRover.SetSteering(1.0f);
        }
        private void SteerRight()
        {
            if (myRover == null)
                return;
            myRover.SetSteering(-1.0f);
        }
        private void UseLaser()
        {
            if (myRover == null)
                return;
            myRover.SetShootLaser(1.0f);
        }

        public void RoverCamPanLeft()
        {
            if (myRover == null) return;
            myRover.AdjustCamYaw(.0005f);
        }

        public void RoverCamPanRight()
        {
            if (myRover == null) return;
            myRover.AdjustCamYaw(-.0005f);
        }
        public void RoverCamPanUp()
        {
            if (myRover == null) return;
            myRover.AdjustCamPitch(.0005f);
        }

        public void RoverCamPanDown()
        {
            if (myRover == null) return;
            myRover.AdjustCamPitch(-.0005f);
        }

        #endregion

        private Gobject SpawnRover(int ownerid, int objectid)
        {
            Gobject newobject = GetRover(new Vector3(90, 20, 20));
            newobject.ID = objectid;
            physicsManager.AddNewObject(newobject);
            if (ownerid == MyClientID) // Only select the new car if its OUR new car
            {
                myRover = (RoverObject)newobject;
                SelectGameObject(myRover);
            }
            return newobject;
        }

        bool CollisionSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            RoverObject rover = null;
            Gobject obj = null;
            if (skin0.Owner.ExternalData is RoverObject)
                rover = skin0.Owner.ExternalData as RoverObject;
            if (skin1.Owner == null)
                return true;
            if (skin1.Owner.ExternalData is Gobject)
                obj = skin1.Owner.ExternalData as Gobject;

            if(rover==null || obj == null)
                return true;

            if (objectsToDelete.Contains(obj.ID)) // if the object is going to be deleted soon,
                return false; // don't bother doing any collision with it

            int type = obj.type;
            if ((AssetTypes)type == AssetTypes.Laser1Pickup)
            {
                rover.SetLaser(true);
                DeleteObject(obj.ID);
                return false;
            }
            if ((AssetTypes)type == AssetTypes.Radar1Pickup)
            {
                rover.SetRadar(true);
                DeleteObject(obj.ID);
                return false;
            }
            return true;
        }

        private void SpawnLander()
        {
            if (commClient != null)
            {
                commClient.SendObjectRequest((int)AssetTypes.Lander);
            }
        }
        private void ChatKeyPressed()
        {
            inputManager.Mode = InputMode.Chat;
            ChatManager.Typing = true;
        }
        private void ChatCallback(List<Microsoft.Xna.Framework.Input.Keys> pressed)
        {
            ChatMessage message;
            if (ChatManager.KeysPressed(pressed, out message))
            {
                inputManager.Mode = InputMode.Mapped;
                ChatManager.Typing = false;
                if (message != null)
                    SendChatPacket(message);
            }
        }

        #region Graphics
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            
            // Lets draw names for cars!
            List<Vector3> pos = new List<Vector3>();
            List<string> text = new List<string>();
            lock (gameObjects)
            {
                for (int i = 0; i < objectsOwned.Count; i++)
                {
                    int playerId = objectsOwned.Values[i];
                    int objectId = objectsOwned.Keys[i];
                    string alias;
                    Gobject g;
                    if (gameObjects.TryGetValue(objectId, out g) && players.TryGetValue(playerId, out alias))
                    {
                        pos.Add(g.GetPositionAbove());
                        text.Add(alias);
                    }
                }
            }
            if (BlankBackground == null)
            {
                BlankBackground = new Texture2D(sb.GraphicsDevice, 1, 1);
                BlankBackground.SetData(new Color[] { Color.White });
            }

            //Now that we're no longer blocking, lets draw
            for (int i = 0; i < pos.Count; i++)
            {
                // TODO - magic number
                if (Vector3.Distance(cameraManager.currentCamera.CurrentPosition, pos[i]) < 100)
                {
                    Vector3 screen = sb.GraphicsDevice.Viewport.Project(pos[i], cameraManager.ProjectionMatrix(), cameraManager.currentCamera.GetViewMatrix(), Matrix.Identity);
                    
                    int size = (int)chatFont.MeasureString(text[i]).X;
                    sb.Draw(BlankBackground, new Microsoft.Xna.Framework.Rectangle((int)screen.X - size/2, (int)screen.Y, size, chatFont.LineSpacing), Color.Gray * .5f);
                    sb.DrawString(chatFont, text[i], new Vector2(screen.X - size/2, screen.Y), Color.White);
                }
            }

            ChatManager.Draw(sb);
            
            if(isClient) // only clients need sound
                FrameworkDispatcher.Update(); // Sounds like this called early and often

            if (myRover != null)
            {            
                int nrg = (int)myRover.Energy;
                sb.Draw(energy, new Microsoft.Xna.Framework.Rectangle(5, 5, nrg, 5), Color.White);

                if (myRover.hasRadar)
                {
                    try
                    {
                        if (isClient)
                            soundManager.Play(Sounds.RadarNoise.ToString());
                    }
                    catch(Exception x)
                    {
                        string tacos = x.Message;
                    }

                    // The radar map                
                    if(radar!=null)
                        sb.Draw(radar, new Microsoft.Xna.Framework.Rectangle(10, 900, 100, 100), Color.White);

                    // The Icon
                    if(radar_icon != null)
                        sb.Draw(radar_icon, new Microsoft.Xna.Framework.Rectangle(10, 10, 50, 50), Color.White);
                }

                if (myRover.hasLaser)
                {
                    // The Icon
                    if(laser_icon!=null)
                        sb.Draw(laser_icon, new Microsoft.Xna.Framework.Rectangle(70, 10, 50, 50), Color.White);
                }
            }

        }
        #endregion

        #endregion

    }
}
