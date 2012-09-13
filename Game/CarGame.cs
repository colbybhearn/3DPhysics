using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Physics.PhysicsObjects;
using Microsoft.Xna.Framework;
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
        }

        public override void InitializeMultiplayer(BaseGame.CommTypes CommType)
        {
            base.InitializeMultiplayer(CommType);

            switch (CommType)
            {
                case CommTypes.Client:
                    //commClient.ObjectRequestResponseReceived += new Helper.Handlers.IntEH(commClient_ObjectRequestResponseReceived);
                    break;
                case CommTypes.Server:
                    commServer.ObjectRequestReceived += new Helper.Handlers.ObjectRequestEH(commServer_ObjectRequestReceived);
                    break;

                default:
                    break;
            }

        }

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

            
            return defaults;
        }

        
        public override Input.KeyMap GetDefaultKeyMap()
        {
            return new KeyMap(this.name, GetDefaultKeyBindings());
        }

        /// <summary>
        /// This is called by BaseGame immediately before Keyboard state is used to process the KeyWatches
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

        /*
        public override bool AddNewObject(int objectid, string asset)
        {
            // the game needs to know what assets go with what primitives
            // if not primitives, then physicsobjects or something
            
            Model model = Content.Load<Model>(asset);
            Gobject newobject = physicsManager.GetDefaultSphere(model);
            newobject.ID = objectid;
            return physicsManager.AddNewObject(newobject);
        }*/

        private void SpawnCar()
        {
            //if (myCar != null)
                //gameObjects.Remove(myCar.ID);
            //myCar = physicsManager.GetCar(carModel, wheelModel);
            //currentSelectedObject = myCar;
            if(commClient!=null)
                commClient.SendObjectRequest("car");
        }

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
    }
}
