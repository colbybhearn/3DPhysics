using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;
using JigLibX.Math;
using Microsoft.Xna.Framework;

namespace Winform_XNA
{
    public class BoostController : Controller
    {
        private Body Body;
        public Vector3 Force;
        public Vector3 torque;

        // I'd like to create a controller and tell it how to control from then on.
        // maybe it should be a more abstract body controller, made up of physics controllers
        // if I create a controller and give it a body, 

        public BoostController(Body body, Vector3 force)
        {
            Body = body;
            Force = force;
        }

        public override void UpdateController(float dt)
        {
            if (Body == null)
                return;

            if (Force != null && Force != Vector3.Zero)
            {
                Body.AddBodyForce(Force);
                if (!Body.IsActive)
                    Body.SetActive();
            }
            if (torque != null && torque != Vector3.Zero)
            {
                Body.AddBodyTorque(torque);
                if (!Body.IsActive)
                    Body.SetActive();
            }

        }

    }
}
