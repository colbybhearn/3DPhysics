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
using System.Diagnostics;
using Helper.Objects;
using LegoLib;
using JigLibX.Physics;

namespace LegoGame
{
    // Wiki: https://github.com/colbybhearn/3DPhysics/wiki
    public class LegoGame : BaseGame
    {

        #region Properties / Fields

        GameplayModes gameplaymode = GameplayModes.Build;

        Model roverModel, wheelModel, landerModel;
        RoverObject myRover;
        Model terrainModel;
        Texture2D moon;
        Model cubeModel;
        Model sphereModel;
        Model roverRadar;
        Model RotArm;
        Model roverCam;
        Model Pole;

        Texture2D radar;
        Texture2D radar_icon;
        Texture2D laser_icon;
        Texture2D energy;
        Texture2D texLegoDot;

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

        #region Lego!
        Piece buildPiece;
        List<Piece> selectedPieces = new List<Piece>();
        #endregion


        #region Enumerations
        public enum InputGroups
        {
            Communication,
            Rover,
            Lander,
            Interface,
            Build,
            Play,
        }
        public enum AssetTypes
        {
            B_4_2_3, // Brick 4L x 2W x 3H (height is in slim increments)
            Radar1Pickup,
            Laser1Pickup,
            Lander,
            Rover,
        }
        public enum GameplayModes
        {
            Build,
            BuildCam,
            Play,
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

        public enum BuildModes
        {
            Orientation,
            Location,
            Length,            
            Height
        }
        #endregion

