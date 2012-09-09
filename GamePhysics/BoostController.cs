using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;
using JigLibX.Math;
using Microsoft.Xna.Framework;

namespace Physics
{
    public class BoostController : Controller
    {
        private Body Body;
        public Vector3 Force;
        public Vector3 Torque;
        float forceMag = 0;
        float torqueMag = 0;

        // I'd like to create a controller and tell it how to control from then on.
        // maybe it should be a more abstract body controller, made up of physics controllers
        // if I create a controller and give it a body, 

        public BoostController(Body body, Vector3 force, Vector3 torque)
        {
            Body = body;
            Force = force;
            Torque = torque;
        }

        public void SetForceMagnitude(float mag)
        {
            forceMag = mag;
        }
        public void SetTorqueMagnitude(float mag)
        {
            torqueMag = mag;
        }

        public override void UpdateController(float dt)
        {
            if (Body == null)
                return;

            if (Force != null && Force != Vector3.Zero)
            {
                Body.AddBodyForce(Force * forceMag);
                //Body.AddBodyForce(Force * forceMag);
                if (!Body.IsActive)
                    Body.SetActive();
            }
            if (Torque != null && Torque != Vector3.Zero)
            {
                if (torqueMag > 0)
                {
                }
                Body.AddBodyTorque(Torque * torqueMag);
                if (!Body.IsActive)
                    Body.SetActive();
            }

        }

    }
}
