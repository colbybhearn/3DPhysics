using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
//using Input;
using Helper.Input;
using Helper.Physics.PhysicsObjects;
using Helper.Physics;
using Microsoft.Xna.Framework;
using System;

namespace Game
{

    /* Todo:
     * work on having different first person and chase cam views for different object types (car vs. lunar lander vs. aircraft)
     * Work on what the physicsManager has to know in ProcessObjectAdded. Right now, it has to know the specific physicalObject. (that may or may not be the best we can do - I'd like it to be generic).
     * work on having the same input do different things, based on the current role or player mode. (car mode and lander mode, both with WASD controls is the goal)
     */

    /* Adding a Vehicle
     * Add the specific Gobject under physicsObjects folder
     * Add Getter method to PhysicsManager with some defaults like location, orientation, etc.
     * 
     * In the specific Game,
     *  - Process the asset type in ProcessObjectAdded
     *  - Process the asset type in AddNewObject
     *  - Add InputManager Key Bindings in the specific Game
     * 
     * In the specific Gobject, 
     *  - add the degrees of freedom into an "Actions" enum
     *  - add generic action methods that take object[] 
     *  - Add ActionManager Action Bindings in the constructor
     *  - call ActionManager.SetActionValues in specific action method
     *  - override SetNominalInput method and define what nominal input is 
     * 
     */
    public class CarGame : BaseGame
    {

        Model carModel, wheelModel, landerModel;
        CarObject myCar;
        Model terrainModel;
        Model planeModel;
        Texture2D moon;
        Model cubeModel;
        Model sphereModel;
        LunarVehicle lander;

        public CarGame()
        {
            name = "CarGame";
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

            chatFont = Content.Load<SpriteFont>("debugFont");
            ChatManager = new Chat(chatFont);
            ChatMessageReceived += new Helper.Handlers.ChatMessageEH(ChatManager.ReceiveMessage);

           
        }

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
        }

        public override void InitializeInputs()
        {
            inputManager = new InputManager(this.name, GetDefaultKeyMap());

            inputManager.AddInputMode(InputMode.Chat, (ChatDelegate)ChatCallback);
        }

        public override List<KeyBinding> GetDefaultKeyBindings()
        {
            List<KeyBinding> defaults = base.GetDefaultKeyBindings();
            // Car
            defaults.Add(new KeyBinding("CarAccelerate", Keys.Up, false, false, false, KeyEvent.Down, Accelerate));
            defaults.Add(new KeyBinding("CarSteerLeft", Keys.Left, false, false, false, KeyEvent.Down, SteerLeft));
            defaults.Add(new KeyBinding("CarDecelerate", Keys.Down, false, false, false, KeyEvent.Down, Deccelerate));
            defaults.Add(new KeyBinding("CarSteerRight", Keys.Right, false, false, false, KeyEvent.Down, SteerRight));
            defaults.Add(new KeyBinding("Handbrake", Keys.B, false, false, false, KeyEvent.Down, ApplyHandbrake));
            // player 
            defaults.Add(new KeyBinding("RespawnCar", Keys.R, false, true, false, KeyEvent.Pressed, SpawnCar));
            // Spheres
            defaults.Add(new KeyBinding("SpawnSpheres", Keys.N, false, true, false, KeyEvent.Pressed, SpawnSpheres));

            // Chat
            defaults.Add(new KeyBinding("ChatKeyPressed", Keys.Enter, false, false, false, KeyEvent.Pressed, ChatKeyPressed));

            //Lunar Lander
            defaults.Add(new KeyBinding("SpawnLunarLander", Keys.Decimal, false, false, false, KeyEvent.Pressed, SpawnLander));
            defaults.Add(new KeyBinding("LunarThrustUp", Keys.Space, false, false, false, KeyEvent.Down, LunarThrustUp));
            defaults.Add(new KeyBinding("LunarPitchUp", Keys.NumPad5, false, false, false, KeyEvent.Down, LunarPitchUp));
            defaults.Add(new KeyBinding("LunarPitchDown", Keys.NumPad8, false, false, false, KeyEvent.Down, LunarPitchDown));
            defaults.Add(new KeyBinding("LunarRollLeft", Keys.NumPad4, false, false, false, KeyEvent.Down, LunarRollLeft));            
            defaults.Add(new KeyBinding("LunarRollRight", Keys.NumPad6, false, false, false, KeyEvent.Down, LunarRollRight));
            defaults.Add(new KeyBinding("LunarYawLeft", Keys.NumPad7, false, false, false, KeyEvent.Down, LunarYawLeft));
            defaults.Add(new KeyBinding("LunarYawRight", Keys.NumPad9, false, false, false, KeyEvent.Down, LunarYawRight));

            //
            defaults.Add(new KeyBinding("SpawnPlane", Keys.P, false, true, false, KeyEvent.Pressed, SpawnPlane));
            defaults.Add(new KeyBinding("IncreaseThrust", Keys.OemPlus, false, false, false, KeyEvent.Down, PlaneThrustIncrease));
            defaults.Add(new KeyBinding("DecreaseThrust", Keys.OemMinus, false, false, false, KeyEvent.Down, PlaneThrustDecrease));
            defaults.Add(new KeyBinding("RollLeft", Keys.H, false, false, false, KeyEvent.Down, PlaneRollLeft));
            defaults.Add(new KeyBinding("RollRight", Keys.K, false, false, false, KeyEvent.Down, PlaneRollRight));
            defaults.Add(new KeyBinding("PitchUp", Keys.J, false, false, false, KeyEvent.Down, PlanePitchUp));
            return defaults;
        }

        
        public override KeyMap GetDefaultKeyMap()
        {
            return new KeyMap(this.name, GetDefaultKeyBindings());
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
                case "sphere":
                    newobject = physicsManager.GetDefaultSphere(model);
                    break;
                case "car":
                    newobject = physicsManager.GetCar(carModel, wheelModel);
                    break;
                case "lunar lander":
                    newobject = physicsManager.GetLunarLander(landerModel);
                    break;
                case "cube":
                    newobject = physicsManager.GetAircraft(model);
                    break;
                default:
                    break;
            }
            