        #region Initialization
        public LegoGame(bool server)
            : base(server)
        {
            name = "Lego Game";


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
                texLegoDot = Content.Load<Texture2D>("LegoDot");
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
                assetManager.AddAsset(AssetTypes.B_4_2_3, CreateB_4_2_3);
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
            {
                SpawnPickups();
                SpawnCircleHouse();
                SpawnRectangleHouse();
            }
        }
        public override void InitializeEnvironment()
        {            
            try
            {
                terrain = new Terrain(new Vector3(0, -15, 0), // position
                    //new Vector3(100f, .1f, 100f),  // X with, possible y range, Z depth 
                                        new Vector3(15000f, .55f, 15000f),  // X with, possible y range, Z depth 
                                        100, 100, graphicsDevice, texLegoDot);
                objectsToAdd.Add(terrain.ID, terrain);
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }
        public override void InitializeCameras()
        {
            cameraManager.AddCamera((int)CameraModes.FreeLook, new FreeCamera());
            cameraManager.AddCamera((int)CameraModes.RoverFirstPerson, new BaseCamera());
            cameraManager.currentCamera.TargetPosition = new Vector3(10, 5, 20);
            cameraManager.currentCamera.Orientation = Quaternion.CreateFromYawPitchRoll((float)Math.PI, 0, 0);
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
            if (isClient)
            {
            }
            inputManager = new InputManager(this.name, GetDefaultControls());
            inputManager.AddInputMode(InputMode.Chat, (ChatDelegate)ChatCallback);
            UpdateInputs();
        }
        public override KeyMapCollection GetDefaultControls()
        {
            KeyMapCollection defControls = new KeyMapCollection();
            defControls.Game = this.name;


            List<KeyBinding> buildDefaults = new List<KeyBinding>();
            buildDefaults.Add(new KeyBinding("Forward", Keys.NumPad8, false, false, false, KeyEvent.Down, CameraMoveForward));
            buildDefaults.Add(new KeyBinding("Left", Keys.NumPad4, false, false, false, KeyEvent.Down, CameraMoveLeft));
            buildDefaults.Add(new KeyBinding("Backward", Keys.NumPad5, false, false, false, KeyEvent.Down, CameraMoveBackward));
            buildDefaults.Add(new KeyBinding("Right", Keys.NumPad6, false, false, false, KeyEvent.Down, CameraMoveRight));
            buildDefaults.Add(new KeyBinding("Speed Increase", Keys.NumPad7, false, false, false, KeyEvent.Pressed, CameraMoveSpeedIncrease));
            buildDefaults.Add(new KeyBinding("Speed Decrease", Keys.NumPad1, false, false, false, KeyEvent.Pressed, CameraMoveSpeedDecrease));
            buildDefaults.Add(new KeyBinding("Height Increase", Keys.NumPad9, false, false, false, KeyEvent.Down, CameraMoveHeightIncrease));
            buildDefaults.Add(new KeyBinding("Height Decrease", Keys.NumPad3, false, false, false, KeyEvent.Down, CameraMoveHeightDecrease));

            buildDefaults.Add(new KeyBinding("Cycle Mode", Keys.Decimal, false, false, false, KeyEvent.Pressed, CameraModeCycle));
            buildDefaults.Add(new KeyBinding("Home", Keys.Multiply, false, false, false, KeyEvent.Pressed, CameraMoveHome));
            //
            buildDefaults.Add(new KeyBinding("Toggle Debug Info", Keys.F1, false, false, false, KeyEvent.Pressed, ToggleDebugInfo));
            buildDefaults.Add(new KeyBinding("Toggle Physics Debug", Keys.F2, false, false, false, KeyEvent.Pressed, TogglePhsyicsDebug));
            

            KeyMap buildControls = new KeyMap(InputGroups.Build.ToString(), buildDefaults);



            List<KeyBinding> ClientDefs = new List<KeyBinding>();
            ClientDefs.Add(new KeyBinding("Escape", Keys.Escape, false, false, false, KeyEvent.Pressed, Stop));
            KeyMap clientControls = new KeyMap(GenericInputGroups.Client.ToString(), ClientDefs);


            
            // Chat
            List<KeyBinding> commDefaults = new List<KeyBinding>();
            commDefaults.Add(new KeyBinding("Chat ", Keys.Enter, false, false, false, KeyEvent.Pressed, ChatKeyPressed));
            KeyMap commControls = new KeyMap(InputGroups.Communication.ToString(), commDefaults);

            // Interface
            List<KeyBinding> interfaceDefaults = new List<KeyBinding>();
            interfaceDefaults.Add(new KeyBinding("Pause Physics", Keys.P, false, true, false, KeyEvent.Pressed, Pause));
            interfaceDefaults.Add(new KeyBinding("Toggle Build / Play Mode", Keys.F5, false, false, false, KeyEvent.Pressed, ToggleMode));
            interfaceDefaults.Add(new KeyBinding("Toggle Build Camera", Keys.C, false, false, false, KeyEvent.Pressed, buildCamToggle));
            KeyMap interfaceControls = new KeyMap(InputGroups.Interface.ToString(), interfaceDefaults);


            defControls.AddMap(buildControls);
            defControls.AddMap(clientControls);
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
        }

        public void CameraModeCycle()
        {
            cameraManager.NextCamera();
        }

        bool paused = false;
        private void Pause()
        {
            paused = !paused;

            if(paused)
                physicsManager.SetSimFactor(0);
            else
                physicsManager.SetSimFactor(1);
        }

        #region Assets
        // Standard and Static callback create methods for the asset manager

        private Gobject CreateB_4_2_3()
        {
            Vector3 size = new Vector3(1, 1, 2);
            // position of box was upper leftmost corner
            // body has world position
            // skin is relative to the body
            Box boxPrimitive = new Box(-.5f * size, Matrix.Identity, size); // relative to the body, the position is the top left-ish corner instead of the center, so subtract from the center, half of all sides to get that point.

            Gobject brick = new Gobject(
                Vector3.Zero, // position can be setup just following call to assetManager.GetNewInstance
                size / 2,
                boxPrimitive,
                MaterialTable.MaterialID.NotBouncyRough,
                cubeModel,
                0 // asset name is set automatically by asset manager when requested
                );
            return brick;
                
        }
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
                    driveTorque, /*physicsManager.PhysicsSystem.Gravity.Length());*/ 10f);
                // TODO FIX - Jeffrey changed gravity to magic constant because planets have their own gravity ... thus PhysicsSystem.Gravity = 0;
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

