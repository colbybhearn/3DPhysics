using System.Collections.Generic;
using Helper;
using Helper.Input;
using Helper.Physics;
using Helper.Physics.PhysicsObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper.Lighting;
using System;
using Helper.Camera;
using Game;
using JigLibX.Collision;


namespace RoboGame
{
    // Wiki: https://github.com/colbybhearn/3DPhysics/wiki
    public class RoboGame : BaseGame
    {
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
            
            ChatManager = new Chat(chatFont);
            ChatMessageReceived += new Helper.Handlers.ChatMessageEH(ChatManager.ReceiveMessage);
        }

        public override void InitializeMultiplayer()
        {
            base.InitializeMultiplayer();

            if(isClient)
            {
                //commClient.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commClient_ObjectUpdateReceived);
            }
            else if(isServer)
            {
                commServer.ObjectRequestReceived += new Helper.Handlers.ObjectRequestEH(commServer_ObjectRequestReceived);
            }
        }
        
        void commServer_ObjectRequestReceived(int clientId, string asset)
        {
            ProcessObjectRequest(clientId, asset);
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
                    newObjects.Add(heightmapObj.ID, heightmapObj);
                }
                catch (Exception E)
                {
                    // if that happens just create a ground plane 
                    //planeObj = new PlaneObject(planeModel, 0.0f, new Vector3(0, -15, 0), "");
                    //newObjects.Add(planeObj.ID, planeObj);
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
                }
            }

            //SpawnRover(0, 1);
            SpawnPickups();
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
            interfaceDefaults.Add(new KeyBinding("Enter / Exit Vehicle", Keys.E, false, true, false, KeyEvent.Pressed, EnterExitVehicle));
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
        
        public enum SpecificInputGroups
        {
            Communication,
            Rover,
            Lander,
            Interface,
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
        /// 
        /// 
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
        }

        /// <summary>
        /// CLIENT SIDE
        /// client should do something oriented to the specific game here, like player bullets or cars.
        /// The server has granted the object request and this is where we handle the response it has sent back to the client
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
                box.ID = 1000 + i;                
                physicsManager.AddNewObject(box);
            }


            for (int i = 0; i < 10; i++)
            {
                x = (float)(r.NextDouble() - .5);
                z = (float)(r.NextDouble() - .5);

                x = x * 250;
                z = z * 250;

                Gobject sphere = physicsManager.GetSphere(new Vector3(x, 3.0f, z), 0.4f, sphereModel, true);                
                sphere.ID = 2000 + i;
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
            myRover.setLaser(1.0f);
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

            foreach(Gobject go in gameObjects.Values)
            {
                if (go.Skin.Equals(skin1))
                {
                    obj = go;
                }
                else if(go.Skin.Equals(skin0))
                {
                    rover = go as RoverObject;
                }
            }

            if(rover==null || obj == null)
                return true;

            string type = obj.Asset.ToLower();
            if (type == "cube")
            {
                rover.AddLaser();
                gameObjects.Remove(obj.ID);
                return false;
            }
            if (type == "sphere")
            {
                rover.AddRadar();
                gameObjects.Remove(obj.ID);
                return false;
            }
            return true;
        }
        public override List<ViewProfile> GetViewProfiles()
        {
            // TODO: Why is this not working as expected?
            // Capitalization of asset name?
            // CameraManager ViewProfiles for selected Gobjects?
            List<ViewProfile> profiles = base.GetViewProfiles();
            profiles.Add(new ViewProfile(GenericCameraModes.ObjectFirstPerson.ToString(),
                "rover2", new Vector3(-.45f, 1.4f, .05f), .25f, new Vector3(0, (float)-Math.PI/2.0f, 0), 1.0f));
            return profiles;
        }

        private void SpawnLander()
        {
            if (commClient != null)
            {
                commClient.SendObjectRequest("lunar lander");
            }
        }
        private void SpawnSpheres()
        {
            physicsManager.AddSpheres(5, sphereModel);
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

        Texture2D BlankBackground;
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

            if (myRover.hasRadar)
            {/*
                // The radar map
                Texture2D radar = new Texture2D(sb.GraphicsDevice, 1, 1);
                radar.SetData(new Color[] { Color.White });
                radar = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead(@"C:\Users\Kernel\Documents\GitHub\3DPhysics\Content\radar.png"));
                sb.Draw(radar, new Rectangle(10, 900, 100, 100), Color.White);

                // The Icon
                Texture2D radarIcon = new Texture2D(sb.GraphicsDevice, 1, 1);
                radarIcon.SetData(new Color[] { Color.White });
                radarIcon = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead(@"C:\Users\Kernel\Documents\GitHub\3DPhysics\Content\radar_icon.png"));
                sb.Draw(radarIcon, new Rectangle(10, 10, 50, 50), Color.White);*/
            }

            if (myRover.hasLaser)
            {/*
                // The Icon
                Texture2D LaserIcon = new Texture2D(sb.GraphicsDevice, 1, 1);
                LaserIcon.SetData(new Color[] { Color.White });
                LaserIcon = Texture2D.FromStream(sb.GraphicsDevice, File.OpenRead(@"C:\Users\Kernel\Documents\GitHub\3DPhysics\Content\laser_icon.png"));
                sb.Draw(LaserIcon, new Rectangle(70, 10, 50, 50), Color.White);*/
            }

        }
    }
}
