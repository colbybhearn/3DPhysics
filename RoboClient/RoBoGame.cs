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


namespace RoboGame
{
    // Wiki: https://github.com/colbybhearn/3DPhysics/wiki
    public class RoboGame : BaseGame
    {

        public enum GameplayModes
        {
            Car,
            Lander,
            Aircraft,
            Spectate,
        }

        Texture2D wallTexture;
        Model carModel, wheelModel, landerModel;
        CarObject myCar;
        Model terrainModel;
        Model planeModel;
        Texture2D moon;
        Model cubeModel;
        Model sphereModel;
        LunarVehicle lander;
        Model airplane;

        public RoboGame()            
        {
            name = "RoBo Game";
        }

        public override void InitializeContent()
        {
            base.InitializeContent();
            cubeModel = Content.Load<Model>("Cube");
            sphereModel = Content.Load<Model>("Sphere");
            carModel = Content.Load<Model>("car");
            wheelModel = Content.Load<Model>("wheel");
            moon = Content.Load<Texture2D>("Moon");
            planeModel = Content.Load<Model>("plane");
            terrainModel = Content.Load<Model>("terrain");
            carModel = Content.Load<Model>("car");
            wheelModel = Content.Load<Model>("wheel");
            landerModel = Content.Load<Model>("Lunar Lander");
            airplane = Content.Load<Model>("Airplane");
            chatFont = Content.Load<SpriteFont>("debugFont");
            
            ChatManager = new Chat(chatFont);
            ChatMessageReceived += new Helper.Handlers.ChatMessageEH(ChatManager.ReceiveMessage);
            
            SetUpVertices(this.graphicsDevice);
            try
            {
                // this is where Colby is, current: http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series3/Pixel_shader.php 
                lighteffect = Content.Load<Effect>(@"Lighting\LightEffect");
                wallTexture = Content.Load<Texture2D>("metal3_scr");
                
            }
            catch (Exception E)
            {
            }

            
            //lighteffect.Parameters["xViewProjection"].SetValue(cameraManager.currentCamera.GetViewMatrix() * cameraManager.currentCamera.GetProjectionMatrix());

        }
        PointLight firstLight;

        public override void InitializeMultiplayer(BaseGame.CommTypes CommType)
        {
            base.InitializeMultiplayer(CommType);

            switch (CommType)
            {
                case CommTypes.Client:
                    //commClient.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commClient_ObjectUpdateReceived);
                    break;
                case CommTypes.Server:
                    commServer.ObjectRequestReceived += new Helper.Handlers.ObjectRequestEH(commServer_ObjectRequestReceived);
                    break;

                default:
                    break;
            }
        }


        /*
        /// <summary>
        /// CLIENT SIDE
        /// Client has received an object update from the server
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asset"></param>
        /// <param name="pos"></param>
        /// <param name="orient"></param>
        /// <param name="vel"></param>
        void commClient_ObjectUpdateReceived(int id, string asset, Microsoft.Xna.Framework.Vector3 pos, Microsoft.Xna.Framework.Matrix orient, Microsoft.Xna.Framework.Vector3 vel)
        {
            ProcessObjectUpdate(id, asset, pos, orient, vel);
        }

        /// <summary>
        /// CLIENT SIDE
        /// Client should take the information from the server and use it here
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asset"></param>
        /// <param name="pos"></param>
        /// <param name="orient"></param>
        /// <param name="vel"></param>
        private void ProcessObjectUpdate(int id, string asset, Microsoft.Xna.Framework.Vector3 pos, Microsoft.Xna.Framework.Matrix orient, Microsoft.Xna.Framework.Vector3 vel)
        {

            physicsUpdateList.Add(new Helper.Multiplayer.Packets.ObjectUpdatePacket(id, asset, pos, orient, vel));
            //lock (gameObjects)
            //{

            //    if (!gameObjects.ContainsKey(id))
            //    {
            //        AddNewObject(id, asset); // which will only put it on newObjects;
            //    }
            //    if (newObjects.ContainsKey(id))
            //        return;
            //    Gobject go = gameObjects[id];
            //    //go.SetOrientation(orient);
            //    if (id == 1)
            //    {
            //    }
            //    go.MoveTo(pos, go.BodyOrientation());
            //    go.SetVelocity(vel);
            //}
        }*/

        void commServer_ObjectRequestReceived(int clientId, string asset)
        {
            ProcessObjectRequest(clientId, asset);
        }

