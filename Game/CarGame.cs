using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Physics.PhysicsObjects;
using Helper;
using Input;
using Physics;

namespace Game
{
    public class CarGame : BaseGame
    {
        Model carModel, wheelModel;
        CarObject myCar;
        Model terrainModel;
        Model planeModel;
        Texture2D moon;
        Model cubeModel;
        Model sphereModel;



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

            chatFont = Content.Load<SpriteFont>("debugFont");
            ChatManager = new Chat(chatFont);
            ChatMessageReceived += new Helper.Handlers.StringStringEH(ChatManager.ReceiveMessage);

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
            inputManager = new Input.InputManager(this.name, GetDefaultKeyMap());

            inputManager.AddInputMode(InputMode.Chat, (Input.ChatDelegate)ChatCallback);
        }

        public override List<KeyBinding> GetDefaultKeyBindings()
        {
            List<KeyBinding> defaults = base.GetDefaultKeyBindings();
            // Car
            defaults.Add(new KeyBinding("CarAccelerate", Keys.Up, false, false, false, Input.KeyEvent.Down, Accelerate));
            defaults.Add(new KeyBinding("CarSteerLeft", Keys.Left, false, false, false, Input.KeyEvent.Down, SteerLeft));
            defaults.Add(new KeyBinding("CarDecelerate", Keys.Down, false, false, false, Input.KeyEvent.Down, Deccelerate));
            defaults.Add(new KeyBinding("CarSteerRight", Keys.Right, false, false, false, Input.KeyEvent.Down, SteerRight));
            defaults.Add(new KeyBinding("Handbrake", Keys.B, false, false, false, Input.KeyEvent.Down, ApplyHandbrake));
            // player 
            defaults.Add(new KeyBinding("RespawnCar", Keys.R, false, true, false, Input.KeyEvent.Pressed, SpawnCar));
            // Spheres
            defaults.Add(new KeyBinding("SpawnSpheres", Keys.N, false, true, false, Input.KeyEvent.Pressed, SpawnSpheres));

            // Chat
            defaults.Add(new KeyBinding("ChatKeyPressed", Keys.Enter, false, false, false, Input.KeyEvent.Pressed, ChatKeyPressed));
            return defaults;
        }

        
        public override Input.KeyMap GetDefaultKeyMap()
        {
            return new KeyMap(this.name, GetDefaultKeyBindings());
        }

        /// <summary>
        /// This is called by BaseGame immediately before Keyboard state is used to process the KeyBindings
        /// we don't want to handle keydowns and keyups, so revert to nominal states and then immediately process key actions to arrive at a current state
        /// </summary>
        public override void SetNominalInputState()
        {
            foreach (int i in clientControlledObjects)
            {
                if (!gameObjects.ContainsKey(i))
                    return;
                Gobject go = gameObjects[i];
                if (go is CarObject)
                {
                    CarObject myCar = go as CarObject;
                    // we don't want to handle
                    myCar.SetAcceleration(0);
                    myCar.SetSteering(0);
                    myCar.setHandbrake(0);
                }
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
                case "sphere":
                    newobject = physicsManager.GetDefaultSphere(model);
                    break;
                case "car":
                    newobject = physicsManager.GetCar(carModel, wheelModel);
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

        /// <summary>
        /// CLIENT SIDE
        /// client should do something oriented to the specific game here, like player bullets or cars.
        /// The server has granted the object request and this is where we handle the response it has sent back to the client
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="asset"></param>
        public override void ProcessObjectRequestResponse(int objectid, string asset)
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
                    break;
                default:
                    break;
            }

            
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            ChatManager.Draw(spriteBatch);
        }
    }
}
