using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Geometry;

namespace Winform_XNA
{
    public class LunarVehicle : Gobject
    {
        BoostController VertJet;
        BoostController RotJetY;
        BoostController RotJetX;
        BoostController RotJetZ;
        const float MAX_VERT_MAGNITUDE=5;
        const float MAX_ROT_JETX=1;
        const float MAX_ROT_JETZ=1;

        public LunarVehicle(Vector3 position, Vector3 scale, Primitive primitive, Model model)
            : base(position, scale, primitive, model, true)
        {
            VertJet = new BoostController(Body, Vector3.Up, Vector3.Zero);
            RotJetX = new BoostController(Body, Vector3.Zero, Vector3.UnitZ);
            RotJetZ = new BoostController(Body, Vector3.Zero, Vector3.UnitX);
            RotJetY = new BoostController(Body, Vector3.Zero, Vector3.UnitY);

            PhysicsSystem.CurrentPhysicsSystem.AddController(VertJet);
            PhysicsSystem.CurrentPhysicsSystem.AddController(RotJetX);
            PhysicsSystem.CurrentPhysicsSystem.AddController(RotJetZ);
            PhysicsSystem.CurrentPhysicsSystem.AddController(RotJetY);
        }

        public void SetVertJetThrust(float percentThrust)
        {
            VertJet.SetForceMagnitude(percentThrust * MAX_VERT_MAGNITUDE);
        }

        public void SetRotJetXThrust(float percentThrust)
        {
            RotJetX.SetTorqueMagnitude(percentThrust * MAX_ROT_JETX);
        }

        public void SetRotJetZThrust(float percentThrust)
        {
            RotJetZ.SetTorqueMagnitude(percentThrust * MAX_ROT_JETZ);
        }

        public void SetRotJetYThrust(float percentThrust)
        {
            RotJetY.SetTorqueMagnitude(percentThrust * MAX_ROT_JETZ);
        }

        public void ProcessInputKeyUp(KeyEventArgs e)
        {
            Keys key = e.KeyCode;
            if (key == Keys.Space)
            {
                SetVertJetThrust(0);
            }
            if (key == Keys.W)
            {
                SetRotJetZThrust(0);
            }
            if (key == Keys.A)
            {
                SetRotJetXThrust(0);
            }
            if (e.KeyCode == Keys.S)
            {
                SetRotJetZThrust(0);
            }
            if (e.KeyCode == Keys.D)
            {
                SetRotJetXThrust(0);
            }
            if (e.KeyCode == Keys.Q)
            {
                SetRotJetYThrust(0);
            }
            if (e.KeyCode == Keys.E)
            {
                SetRotJetYThrust(0);
            }


        }

        public void ProcessInputKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                SetVertJetThrust(1.0f);
            }
            if (e.KeyCode == Keys.W)
            {
                SetRotJetZThrust(-.4f);
            }
            if (e.KeyCode == Keys.A)
            {
                SetRotJetXThrust(.4f);
            }
            if (e.KeyCode == Keys.S)
            {
                SetRotJetZThrust(.4f);
            }
            if (e.KeyCode == Keys.D)
            {
                SetRotJetXThrust(-.4f);
            }
            if (e.KeyCode == Keys.Q)
            {
                SetRotJetYThrust(.4f);
            }
            if (e.KeyCode == Keys.E)
            {
                SetRotJetYThrust(-.4f);
            }   
        }
        
    }
}
