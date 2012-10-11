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


namespace RoboGame
{
    // Wiki: https://github.com/colbybhearn/3DPhysics/wiki
    public class RoboGame : BaseGame
    {
        #region Properties / Fields
        public enum GameplayModes
        {
            Rover,
            Lander,
            Spectate,
        }
        GameplayModes gameplaymode = GameplayModes.Rover;

        Model roverModel, wheelModel, landerModel;
        RoverObject myRover;
        Model terrainModel;
        Texture2D moon;
        Model cubeModel;
        Model sphereModel;
        LunarVehicle lander;

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
        public enum SpecificInputGroups
        {
            Communication,
            Rover,
            Lander,
            Interface,
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
            cubeModel = Content.Load<Model>("Cube");
            sphereModel = Content.Load<Model>("Sphere");
            roverModel = Content.Load<Model>("car");
            wheelModel = Content.Load<Model>("wheel");
            moon = Content.Load<Texture2D>("Moon");
            terrainModel = Content.Load<Model>("terrain2");
            roverModel = Content.Load<Model>("Rover2");
            wheelModel = Content.Load<Model>("wheel");
            landerModel = Content.Load<Model>("Lunar Lander");
            chatFont = Content.Load<SpriteFont>("debugFont");

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

            motor_running = motor.CreateInstance();
            //motor_running.IsLooped = true; // should be looped, but need to know when rover stops to call .Stop();

            solar_wind_loop = solar_wind.CreateInstance();
            solar_wind_loop.Volume = 0.5f;
            solar_wind_loop.IsLooped = true;

            radar_noise_loop = radar_noise.CreateInstance();
            radar_noise_loop.Volume = 0.5f;
            radar_noise_loop.IsLooped = true;

            // Let play this right away;  should not play on the server through
            if(isClient)
                solar_wind_loop.Play();
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
                    //terrain = new Terrain(new Vector3(0, -15, 0), // position
                    //    //new Vector3(100f, .1f, 100f),  // X with, possible y range, Z depth 
                    //                        new Vector3(15000f, .55f, 15000f),  // X with, possible y range, Z depth 
                    //                        100, 100, graphicsDevice, moon);

                    //newObjects.Add(terrain.ID, terrain);
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