        public override void InitializeEnvironment()
        {
            base.InitializeEnvironment();
            //SpawnCar(1, 1);
            firstLight = new PointLight();
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
            List<KeyBinding> carDefaults = new List<KeyBinding>();
            //careDefaults.Add(new KeyBinding("Spawn", Keys.R, false, true, false, KeyEvent.Pressed, SpawnCar));
            carDefaults.Add(new KeyBinding("Forward", Keys.Up, false, false, false, KeyEvent.Down, Accelerate));
            carDefaults.Add(new KeyBinding("Left", Keys.Left, false, false, false, KeyEvent.Down, SteerLeft));
            carDefaults.Add(new KeyBinding("Brake / Reverse", Keys.Down, false, false, false, KeyEvent.Down, Deccelerate));
            carDefaults.Add(new KeyBinding("Right", Keys.Right, false, false, false, KeyEvent.Down, SteerRight));
            carDefaults.Add(new KeyBinding("Handbrake", Keys.B, false, false, false, KeyEvent.Down, ApplyHandbrake));            
            KeyMap carControls = new KeyMap(SpecificInputGroups.Car.ToString(),carDefaults);

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

            
            // jet
            List<KeyBinding> jetDefaults = new List<KeyBinding>();
            //jetDefaults.Add(new KeyBinding("Spawn ", Keys.P, false, true, false, KeyEvent.Pressed, SpawnPlane));
            jetDefaults.Add(new KeyBinding("Increase Thrust", Keys.OemPlus, false, false, false, KeyEvent.Down, PlaneThrustIncrease));
            jetDefaults.Add(new KeyBinding("Decrease Thrust", Keys.OemMinus, false, false, false, KeyEvent.Down, PlaneThrustDecrease));
            jetDefaults.Add(new KeyBinding("Roll Left", Keys.H, false, false, false, KeyEvent.Down, PlaneRollLeft));
            jetDefaults.Add(new KeyBinding("Roll Right", Keys.K, false, false, false, KeyEvent.Down, PlaneRollRight));
            jetDefaults.Add(new KeyBinding("Pitch Up", Keys.J, false, false, false, KeyEvent.Down, PlanePitchUp));
            jetDefaults.Add(new KeyBinding("Pitch Down", Keys.J, false, false, false, KeyEvent.Down, PlanePitchDown));
            KeyMap jetControls = new KeyMap(SpecificInputGroups.Aircraft.ToString(), jetDefaults);
            
            // Chat
            List<KeyBinding> commDefaults = new List<KeyBinding>();
            commDefaults.Add(new KeyBinding("Chat ", Keys.Enter, false, false, false, KeyEvent.Pressed, ChatKeyPressed));
            KeyMap commControls = new KeyMap(SpecificInputGroups.Communication.ToString(), commDefaults);

            // Interface
            List<KeyBinding> interfaceDefaults = new List<KeyBinding>();
            interfaceDefaults.Add(new KeyBinding("Enter / Exit Vehicle", Keys.Enter, false, false, false, KeyEvent.Pressed, EnterVehicle));
            interfaceDefaults.Add(new KeyBinding("Spawn Lander", Keys.L, false, true, false, KeyEvent.Pressed, SpawnLander));
            interfaceDefaults.Add(new KeyBinding("Spawn Aircraft", Keys.P, false, true, false, KeyEvent.Pressed, Request_Plane));
            interfaceDefaults.Add(new KeyBinding("Spawn Car", Keys.R, false, true, false, KeyEvent.Pressed, Request_Car));
            KeyMap interfaceControls = new KeyMap(SpecificInputGroups.Interface.ToString(), interfaceDefaults);

            defControls.AddMap(carControls);
            defControls.AddMap(jetControls);
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
            Car,
            Aircraft,
            Zombie,
            Lander,
            Interface,

        }

        GameplayModes gameplaymode = GameplayModes.Spectate;

