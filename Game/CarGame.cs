using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Physics.PhysicsObjects;
using Microsoft.Xna.Framework;
using Input;

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

        public override void InitializeEnvironment()
        {
            base.InitializeEnvironment();
            RespawnCar();
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
            defaults.Add(new KeyBinding("RespawnCar", Keys.R, false, true, false, Input.KeyEvent.Pressed, RespawnCar));
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
            // we don't want to handle
            myCar.SetAcceleration(0);
            myCar.SetSteering(0);
            myCar.setHandbrake(0);
        }

        private void RespawnCar()
        {
            if (myCar != null)
                gameObjects.Remove(myCar);
            myCar = physicsManager.AddCar(carModel, wheelModel);
            currentSelectedObject = myCar;
        }

        private void Accelerate()
        {
            myCar.SetAcceleration(1.0f);
        }

        private void Deccelerate()
        {
            myCar.SetAcceleration(-1.0f);
        }

        private void SteerLeft()
        {
            myCar.SetSteering(1.0f);
        }

        private void SteerRight()
        {
            myCar.SetSteering(-1.0f);
        }

        private void ApplyHandbrake()
        {
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
