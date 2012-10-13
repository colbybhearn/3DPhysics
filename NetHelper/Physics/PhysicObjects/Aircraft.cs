using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Collections.Generic;
using JigLibX.Collision;
using System;

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
         * http://jiglibx.wikidot.com/
         * http://www.centennialofflight.gov/essay/Theories_of_Flight/Stability/TH26.htm
         */

        BoostController Yaw;
        BoostController Elevator;
        BoostController Thrust;
        BoostController LiftLeft;
        BoostController LiftRight;
        BoostController Drag;

        Vector3 CenterOfPressure;
        Vector3 CenterOfMass;

        float ForwardThrust =0;
        const float DragCoefficient = .1f;
        float dragForce = 0;
        float WingLiftCoefficient = .050f;
        const float AileronFactor = .001f;
        float RollDestination = 0;
        float RollCurrent = 0;
        float ElevatorTarget = 0;
        float ElevatorForce=0;
        float ElevatorCoefficient = .001f;
        public float ElevatorCurrent = 0;
        const float MaxThrust = 1000;
        const float MinThrust = -15;

        float airDensity = 1.2f;


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

        public Aircraft(Vector3 position, Vector3 scale, Primitive primitive, Model model, int asset)
            : base(position, scale, primitive, model, true, asset)
        {
            Initialize();
        }

        public Aircraft(Vector3 position, Vector3 scale, List<Primitive> prims, List<MaterialProperties> props, Model model, int asset)
            :base (position, scale, prims, props, model, asset)
        {
            Initialize();
        }

        private void Initialize()
        {
            //float mass;
            //Vector3 com;
            //Matrix it, itCom;
            //Skin.GetMassProperties(new PrimitiveProperties( PrimitiveProperties.MassDistributionEnum.Shell, PrimitiveProperties.MassTypeEnum.Mass, 0), out mass, out com, out it, out itCom);
            CenterOfMass = new Vector3(0, 0, -.5f);
            CenterOfPressure = new Vector3(0, 0, 0.0f);

            Vector3 LeftWingLiftLocation = 4 * Vector3.Left;
            LeftWingLiftLocation.Z = CenterOfPressure.Z;
            Vector3 RightWingLiftLocation = 4 * Vector3.Right;
            RightWingLiftLocation.Z = CenterOfPressure.Z;

            Thrust = new BoostController(Body, Vector3.Forward, 4 * Vector3.Forward, Vector3.Zero);
            LiftLeft = new BoostController(Body, Vector3.Up, LeftWingLiftLocation, Vector3.Zero);  // this could be totally different than a force at a position (midwing)
            LiftRight = new BoostController(Body, Vector3.Up, RightWingLiftLocation, Vector3.Zero);
            Elevator = new BoostController(Body, Vector3.Zero, Vector3.Backward * 3, Vector3.Zero);
            Drag = new BoostController(Body, Vector3.Zero, Vector3.Zero, Vector3.Zero);

            Yaw = new BoostController(Body, Vector3.Zero, Vector3.UnitY);
            Drag.worldForce = true;

            AddController(Thrust);
            AddController(LiftLeft);
            AddController(LiftRight);
            AddController(Drag);
            AddController(Elevator);

            actionManager.AddBinding((int)Actions.Thrust, new Helper.Input.ActionBindingDelegate(SimulateThrust), 1);
            actionManager.AddBinding((int)Actions.Aileron, new Helper.Input.ActionBindingDelegate(SimulateAileron), 1);
            actionManager.AddBinding((int)Actions.Elevator, new Helper.Input.ActionBindingDelegate(SimulateElevator), 1);
        }
        public override void FinalizeBody()
        {
            try
            {
                Vector3 com = SetMass(1.0f);
                Body.MoveTo(Position, Matrix.Identity);
                //Skin.ApplyLocalTransform(new JigLibX.Math.Transform(-com, Matrix.Identity));
                Body.EnableBody(); // adds to CurrentPhysicsSystem
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        public enum Actions
        {
            Thrust,
            Roll,
            Yaw,
            Aileron,
            Elevator,
        }

        // simulated input
        private void SimulateAileron(object[] v)
        {
            SetAilerons((float)v[0]);
        }
        private void SimulateElevator(object[] v)
        {
            SetElevator((float)v[0]);
        }
        private void SimulateThrust(object[] v)
        {
            SetThrust((float)v[0]);
        }

        //user input
        public void AdjustThrust(float v)
        {
            SetThrust(ForwardThrust + v);
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
        private void SetRightWingLift(float right)
        {
            LiftRight.SetForceMagnitude(right);
        }
        private void SetLeftWingLift(float left)
        {
            LiftLeft.SetForceMagnitude(left);
        }
        private void UpdateDrag()
        {
            // amount of velocity 
            // a large amount of velocity in the direction of forward means we're going very straight. We want small drag.
            // a small amount of velocity in the direction of forward means we're crooked. We want large drag.
            //float f = BodyOrientation().Forward.Length() - Vector3.Dot(BodyOrientation().Forward, BodyVelocity());
            Vector3 velo = BodyVelocity();
            Vector3 forw = BodyOrientation().Forward;
            if (velo == Vector3.Zero ||
                forw == Vector3.Zero)
            {
                Drag.SetForceMagnitude(0);
                return;
            }

            float f = 1 - Vector3.Dot(Vector3.Normalize(BodyVelocity()), Vector3.Normalize(BodyOrientation().Forward));
            float area = .6f + (.5f * f);
            //if(area >.00001f)
                //System.Diagnostics.Debug.WriteLine(area);
            // 1/2 * airDensity * Velocity^2 * Cd * area
            dragForce = .5f * airDensity * BodyVelocity().LengthSquared() * DragCoefficient * area;
            //dragForce = BodyVelocity().Length() * DragCoefficient;
            Drag.Force = Vector3.Normalize(-BodyVelocity());
            Matrix orient = BodyOrientation();
            Drag.ForcePosition = Body.Position + Vector3.Transform(CenterOfPressure, orient);
            Drag.SetForceMagnitude(dragForce);
        }
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

            float leftWingLift = ForwardAirSpeed * LeftWingLiftCoefficient;
            float rightWingLift = ForwardAirSpeed * RightWingLiftCoefficient;
            
            SetLeftWingLift(leftWingLift);
            SetRightWingLift(rightWingLift);
            actionManager.SetActionValues((int)Actions.Aileron, new object[] { RollDestination });
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
        
        public override void SetNominalInput()
        {
            RollCurrent += (RollDestination - RollCurrent) * .7f;
            
            SetThrust(ForwardThrust);
            UpdateDrag();
            SetAilerons(0);
            SetElevator(0);
        }
    }
}