        private void EnterVehicle()
        {

            switch (gameplaymode)
            {
                case GameplayModes.Car:
                    gameplaymode = GameplayModes.Spectate;
                    break;
                case GameplayModes.Lander:
                    gameplaymode = GameplayModes.Spectate;
                    break;
                case GameplayModes.Aircraft:
                    gameplaymode = GameplayModes.Spectate;
                    break;
                case GameplayModes.Spectate:
                    if (currentSelectedObject == null)
                        return;
                    // turn on only those appropriate to the current Game mode
                    if (currentSelectedObject is CarObject)
                        gameplaymode = GameplayModes.Car;
                    if (currentSelectedObject is Aircraft)
                        gameplaymode = GameplayModes.Aircraft;
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
            inputManager.EnableKeyMap(GenericInputGroups.Camera.ToString());
            inputManager.EnableKeyMap(SpecificInputGroups.Communication.ToString());
            inputManager.EnableKeyMap(SpecificInputGroups.Interface.ToString());

            switch (gameplaymode)
            {
                case GameplayModes.Car:
                    inputManager.EnableKeyMap(SpecificInputGroups.Car.ToString());
                    break;
                case GameplayModes.Lander:
                    inputManager.EnableKeyMap(SpecificInputGroups.Lander.ToString());
                    break;
                case GameplayModes.Aircraft:
                    inputManager.EnableKeyMap(SpecificInputGroups.Aircraft.ToString());
                    break;
                case GameplayModes.Spectate:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// CLIENT SIDE
        /// When a client receives an object update for an object it does not know about, instantiate one!
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="asset"></param>
        public override void AddNewObject(int objectid, string asset)
        {
            Model model = Content.Load<Model>(asset);
            Gobject newobject = null;
            switch (asset.ToLower())
            {
                case "sphere":          newobject = physicsManager.GetDefaultSphere(model);         break;
                case "car":             newobject = physicsManager.GetCar(carModel, wheelModel);    break;
                case "lunar lander":    newobject = physicsManager.GetLunarLander(landerModel);     break;
                case "airplane":        newobject = physicsManager.GetAircraft(model);              break;
                default:                                                                            break;
            }
            
            newobject.ID = objectid;
            physicsManager.AddNewObject(newobject);
        }        

        /// <summary>
        /// CLIENT SIDE
        /// 
        /// </summary>
        private void Request_Car()
        {
            if(commClient!=null)
                // send a request to the server for an object of asset type "car"
                commClient.SendObjectRequest("car");
        }

        private void Request_Plane()
        {
            if (commClient != null)
                // send a request to the server for an object of asset type "car"
                commClient.SendObjectRequest("airplane");
        }

        

        Aircraft myPlane;
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
                case "sphere":
                    newobject = physicsManager.GetDefaultSphere(model);
                    newobject.ID = objectid;
                    physicsManager.AddNewObject(newobject);
                    break;
                case "car":
                    newobject = SpawnCar(ownerid, objectid);
                    break;
                case "lunar lander":
                    lander = physicsManager.GetLunarLander(landerModel);
                    lander.ID = objectid;
                    physicsManager.AddNewObject(lander);
                    if (ownerid == MyClientID) // Only select the new object if its OUR new object
                        SelectGameObject(lander);

                    break;
                case "airplane":
                    newobject = SpawnPlane(ownerid, objectid);
                    break;
                default:
                    break;
            }
        }

        private Gobject SpawnCar(int ownerid, int objectid)
        {
            Gobject newobject  = physicsManager.GetCar(carModel, wheelModel);
            newobject.ID = objectid;
            physicsManager.AddNewObject(newobject);
            if (ownerid == MyClientID) // Only select the new car if its OUR new car
            {
                myCar = (CarObject)newobject;
                SelectGameObject(myCar);
            }
            return newobject;
        }        
        private Gobject SpawnPlane(int ownerid, int objectid)
        {
            // test code for client-side aircraft/plane spawning
            Gobject newobject = physicsManager.GetAircraft(airplane);
            newobject.ID = gameObjects.Count;
            physicsManager.AddNewObject(newobject);
            if (ownerid == MyClientID) // Only select the new car if its OUR new car
            {
                myPlane = (Aircraft)newobject;
                SelectGameObject(myPlane);
            }
            return newobject;
        }
        private void SpawnLander()
        {
            if (commClient != null)
            {
                commClient.SendObjectRequest("lunar lander");
            }
        }
        
        #region Plane
        private void PlaneThrustIncrease()
        {
            if(myPlane==null)return;
            myPlane.AdjustThrust(.1f);
        }
        private void PlaneThrustDecrease()
        {
            if (myPlane == null) return;
            myPlane.AdjustThrust(-.1f);
        }
        private void PlaneRollLeft()
        {
            if (myPlane == null) return;
            myPlane.SetAilerons(-1f);
        }
        private void PlaneRollRight()
        {
            if (myPlane == null) return;
            myPlane.SetAilerons(1f);
        }
        private void PlanePitchUp()
        {
            if (myPlane == null) return;
            myPlane.SetElevator(1f);
        }
        private void PlanePitchDown()
        {
            if (myPlane == null) return;
            myPlane.SetElevator(-1f);
        }
        
        #endregion

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

        #region Car
        private void Accelerate()
        {
            if (myCar == null)
                return;
            myCar.SetAcceleration(1.0f);
        }
        private void Deccelerate()
        {
            if (myCar == null)
                return;
            myCar.SetAcceleration(-1.0f);
        }
        private void SteerLeft()
        {
            if (myCar == null)
                return;
            myCar.SetSteering(1.0f);
        }
        private void SteerRight()
        {
            if (myCar == null)
                return;
            myCar.SetSteering(-1.0f);
        }
        private void ApplyHandbrake()
        {
            if (myCar == null)
                return;
            myCar.setHandbrake(1.0f);
        }
        private void ShiftUp()
        {
            // shift from 1st to 2nd gear
        }
        private void ShiftDown()
        {
            // shift from 2nd to 1st gear
        }
        private void ChangeTransmissionType()
        {
            // manual vs. automatic
        }
        #endregion

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
        }
        
