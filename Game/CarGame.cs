using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Physics.PhysicsObjects;
using Microsoft.Xna.Framework;

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
            
            myCar = physicsManager.AddCar(carModel, wheelModel);
            currentSelectedObject = myCar;
        }

        public override void  InitializeInputs()
        {
 	        base.InitializeInputs();
            // Car controls
            inputManager.AddWatch(new Input.KeyWatch(Keys.Up, false, false, false, Input.KeyWatch.keyEvent.Down, Accelerate));
            inputManager.AddWatch(new Input.KeyWatch(Keys.Down, false, false, false, Input.KeyWatch.keyEvent.Down, Decelerate));
            inputManager.AddWatch(new Input.KeyWatch(Keys.Left, false, false, false, Input.KeyWatch.keyEvent.Down, SteerLeft));
            inputManager.AddWatch(new Input.KeyWatch(Keys.Right, false, false, false, Input.KeyWatch.keyEvent.Down, SteerRight));
            inputManager.AddWatch(new Input.KeyWatch(Keys.B, false, false, false, Input.KeyWatch.keyEvent.Down, ApplyHandbrake));
            // Random Spheres
            inputManager.AddWatch(new Input.KeyWatch(Keys.N, false, true, false, Input.KeyWatch.keyEvent.Pressed, SpawnSpheres));

        }
        /*
        /// <summary>
        /// This is called before the Xna_Panel is told where the camera is
        /// </summary>
        public override void PreUpdateCameraCallback()
        {
            // we can set our camera where we want it here
            //cam.CurrentPosition = myCar.Position;
            Vector3 ThirdPersonRef = new Vector3(0, 1, 5);
            if (currentSelectedObject == null)
                return;
            Vector3 TransRef = Vector3.Transform(ThirdPersonRef, currentSelectedObject.BodyOrientation());
            Vector3 bodyPosition = currentSelectedObject.BodyPosition();
            cam.TargetPosition = TransRef + bodyPosition;
            cam.LookAtLocation(bodyPosition);
        }*/

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

        private void Accelerate()
        {
            myCar.SetAcceleration(1.0f);
            
        }

        private void Decelerate()
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
