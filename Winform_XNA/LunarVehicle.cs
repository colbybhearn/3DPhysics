using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using Microsoft.Xna.Framework;

namespace Winform_XNA
{
    public class LunarVehicle
    {
        BoostController VertJet;
        BoostController RotJetY;
        BoostController RotJetX;
        BoostController RotJetZ;
        const float MAX_VERT_MAGNITUDE=5;
        const float MAX_ROT_JETX=1;
        const float MAX_ROT_JETZ=1;

        public LunarVehicle(Body body)
        {
            VertJet = new BoostController(body, Vector3.Up, Vector3.Zero);
            RotJetX = new BoostController(body, Vector3.Zero, Vector3.UnitZ);
            RotJetZ = new BoostController(body, Vector3.Zero, Vector3.UnitX);
            RotJetY = new BoostController(body, Vector3.Zero, Vector3.UnitY);

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

        
    }
}