        VertexBuffer vertexBuffer;
        private void SetUpVertices(GraphicsDevice device)
        {
            TexturedVertexFormat[] vertices = new TexturedVertexFormat[18];

            vertices[0] = new TexturedVertexFormat(new Vector3(-20, .0f, 10), new Vector2(-0.25f, 25.0f), new Vector3(0, 1, 0));
            vertices[1] = new TexturedVertexFormat(new Vector3(-20, .0f, -100), new Vector2(-0.25f, 0.0f), new Vector3(0, 1, 0));
            vertices[2] = new TexturedVertexFormat(new Vector3(2, .0f, 10), new Vector2(0.25f, 25.0f), new Vector3(0, 1, 0));
            vertices[3] = new TexturedVertexFormat(new Vector3(2, .0f, -100), new Vector2(0.25f, 0.0f), new Vector3(0, 1, 0));
            vertices[4] = new TexturedVertexFormat(new Vector3(2, .0f, 10), new Vector2(0.25f, 25.0f), new Vector3(-1, 0, 0));
            vertices[5] = new TexturedVertexFormat(new Vector3(2, .0f, -100), new Vector2(0.25f, 0.0f), new Vector3(-1, 0, 0));
            vertices[6] = new TexturedVertexFormat(new Vector3(2, 1, 10), new Vector2(0.375f, 25.0f), new Vector3(-1, 0, 0));
            vertices[7] = new TexturedVertexFormat(new Vector3(2, 1, -100), new Vector2(0.375f, 0.0f), new Vector3(-1, 0, 0));
            vertices[8] = new TexturedVertexFormat(new Vector3(2, 1, 10), new Vector2(0.375f, 25.0f), new Vector3(0, 1, 0));
            vertices[9] = new TexturedVertexFormat(new Vector3(2, 1, -100), new Vector2(0.375f, 0.0f), new Vector3(0, 1, 0));
            vertices[10] = new TexturedVertexFormat(new Vector3(3, 1, 10), new Vector2(0.5f, 25.0f), new Vector3(0, 1, 0));
            vertices[11] = new TexturedVertexFormat(new Vector3(3, 1, -100), new Vector2(0.5f, 0.0f), new Vector3(0, 1, 0));
            vertices[12] = new TexturedVertexFormat(new Vector3(13, 1, 10), new Vector2(0.75f, 25.0f), new Vector3(0, 1, 0));
            vertices[13] = new TexturedVertexFormat(new Vector3(13, 1, -100), new Vector2(0.75f, 0.0f), new Vector3(0, 1, 0));
            vertices[14] = new TexturedVertexFormat(new Vector3(13, 1, 10), new Vector2(0.75f, 25.0f), new Vector3(-1, 0, 0));
            vertices[15] = new TexturedVertexFormat(new Vector3(13, 1, -100), new Vector2(0.75f, 0.0f), new Vector3(-1, 0, 0));
            vertices[16] = new TexturedVertexFormat(new Vector3(13, 21, 10), new Vector2(1.25f, 25.0f), new Vector3(-1, 0, 0));
            vertices[17] = new TexturedVertexFormat(new Vector3(13, 21, -100), new Vector2(1.25f, 0.0f), new Vector3(-1, 0, 0));
            
            for(int i=0;i<vertices.Length;i++)
                vertices[i].position.Y -= 14.9f;
            vertexBuffer = new VertexBuffer(device, TexturedVertexFormat.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
        }

        private void DrawLightTest(GraphicsDevice device)
        {
            if (cameraManager.currentCamera == null)
                return;
            lighteffect.CurrentTechnique = lighteffect.Techniques["Simplest"];
            Matrix vp = cameraManager.currentCamera.GetViewMatrix() * cameraManager.currentCamera.GetProjectionMatrix();
            lighteffect.Parameters["xViewProjection"].SetValue(vp);
            lighteffect.Parameters["xTexture"].SetValue(wallTexture);
            lighteffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            lighteffect.Parameters["xLightPos"].SetValue(firstLight.lightPos);
            lighteffect.Parameters["xLightPower"].SetValue(firstLight.lightPower);
            lighteffect.Parameters["xAmbient"].SetValue(firstLight.ambientPower);

            try
            {
                foreach (EffectPass pass in lighteffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    device.SetVertexBuffer(vertexBuffer);
                    device.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleStrip, 0, 16);
                }
            }
            catch (Exception E)
            {

            }
        }
                
    }
}