        private void SpawnRectangleHouse()
        {
            Vector3 HouseLocation = new Vector3(0, 0, 0);
            int LongWallLengthBrickCount = 4;
            int ShortWallLengthBrickCount = 2;
            float blockLength = 2.0f;
            int rows = 2;
            float blockHeight = 1f;
            Vector3 WallPoint = new Vector3(0,0,0);
            int count = 0;
            
            for (int y = 0; y < rows; y++)
            {
                WallPoint.Z = 0;
                if (y % 2 == 0)
                {
                    WallPoint.X -= blockLength / 4.0f;
                    
                    count = LongWallLengthBrickCount;
                }
                else
                {
                    WallPoint.X += blockLength / 4.0f;
                    count = LongWallLengthBrickCount;
                }

                // increase x
                for (int b = 0; b < count; b++)
                {
                    Vector3 brickLocation = new Vector3(WallPoint.X, (y*blockHeight)-10.5f, WallPoint.Z );
                    Vector3 brickOrientYPR = new Vector3((float)Math.PI/2.0f, 0, 0);
                    Gobject brick = GetB_4_2_3(HouseLocation + brickLocation, brickOrientYPR);
                    WallPoint.X += blockLength;
                    physicsManager.AddNewObject(brick);
                }

                if (y % 2 == 0)
                {
                    WallPoint.Z -= 0;
                    count = ShortWallLengthBrickCount;
                }
                else
                {
                    WallPoint.Z -= blockLength / 2.0f;
                    count = ShortWallLengthBrickCount;
                }

                // increase Z
                for (int b = 0; b < count; b++)
                {
                    Vector3 brickLocation = new Vector3(WallPoint.X, (y * blockHeight)-10.5f, WallPoint.Z);
                    Vector3 brickOrientYPR = new Vector3(0, 0, 0);
                    Gobject brick = GetB_4_2_3(HouseLocation + brickLocation, brickOrientYPR);
                    WallPoint.Z += blockLength;
                    physicsManager.AddNewObject(brick);
                }

                if (y % 2 == 0)
                {
                    WallPoint.X -= 0;
                    count = LongWallLengthBrickCount;
                }
                else
                {
                    WallPoint.X -= blockLength / 2.0f;
                    count = LongWallLengthBrickCount;
                }

                // decrease x
                for (int b = 0; b < count; b++)
                {
                    Vector3 brickLocation = new Vector3(WallPoint.X, (y * blockHeight) - 10.5f, WallPoint.Z);
                    Vector3 brickOrientYPR = new Vector3((float)Math.PI / 2.0f, 0, 0);
                    Gobject brick = GetB_4_2_3(HouseLocation + brickLocation, brickOrientYPR);
                    WallPoint.X -= blockLength;
                    physicsManager.AddNewObject(brick);
                }


                if (y % 2 == 0)
                {
                    WallPoint.Z -= 0;
                    count = ShortWallLengthBrickCount;
                }
                else
                {
                    WallPoint.Z -= blockLength / 2.0f;
                    count = ShortWallLengthBrickCount ;
                }

                // decrease Z
                for (int b = 0; b < count; b++)
                {
                    Vector3 brickLocation = new Vector3(WallPoint.X, (y * blockHeight) - 10.5f, WallPoint.Z);
                    Vector3 brickOrientYPR = new Vector3(0, 0, 0);
                    Gobject brick = GetB_4_2_3(HouseLocation + brickLocation, brickOrientYPR);
                    WallPoint.Z -= blockLength;
                    physicsManager.AddNewObject(brick);
                }
            }
        }