            newobject.ID = objectid;
            physicsManager.AddNewObject(newobject);
        }        

        /// <summary>
        /// CLIENT SIDE
        /// 
        /// </summary>
        private void SpawnCar()
        {
            if(commClient!=null)
                // send a request to the server for an object of asset type "car"
                commClient.SendObjectRequest("car");
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
                    myCar = physicsManager.GetCar(carModel, wheelModel);
                    myCar.ID = objectid;
                    physicsManager.AddNewObject(myCar);
                    if (ownerid == MyClientID) // Only select the new car if its OUR new car
                        SelectGameObject(myCar);
                    break;
                case "lunar lander":
                    lander = physicsManager.GetLunarLander(landerModel);
                    lander.ID = objectid;
                    physicsManager.AddNewObject(lander);
                    if (ownerid == MyClientID) // Only select the new object if its OUR new object
                        SelectGameObject(lander);
                    break;
                case "cube":
                    myPlane = physicsManager.GetAircraft(model);
                    myPlane.ID = objectid;
                    physicsManager.AddNewObject(myPlane);
                    if (ownerid == MyClientID) // Only select the new plane if its OUR new plane
                        SelectGameObject(lander);
                    break;
                default:
                    break;
            }
        }


        private void SpawnPlane()
        {
            // test code for client-side aircraft/plane spawning
            myPlane = physicsManager.GetAircraft(cubeModel);
            myPlane.ID = gameObjects.Count;
            physicsManager.AddNewObject(myPlane);

            if (commClient != null)
                commClient.SendObjectRequest("cube");
        }
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
            myPlane.SetAilerons(-.1f);
        }

        private void PlaneRollRight()
        {
            if (myPlane == null) return;
            myPlane.SetAilerons(.1f);
        }

        private void PlanePitchUp()
        {
            if (myPlane == null) return;
            myPlane.PitchUp(.1f);
        }


        private void SpawnLander()
        {
            if (commClient != null)
            {
                commClient.SendObjectRequest("lunar lander");
            }
        }

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
                if (Vector3.Distance(cam.CurrentPosition, pos[i]) < 100)
                {
                    Vector3 screen = sb.GraphicsDevice.Viewport.Project(pos[i], cam._projection, cam.RhsLevelViewMatrix, Matrix.Identity);
                    
                    int size = (int)chatFont.MeasureString(text[i]).X;
                    sb.Draw(BlankBackground, new Rectangle((int)screen.X - size/2, (int)screen.Y, size, chatFont.LineSpacing), Color.Gray * .5f);

                    sb.DrawString(chatFont, text[i], new Vector2(screen.X - size/2, screen.Y), Color.White);
                }
            }

            ChatManager.Draw(sb);
        }

        
    }
}
