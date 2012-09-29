using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Helper.Physics.PhysicsObjects
{
    public class Aircraft : Gobject
    {

        /* Flight dynamics
         * Craft flies by balancing drag and gravity with thrust and lift.
         * Main wings produce an upward lift pressure
         *  - lift coefficient depends upon angle of attack. 
         *  - increased angle of attack => greater lift and results in slower speed)
         *  - decreased angle of attack => less lift and allows faster speed
         *  
         * Center of gravity
         *  - http://en.wikipedia.org/wiki/Longitudinal_static_stability
         *  - should exist within boundaries determined by the design of the craft (wing chord length, wing placement, horizontal stabilizer's restoring moment)
         *  
         * 
         * Tail horizontal stablizer (http://en.wikipedia.org/wiki/Elevator_(aircraft))
         *  - produces a downward pressure
         *  - up elevator forces the tail down and nose up (increased angle of attack for main wings => greater lift)
         * http://adamone.rchomepage.com/cg_calc.htm
         * //with equations:
         * http://www.geistware.com/rcmodeling/cg_super_calc.htm
         * 
         * 
         */

        BoostController Yaw;
        BoostController Elevator;
        BoostController Thrust;
        BoostController LiftLeft;
        BoostController LiftRight;
        BoostController Drag;

        float ForwardThrust =0;
        const float DragCoefficient = .1f;
        float dragForce = 0;
        float WingLiftCoefficient = .050f;
        const float AileronFactor = .03f;
        float RollDestination = 0;
        float RollCurrent = 0;
        float ElevatorTarget = 0;
        float ElevatorForce=0;
        float ElevatorCoefficient = .01f;
        const float MaxThrust = 1000;
        const float MinThrust = -15;

        public float ForwardAirSpeed
        {
            get
            {
                Vector3 vel = BodyVelocity();
                Vector3 forr = BodyOrientation().Forward;
                // the amount of vel in the direction of forr
                float speed = Vector3.Dot(vel, forr);
                if (speed < 0)
                    return 0;
                return speed;
            }
        }

        public Aircraft(Vector3 position, Vector3 scale, Primitive primitive, Model model, string asset)
            : base(position, scale, primitive, model, true, asset)
        {
            
            
            Thrust = new BoostController(Body, Vector3.Forward, 4*Vector3.Forward, Vector3.Zero);
            LiftLeft = new BoostController(Body, Vector3.Up,  4*Vector3.Left, Vector3.Zero);  // this could be totally different than a force at a position (midwing)
            LiftRight = new BoostController(Body, Vector3.Up, 4*Vector3.Right, Vector3.Zero);
            Elevator = new BoostController(Body, Vector3.Zero, Vector3.Backward*3, Vector3.Zero);
            Drag = new BoostController(Body, Vector3.Zero, Vector3.Zero, Vector3.Zero);

            Yaw = new BoostController(Body, Vector3.Zero, Vector3.UnitY);
            Drag.worldForce = true;

            AddController(Thrust);
            AddController(LiftLeft);
            AddController(LiftRight);
            AddController(Drag);
            AddController(Elevator);
            
            actionManager.AddBinding((int)Actions.Thrust, new Helper.Input.ActionBindingDelegate(GenericThrust), 1);
            actionManager.AddBinding((int)Actions.Aileron, new Helper.Input.ActionBindingDelegate(GenericAileron), 1);
            actionManager.AddBinding((int)Actions.Elevator, new Helper.Input.ActionBindingDelegate(GenericElevator), 1);
            SetMass(500);
            // airplane wobbles
            // drag is applied at very back with maximum leverage
            // surface area exposed by inclination may be sketchy
            // Elevator is unreasonably poweful
        }

        public enum Actions
        {
            Thrust,
            Roll,
            Yaw,
            Aileron,
            Elevator,
        }

        private void GenericAileron(object[] v)
        {
            SetAilerons((float)v[0]);
        }
        private void GenericElevator(object[] v)
        {
            SetElevator((float)v[10]);
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
        public void SetThrust(float v)
        {
            ForwardThrust = v;
            if (ForwardThrust <= MinThrust)
                ForwardThrust = MinThrust;
            if (ForwardThrust >= MaxThrust)
                ForwardThrust = MaxThrust;
            
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

        float airDensity = 1.2f;

        private void SetDrag(float v)
        {
            // amount of velocity 
            // a large amount of velocity in the direction of forward means we're going very straight. We want small drag.
            // a small amount of velocity in the direction of forward means we're crooked. We want large drag.
            //float f = BodyOrientation().Forward.Length() - Vector3.Dot(BodyOrientation().Forward, BodyVelocity());
            float f = 1 - Vector3.Dot(Vector3.Normalize(BodyVelocity()), Vector3.Normalize(BodyOrientation().Forward));
            float area = .01f + (.5f * f);
            //if(area >.00001f)
                //System.Diagnostics.Debug.WriteLine(area);
            // 1/2 * airDensity * Velocity^2 * Cd * area
            dragForce = .5f * airDensity * BodyVelocity().LengthSquared() * DragCoefficient * area;
            //dragForce = BodyVelocity().Length() * DragCoefficient;
            Drag.Force = Vector3.Normalize(-BodyVelocity());
            Matrix orient = BodyOrientation();
            Drag.ForcePosition = Body.Position + orient.Backward * 4 + orient.Up*.5f;
            Drag.SetForceMagnitude(dragForce);
        }

        float leftWingLift = 0;
        float rightWingLift = 0;

        public void SetAilerons(float v)
        {
            RollDestination = v;

            float leftAileron = RollCurrent * -1;
            if (leftAileron < 0)
                leftAileron = 0;
            float LeftWingLiftCoefficient = WingLiftCoefficient - (AileronFactor * leftAileron);

            float rightAileron = RollCurrent;
            if(rightAileron<0)
                rightAileron = 0;
            float RightWingLiftCoefficient = WingLiftCoefficient - (AileronFactor * rightAileron);

            leftWingLift = ForwardAirSpeed * LeftWingLiftCoefficient;
            rightWingLift = ForwardAirSpeed * RightWingLiftCoefficient;
            SetLeftWingLift(leftWingLift);
            //actionManager.SetActionValues((int)Actions.Aileron, new object[] { v });
            SetRightWingLift(rightWingLift);
        }

        public void SetElevator(float v)
        {
            ElevatorTarget = v;
            ElevatorCurrent = ElevatorCurrent + (ElevatorTarget - ElevatorCurrent) * .5f;
            ElevatorForce = ForwardAirSpeed * ElevatorCoefficient * ElevatorCurrent;
            if (ElevatorForce > 0)
                Elevator.Force = Vector3.Down;
            else
                Elevator.Force = Vector3.Down;
            Elevator.SetForceMagnitude(ElevatorForce);

            actionManager.SetActionValues((int)Actions.Elevator, new object[] { ElevatorTarget });
        }
        public float ElevatorCurrent=0;
        public override void SetNominalInput()
        {
            
            RollCurrent = RollCurrent + (RollDestination - RollCurrent) * .7f;
            
            
            SetThrust(ForwardThrust);
            SetDrag(dragForce);
            SetAilerons(0);
            SetElevator(0);
        }
    }
}