        private void SpawnCircleHouse()
        {
            Vector3 HouseLocation = new Vector3(40,0,0);

            int circumferenceBlockCount = 20;
            float blockLength = 1.12f;
            float circumference = circumferenceBlockCount* blockLength;
            float radius = circumference / (float)Math.PI;
            float circPoint = 0;
            float intervals = (float)Math.PI * 2.0f / (float)circumferenceBlockCount;
            int rows = 5;
            float blockHeight = .1f;
            
            for (int y = 0; y < rows; y++)
            {
                if (y % 2 == 0)
                    circPoint = 0;
                else
                    circPoint = intervals / 2.0f;

                for (int c = 0; c < circumferenceBlockCount; c++)
                {
                    Vector3 brickLocation = new Vector3((float)Math.Cos(circPoint), (blockHeight*y)-1.5f, (float)Math.Sin(circPoint));
                    Vector3 brickOrientYPR = new Vector3((float)(-circPoint), 0, 0);
                    Gobject brick = GetB_4_2_3(HouseLocation + (radius * brickLocation), brickOrientYPR);
                    circPoint += intervals;
                    physicsManager.AddNewObject(brick);
                }
            }
        }
        // Flexible create methods for the game 
        private Gobject GetB_4_2_3(Vector3 pos, Vector3 orientYPR)
        {
            Gobject brick = assetManager.GetNewInstance(AssetTypes.B_4_2_3);
            brick.MoveTo(pos, Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(orientYPR.X, orientYPR.Y, orientYPR.Z)));
            return brick;
        }
        private RoverObject GetRover(Vector3 pos)
        {
            // Colby says: Assets.Rover exists only as an experiment to decide between using Enums with casting or public static int variables with manual (possibly automated with reflection) initialization
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


        private void buildCamToggle()
        {
            switch (gameplaymode)
            {
                case GameplayModes.Build:
                    gameplaymode = GameplayModes.BuildCam;
                    break;
                case GameplayModes.BuildCam:
                    gameplaymode = GameplayModes.Build;

                    break;
            }
        }

        private void ToggleMode()
        {

            switch (gameplaymode)
            {
                case GameplayModes.Build:
                case GameplayModes.BuildCam:
                    gameplaymode = GameplayModes.Play;
                    break;
                case GameplayModes.Play:
                    gameplaymode = GameplayModes.Build;
                    PickPiece(AssetTypes.B_4_2_3);
                    break;
            }

            ProcessModeChange();
            UpdateInputs();
        }

        private void ProcessModeChange()
        {
            switch (gameplaymode)
            {
                case GameplayModes.Build:
                    SetSimFactor(0);
                    break;
                case GameplayModes.Play:
                    SetSimFactor(1);
                    break;
            }
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
                case GameplayModes.Build:
                    inputManager.EnableKeyMap(InputGroups.Build.ToString());
                    break;
                case GameplayModes.Play:
                    inputManager.EnableKeyMap(InputGroups.Play.ToString());
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
            if (newobject == null)
            {
                return;
            }

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
            if (commClient != null)
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
            Debug.WriteLine("Process Object Added: owner:" + ownerid + " id:" + objectid + " asset:" + asset);
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
            int pickups = 10;

            for (int i = 0; i < pickups; i++)
            {
                x = (float)(r.NextDouble() - .5);
                z = (float)(r.NextDouble() - .5);

                x = x * 250;
                z = z * 250;

                Gobject box = GetLaserPickup(new Vector3(x, 3.0f, z));
                physicsManager.AddNewObject(box);
            }


            for (int i = 0; i < pickups; i++)
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
                cameraManager.currentCamera.SetCurrentOrientation(myRover.GetCameraOrientation());
            }
        }
        private Gobject SpawnRover(int ownerid, int objectid)
        {
            Gobject newobject = GetRover(new Vector3(90, 20, 20));
            newobject.ID = objectid;
            physicsManager.AddNewObject(newobject);
            Debug.WriteLine("Selecting: owner" + ownerid + " mine" + MyClientID);
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

            if (rover == null || obj == null)
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

        #region Mouse Input
        float WindowLocationX;
        float WindowLocationY;
        /*
         * In build mode, the screen is your build area? is there some other, better way?
         * In build cam, we don't show the mouse and we auto-center the mouse.
         * 
         */
        public override void ProcessMouseMove(Point dPos, System.Windows.Forms.MouseEventArgs e, System.Drawing.Rectangle bounds)
        {
            switch (gameplaymode)
            {
                case GameplayModes.Build:
                    UpdateMouseLocationIn2DWindow(dPos);
                    BuildMouseMove(dPos, e, bounds);                    
                    break;
                case GameplayModes.BuildCam:
                    BuildCamMouseMove(dPos);
                    break;
                case GameplayModes.Play:
                    PlayMouseMove(e);
                    break;
                default:
                    break;
            }
        }

        private void UpdateMouseLocationIn2DWindow(Point dPos)
        {
            WindowLocationX += dPos.X;
            WindowLocationY += dPos.Y;
        }

        private void PlayMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        private void BuildCamMouseMove(Point d)
        {
            PanCam(d.X, d.Y);
        }

        
        private void BuildMouseMove(Point dPos, System.Windows.Forms.MouseEventArgs e,  System.Drawing.Rectangle bounds)
        {
            switch (gameplaymode)
            {
                case GameplayModes.Build:
                    BuildMouseDown(dPos, e, bounds);
                    break;
                case GameplayModes.BuildCam:
                    break;
                case GameplayModes.Play:
                    PlayMouseDown(e, bounds);
                    break;
                default:
                    break;
            }
        }

        public void PanCam(float dX, float dY)
        {
            cameraManager.AdjustTargetOrientationBy(dY*.001f, dX*.001f);
        }

        
        private void BuildMouseDown(Point dPos, System.Windows.Forms.MouseEventArgs e, System.Drawing.Rectangle bounds)
        {
            try
            {
                Viewport view = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                Vector2 mouse = new Vector2(WindowLocationX, WindowLocationY);
                Microsoft.Xna.Framework.Ray r = cameraManager.currentCamera.GetMouseRay(mouse, view);
                float dist = 0;
                Vector3 pos;
                Vector3 norm;
                CollisionSkin cs = new CollisionSkin();

                if (GetClickedPoint(out dist, out  cs, out pos, out norm, new Segment(r.Position, r.Direction * 1000)))
                {
                    buildLastMouseDownPosition = pos;
                    
                }
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        private void PlayMouseDown(System.Windows.Forms.MouseEventArgs e, System.Drawing.Rectangle bounds)
        {
            try
            {
                Viewport view = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                Vector2 mouse = new Vector2(e.Location.X, e.Location.Y);
                Microsoft.Xna.Framework.Ray r = cameraManager.currentCamera.GetMouseRay(mouse, view);
                float dist = 0;
                Vector3 pos;
                Vector3 norm;
                CollisionSkin cs = new CollisionSkin();

                if (GetClickedPoint(out dist,out  cs,out pos, out norm, new Segment(r.Position, r.Direction * 1000)))
                {
                    Body b = cs.Owner;
                    if (b == null)
                        return;
                    Gobject go = b.ExternalData as Gobject;
                    SelectGameObject(go);
                }
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        private bool GetClickedPoint(float x, float y, System.Drawing.Rectangle bounds, out float dist, out CollisionSkin cs, out Vector3 pos, out Vector3 norm)
        {
            Viewport view = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            Vector2 mouse = new Vector2(x, y);
            Microsoft.Xna.Framework.Ray r = cameraManager.currentCamera.GetMouseRay(mouse, view);
            return GetClickedPoint(out dist, out  cs, out pos, out norm, new Segment(r.Position, r.Direction * 1000));
        }

        private bool GetClickedPoint(out float dist, out CollisionSkin cs, out Vector3 pos, out Vector3 norm, Segment s)
        {
            // only ray tracing currently done is for object selection.
            // I need object intersection point.
            lock (physicsManager.PhysicsSystem)
            {
                if (physicsManager.PhysicsSystem.CollisionSystem.SegmentIntersect(out dist, out cs, out pos, out norm, s, new Helper.Physics.DefaultCollisionPredicate()))
                {
                    return true;
                }
            }
            return false;
        }


        #endregion

        #region Build Mode
        /*
         * What are we building?
         * what is the location?
         * what is the orientation?
         * 
         * What different things need to be specified for an object
         *  - Anything using two connection points has only 4 orientations
         * What is the length/height
         * 
         */
        BuildModes buildMode = BuildModes.Location;
        Vector3 buildLastMouseDownPosition;        
        float buildOrientation;
        
        Model currentPiece;

        private Model GetAssetModel(AssetTypes a)
        {
            return assetManager.GetModel((int) a);
        }

        private void PickPiece(AssetTypes p)
        {
            currentPiece = GetAssetModel(p);
        }

        #endregion


        #region Graphics

        
        private void DrawBuildMode(Matrix v, Matrix p)
        {
            if (currentPiece == null)
                return;
            Matrix world = Matrix.CreateTranslation(buildLastMouseDownPosition);
            currentPiece.Draw(world, v, p);
        }

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

            Matrix v = cameraManager.currentCamera.GetViewMatrix();
            Matrix p =cameraManager.ProjectionMatrix();
            

            //Now that we're no longer blocking, lets draw
            for (int i = 0; i < pos.Count; i++)
            {
                // TODO - magic number
                if (Vector3.Distance(cameraManager.currentCamera.CurrentPosition, pos[i]) < 100)
                {
                    Vector3 screen = sb.GraphicsDevice.Viewport.Project(pos[i], p, v, Matrix.Identity);

                    int size = (int)chatFont.MeasureString(text[i]).X;
                    sb.Draw(BlankBackground, new Microsoft.Xna.Framework.Rectangle((int)screen.X - size / 2, (int)screen.Y, size, chatFont.LineSpacing), Color.Gray * .5f);
                    sb.DrawString(chatFont, text[i], new Vector2(screen.X - size / 2, screen.Y), Color.White);
                }
            }

            DrawBuildMode(v,p);

            ChatManager.Draw(sb);

            if (isClient) // only clients need sound
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
                    catch (Exception x)
                    {
                        string tacos = x.Message;
                    }

                    // The radar map                
                    if (radar != null)
                        sb.Draw(radar, new Microsoft.Xna.Framework.Rectangle(10, 900, 100, 100), Color.White);

                    // The Icon
                    if (radar_icon != null)
                        sb.Draw(radar_icon, new Microsoft.Xna.Framework.Rectangle(10, 10, 50, 50), Color.White);
                }

                if (myRover.hasLaser)
                {
                    // The Icon
                    if (laser_icon != null)
                        sb.Draw(laser_icon, new Microsoft.Xna.Framework.Rectangle(70, 10, 50, 50), Color.White);
                }
            }

        }

        #endregion

        #endregion

    }
}
