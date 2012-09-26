using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Helper.Physics.PhysicsObjects
{
    public class Aircraft : Gobject
    {
        BoostController Yaw;
        BoostController Pitch;

        BoostController Thrust;
        BoostController LiftLeft;
        BoostController LiftRight;
        BoostController Drag;


        const float MAX_VERT_MAGNITUDE=30;
        const float MAX_ROT_JET=10;
        float ForwardThrust =0;
        const float DragCoefficient = .1f;
        float drag = 0;
        float WingLiftCoefficient = 1.0f;
        const float AileronFactor = .03f;
        float RollDestination = 0;
        float RollCurrent = 0;

        public float AirSpeed
        {
            get
            {
                Vector3 vel = BodyVelocity();
                Vector3 forr = BodyOrientation().Forward;
                // the amount of vel in the direction of forr
                return Vector3.Dot(vel, forr);
            }
        }

        public Aircraft(Vector3 position, Vector3 scale, Primitive primitive, Model model, string asset)
            : base(position, scale, primitive, model, true, asset)
        {
            Pitch = new BoostController(Body, Vector3.Zero, Vector3.UnitZ);
            Yaw = new BoostController(Body, Vector3.Zero, Vector3.UnitY);

            Thrust = new BoostController(Body, Vector3.Forward, Vector3.Forward, Vector3.Zero);
            LiftLeft = new BoostController(Body, Vector3.Up,  2*Vector3.Left, Vector3.Zero);  // this could be totally different than a force at a position (midwing)
            LiftRight = new BoostController(Body, Vector3.Up, 2*Vector3.Right, Vector3.Zero);
            Drag = new BoostController(Body, Vector3.Backward, Vector3.Backward, Vector3.Zero);

            PhysicsSystem.CurrentPhysicsSystem.AddController(Thrust);
            PhysicsSystem.CurrentPhysicsSystem.AddController(LiftLeft);
            PhysicsSystem.CurrentPhysicsSystem.AddController(LiftRight);
            PhysicsSystem.CurrentPhysicsSystem.AddController(Drag);
            
            actionManager.AddBinding((int)Actions.Thrust, new Helper.Input.ActionBindingDelegate(GenericThrust), 1);
            actionManager.AddBinding((int)Actions.Aileron, new Helper.Input.ActionBindingDelegate(GenericAileron), 1);
            /*
            actionManager.AddBinding((int)Actions.Pitch, new Helper.Input.ActionBindingDelegate(GenericPitch), 1);
            actionManager.AddBinding((int)Actions.Roll, new Helper.Input.ActionBindingDelegate(GenericRoll), 1);
            actionManager.AddBinding((int)Actions.Yaw, new Helper.Input.ActionBindingDelegate(GenericYaw), 1);
            */
        }

        public enum Actions
        {
            Thrust,
            Roll,
            Pitch,
            Yaw,
            Aileron
        }

        private void GenericAileron(object[] v)
        {
            SetAilerons((float)v[0]);
        }

        //user input
        public void AdjustThrust(float v)
        {
            SetThrust(ForwardThrust + v);
        }
        // simulated input
        private void GenericThrust(object[] v)
        {
            SetThrust((float)v[0]);
        }
        // common
        public void SetThrust(float t)
        {
            ForwardThrust = t;
            Thrust.SetForceMagnitude(ForwardThrust);
            actionManager.SetActionValues((int)Actions.Thrust, new object[] { ForwardThrust });
        }

        //common
        private void SetRightWingLift(float right)
        {
            LiftRight.SetForceMagnitude(right);
        }

        private void SetLeftWingLift(float left)
        {
            LiftLeft.SetForceMagnitude(left);
        }

        private void SetDrag(float drag)
        {
            Drag.SetForceMagnitude(drag);
        }


        float leftWingLift = 0;
        float rightWingLift = 0;

        public void SetAilerons(float v)
        {
            RollDestination = v;
            if (RollDestination == 0)
            {
                // there's a wobble, 
                // like wrong coordinate systemm..
                // or like wrong direction of force
                Debug.WriteLine(leftWingLift + ", " + rightWingLift);    
            }
            

            float leftAileron = RollCurrent * -1;
            if (leftAileron < 0)
                leftAileron = 0;
            float LeftWingLiftCoefficient = WingLiftCoefficient - (AileronFactor * leftAileron);

            float rightAileron = RollCurrent;
            if(rightAileron<0)
                rightAileron = 0;
            float RightWingLiftCoefficient = WingLiftCoefficient - (AileronFactor * rightAileron);

            leftWingLift = AirSpeed * LeftWingLiftCoefficient;
            rightWingLift = AirSpeed * RightWingLiftCoefficient;
            SetLeftWingLift(leftWingLift);
            //actionManager.SetActionValues((int)Actions.Aileron, new object[] { v });
            SetRightWingLift(rightWingLift);
        }

        public override void SetNominalInput()
        {
            drag = AirSpeed * DragCoefficient;
            RollCurrent = RollCurrent + (RollDestination - RollCurrent) * .3f;
            //System.Diagnostics.Debug.WriteLine(ForwardThrust + ", " + forwardMotion);
            SetThrust(ForwardThrust);
            SetDrag(drag);
            SetAilerons(0);
        }


        public void PitchUp(float p)
        {

        }
    }
}