        public override List<ViewProfile> GetViewProfiles()
        {
            List<ViewProfile> profiles = base.GetViewProfiles();
            profiles.Add(new ViewProfile(GenericCameraModes.ObjectFirstPerson.ToString(),
                "rover2", new Vector3(-.45f, 1.4f, .05f), .25f, new Vector3(0, (float)-Math.PI / 2.0f, 0), 1.0f));
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
            
            // Car
            List<KeyBinding> roverDefaults = new List<KeyBinding>();
            //careDefaults.Add(new KeyBinding("Spawn", Keys.R, false, true, false, KeyEvent.Pressed, SpawnCar));
            roverDefaults.Add(new KeyBinding("Forward", Keys.Up, false, false, false, KeyEvent.Down, Accelerate));
            roverDefaults.Add(new KeyBinding("Left", Keys.Left, false, false, false, KeyEvent.Down, SteerLeft));
            roverDefaults.Add(new KeyBinding("Backward", Keys.Down, false, false, false, KeyEvent.Down, Deccelerate));
            roverDefaults.Add(new KeyBinding("Right", Keys.Right, false, false, false, KeyEvent.Down, SteerRight));
            roverDefaults.Add(new KeyBinding("Laser", Keys.B, false, false, false, KeyEvent.Down, UseLaser));            
            KeyMap roverControls = new KeyMap(SpecificInputGroups.Rover.ToString(),roverDefaults);

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
            KeyMap landerControls = new KeyMap(SpecificInputGroups.Lander.ToString(), landerDefaults);
            
            // Chat
            List<KeyBinding> commDefaults = new List<KeyBinding>();
            commDefaults.Add(new KeyBinding("Chat ", Keys.Enter, false, false, false, KeyEvent.Pressed, ChatKeyPressed));
            KeyMap commControls = new KeyMap(SpecificInputGroups.Communication.ToString(), commDefaults);

            // Interface
            List<KeyBinding> interfaceDefaults = new List<KeyBinding>();
            //interfaceDefaults.Add(new KeyBinding("Enter / Exit Vehicle", Keys.E, false, true, false, KeyEvent.Pressed, EnterExitVehicle));
            interfaceDefaults.Add(new KeyBinding("Spawn Lander", Keys.L, false, true, false, KeyEvent.Pressed, SpawnLander));
            interfaceDefaults.Add(new KeyBinding("Spawn Rover", Keys.R, false, true, false, KeyEvent.Pressed, Request_Rover));
            KeyMap interfaceControls = new KeyMap(SpecificInputGroups.Interface.ToString(), interfaceDefaults);
            

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
        #endregion

        #region Methods

        public override void Stop()
        {
            base.Stop();
            radar_noise_loop.Stop();
            solar_wind_loop.Stop();
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
            inputManager.EnableKeyMap(SpecificInputGroups.Communication.ToString());
            inputManager.EnableKeyMap(SpecificInputGroups.Interface.ToString());

            switch (gameplaymode)
            {
                case GameplayModes.Rover:
                    inputManager.EnableKeyMap(SpecificInputGroups.Rover.ToString());
                    break;
                case GameplayModes.Lander:
                    inputManager.EnableKeyMap(SpecificInputGroups.Lander.ToString());
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
        public override void AddNewObject(int objectid, string asset)
        {
            if (Content == null)
                return;
            Model model = null;
            try
            {
                model = Content.Load<Model>(asset);
            }
            catch (Exception E)
            {

            }
            if (model == null)
            {
                return;
            }

            
            Gobject newobject = null;
            switch (asset.ToLower())
            {
                case "cube":            newobject = physicsManager.GetBox(model);                   break;
                case "sphere":          newobject = physicsManager.GetDefaultSphere(model);         break;
                case "rover2":
                    newobject = physicsManager.GetRover(roverModel, wheelModel, sphereModel, cubeModel);
                    newobject.AddCollisionCallback(CollisionSkin_callbackFn);
                    break;
                case "lunar lander":    newobject = physicsManager.GetLunarLander(landerModel);     break;
                default:                                                                            break;
            }
            
            newobject.ID = objectid;
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
                commClient.SendObjectRequest("rover2");

            spawn.Play();
        }

        /// <summary>
        /// CLIENT SIDE
        /// client should do something oriented to the specific game here, like player bullets or cars.
        /// The server has granted the object request and this is where the client handle the response the server has sent back 
        /// This is called from the Network code, thus in the Network threads
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="asset"></param>
        public override void ProcessObjectAdded(int ownerid, int objectid, string asset)
        {
            Model model = Content.Load<Model>(asset);
            Gobject newobject = null;
            switch (asset.ToLower())
            {
                case "cube":
                    newobject = physicsManager.GetBox(model);
                    newobject.ID = objectid;
                    physicsManager.AddNewObject(newobject);
                    break;
                case "sphere":
                    newobject = physicsManager.GetDefaultSphere(model);
                    newobject.ID = objectid;
                    physicsManager.AddNewObject(newobject);
                    break;
                case "rover2":
                    newobject = SpawnRover(ownerid, objectid);
                    break;
                case "lunar lander":
                    lander = physicsManager.GetLunarLander(landerModel);
                    lander.ID = objectid;
                    physicsManager.AddNewObject(lander);
                    if (ownerid == MyClientID) // Only select the new object if its OUR new object
                        SelectGameObject(lander);
                    break;
                default:
                    break;
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

                //Gobject sphere = physicsManager.GetSphere(new Vector3(x, 3.0f, z), 1.0f, sphereModel, true);
                Gobject box = physicsManager.GetBoxHighFriction(new Vector3(x, 3.0f, z), new Vector3(1.0f, 1.0f, 1.0f), Matrix.Identity, cubeModel, true);
                box.ID = GetAvailableObjectId();  
                
                physicsManager.AddNewObject(box);
            }


            for (int i = 0; i < 10; i++)
            {
                x = (float)(r.NextDouble() - .5);
                z = (float)(r.NextDouble() - .5);

                x = x * 250;
                z = z * 250;

                Gobject sphere = physicsManager.GetSphere(new Vector3(x, 3.0f, z), 0.4f, sphereModel, true);
                sphere.ID = GetAvailableObjectId();
                physicsManager.AddNewObject(sphere);
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
        #endregion

        private Gobject SpawnRover(int ownerid, int objectid)
        {
            Gobject newobject = physicsManager.GetRover(roverModel, wheelModel, sphereModel,cubeModel);
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

            string type = obj.Asset.ToLower();
            if (type == "cube")
            {
                rover.SetLaser(true);
                DeleteObject(obj.ID);
                return false;
            }
            if (type == "sphere")
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
                commClient.SendObjectRequest("lunar lander");
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
                    Vector3 screen = sb.GraphicsDevice.Viewport.Project(pos[i], cameraManager.ProjectionMatrix(), cameraManager.currentCamera.RhsLevelViewMatrix, Matrix.Identity);
                    
                    int size = (int)chatFont.MeasureString(text[i]).X;
                    sb.Draw(BlankBackground, new Rectangle((int)screen.X - size/2, (int)screen.Y, size, chatFont.LineSpacing), Color.Gray * .5f);
                    sb.DrawString(chatFont, text[i], new Vector2(screen.X - size/2, screen.Y), Color.White);
                }
            }

            ChatManager.Draw(sb);
            //DrawLightTest(this.graphicsDevice);


            FrameworkDispatcher.Update(); // Sounds like this called early and often

            if (myRover != null)
            {            
                int nrg = (int)myRover.Energy;
                sb.Draw(energy, new Rectangle(5, 5, nrg, 5), Color.White);

                if (myRover.hasRadar)
                {
                    try
                    {
                        if (isClient)
                            if (radar_noise_loop != null)
                                radar_noise_loop.Play();                        
                    }
                    catch(Exception x)
                    {
                        string tacos = x.Message;
                    }

                    // The radar map                
                    if(radar!=null)                    
                        sb.Draw(radar, new Rectangle(10, 900, 100, 100), Color.White);

                    // The Icon
                    if(radar_icon != null)
                        sb.Draw(radar_icon, new Rectangle(10, 10, 50, 50), Color.White);
                }

                if (myRover.hasLaser)
                {
                    // The Icon
                    if(laser_icon!=null)
                        sb.Draw(laser_icon, new Rectangle(70, 10, 50, 50), Color.White);
                }
            }

        }
        #endregion

        #endregion

    }
}
