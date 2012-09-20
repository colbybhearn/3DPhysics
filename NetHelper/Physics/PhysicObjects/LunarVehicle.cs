using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
//using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Geometry;
using System.Windows.Forms;

namespace Helper.Physics.PhysicsObjects
{
    public class LunarVehicle : Gobject
    {
        BoostController VertJet;
        BoostController RotJetY;
        BoostController RotJetX;
        BoostController RotJetZ;
        const float MAX_VERT_MAGNITUDE=30;
        const float MAX_ROT_JET=10;

        public LunarVehicle(Vector3 position, Vector3 scale, Primitive primitive, Model model, string asset)
            : base(position, scale, primitive, model, true, asset)
        {
            VertJet = new BoostController(Body, Vector3.Up, Vector3.Zero);
            RotJetX = new BoostController(Body, Vector3.Zero, Vector3.UnitZ);
            RotJetZ = new BoostController(Body, Vector3.Zero, Vector3.UnitX);
            RotJetY = new BoostController(Body, Vector3.Zero, Vector3.UnitY);

            PhysicsSystem.CurrentPhysicsSystem.AddController(VertJet);
            PhysicsSystem.CurrentPhysicsSystem.AddController(RotJetX);
            PhysicsSystem.CurrentPhysicsSystem.AddController(RotJetZ);
            PhysicsSystem.CurrentPhysicsSystem.AddController(RotJetY);

            actionManager.AddBinding((int)Actions.ThrustUp, new Helper.Input.ActionBindingDelegate(GenericThrustUp), 1);
            actionManager.AddBinding((int)Actions.Pitch, new Helper.Input.ActionBindingDelegate(GenericPitch), 1);
            actionManager.AddBinding((int)Actions.Roll, new Helper.Input.ActionBindingDelegate(GenericRoll), 1);
            actionManager.AddBinding((int)Actions.Yaw, new Helper.Input.ActionBindingDelegate(GenericYaw), 1);
        }

        public enum Actions
        {
            ThrustUp,
            Roll,
            Pitch,
            Yaw
        }

        private void GenericThrustUp(object[] v)
        {
            SetVertJetThrust((float)v[0]);
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

        public void SetVertJetThrust(float v)
        {
            VertJet.SetForceMagnitude(v * MAX_VERT_MAGNITUDE);
            actionManager.SetActionValues((int)Actions.ThrustUp, new object[] { v });
        }

        public void SetRotJetXThrust(float v)
        {
            RotJetX.SetTorqueMagnitude(v * MAX_ROT_JET);
            actionManager.SetActionValues((int)Actions.Pitch, new object[] { v });
        }

        public void SetRotJetZThrust(float v)
        {
            RotJetZ.SetTorqueMagnitude(v * MAX_ROT_JET);
            actionManager.SetActionValues((int)Actions.Roll, new object[] { v });
        }

        public void SetRotJetYThrust(float v)
        {
            RotJetY.SetTorqueMagnitude(v * MAX_ROT_JET);
            actionManager.SetActionValues((int)Actions.Yaw, new object[] { v });
        }

        public override void SetNominalInput()
        {
            SetVertJetThrust(0);
            SetRotJetXThrust(0);
            SetRotJetYThrust(0);
            SetRotJetZThrust(0);
        }
        
    }
}
