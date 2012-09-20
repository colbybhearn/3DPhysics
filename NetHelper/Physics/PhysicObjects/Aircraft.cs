using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Helper.Physics.PhysicObjects
{
    public class Aircraft : Gobject
    {
        BoostController Yaw;
        BoostController Pitch;
        BoostController Roll;

        BoostController Thrust;
        BoostController LiftLeft;
        BoostController LiftRight;
        BoostController Drag;


        const float MAX_VERT_MAGNITUDE=30;
        const float MAX_ROT_JET=10;
        float ForwardThrust =0;
        const float DragCoefficient = .1f;
        float drag = 0;
        float LeftWingLiftCoefficient = 0;
        float RightWingLiftCoefficient  =0;
        const float WingLiftCoefficientMin = .7f;
        const float WingLiftCoefficientMax = 1.0f;

        public Aircraft(Vector3 position, Vector3 scale, Primitive primitive, Model model, string asset)
            : base(position, scale, primitive, model, true, asset)
        {
            
            Pitch = new BoostController(Body, Vector3.Zero, Vector3.UnitZ);
            Roll = new BoostController(Body, Vector3.Zero, Vector3.UnitX);
            Yaw = new BoostController(Body, Vector3.Zero, Vector3.UnitY);

            Thrust = new BoostController(Body, Vector3.Forward, Vector3.Forward, Vector3.Zero);
            LiftLeft = new BoostController(Body, Vector3.Zero, Vector3.UnitZ);  // this could be totally different than a force at a position (midwing)
            LiftRight = new BoostController(Body, Vector3.Zero, -Vector3.UnitZ);
            Drag = new BoostController(Body, Vector3.Backward, Vector3.Backward, Vector3.Zero);

            PhysicsSystem.CurrentPhysicsSystem.AddController(Thrust);
            PhysicsSystem.CurrentPhysicsSystem.AddController(LiftLeft);
            PhysicsSystem.CurrentPhysicsSystem.AddController(LiftRight);
            PhysicsSystem.CurrentPhysicsSystem.AddController(Drag);

            actionManager.AddBinding((int)Actions.Thrust, new Helper.Input.ActionBindingDelegate(GenericThrustUp), 1);
            actionManager.AddBinding((int)Actions.Pitch, new Helper.Input.ActionBindingDelegate(GenericPitch), 1);
            actionManager.AddBinding((int)Actions.Roll, new Helper.Input.ActionBindingDelegate(GenericRoll), 1);
            actionManager.AddBinding((int)Actions.Yaw, new Helper.Input.ActionBindingDelegate(GenericYaw), 1);
        }

        public enum Actions
        {
            Thrust,
            Roll,
            Pitch,
            Yaw,
        }

        private void GenericThrustUp(object[] v)
        {
            AdjustForwardThrust((float)v[0]);
        }
        private void GenericPitch(object[] v)
        {
            SetRotJetXThrust((float)v[0]);
        }
        private void GenericRoll(object[] v)
        {
            SetRotJetZThrust((float)v[0]);
        }
        private void GenericYaw(object[] v)
        {
            SetRotJetYThrust((float)v[0]);
        }

        public void AdjustForwardThrust(float v)
        {
            ForwardThrust += v;            
            Thrust.SetForceMagnitude(ForwardThrust);
            Drag.SetForceMagnitude(drag);
            actionManager.SetActionValues((int)Actions.Thrust, new object[] { v });
        }

        public void RollLeft(float v)
        {
            LeftWingLiftCoefficient -= v;
            if (LeftWingLiftCoefficient < WingLiftCoefficientMin)
                LeftWingLiftCoefficient = WingLiftCoefficientMin;
            if (LeftWingLiftCoefficient > WingLiftCoefficientMax)
                LeftWingLiftCoefficient = WingLiftCoefficientMax;
        }

        public void RollRight(float v)
        {
            RightWingLiftCoefficient -= v;
            if (RightWingLiftCoefficient < WingLiftCoefficientMin)
                RightWingLiftCoefficient = WingLiftCoefficientMin;
            if (RightWingLiftCoefficient > WingLiftCoefficientMax)
                RightWingLiftCoefficient = WingLiftCoefficientMax;
        }

        public void SetRotJetXThrust(float v)
        {
            Pitch.SetTorqueMagnitude(v * MAX_ROT_JET);
            actionManager.SetActionValues((int)Actions.Pitch, new object[] { v });
        }

        public void SetRotJetZThrust(float v)
        {
            Roll.SetTorqueMagnitude(v * MAX_ROT_JET);
            actionManager.SetActionValues((int)Actions.Roll, new object[] { v });
        }

        public void SetRotJetYThrust(float v)
        {
            Yaw.SetTorqueMagnitude(v * MAX_ROT_JET);
            actionManager.SetActionValues((int)Actions.Yaw, new object[] { v });
        }

        public override void SetNominalInput()
        {
            Vector3 forwardMotion = Vector3.Cross(BodyVelocity(), BodyOrientation().Forward);
            float forwardSpeed = forwardMotion.Length();
            drag = forwardSpeed * DragCoefficient;
            float leftWingLift = forwardSpeed * LeftWingLiftCoefficient;
            float rightWinLift = forwardSpeed * RightWingLiftCoefficient;
            
            Drag.SetForceMagnitude(drag);
            Thrust.SetForceMagnitude(ForwardThrust);
            LiftLeft.SetForceMagnitude(leftWingLift);
            LiftRight.SetForceMagnitude(rightWinLift);


            SetRotJetXThrust(0);
            SetRotJetYThrust(0);
            SetRotJetZThrust(0);
        }
    }
}
