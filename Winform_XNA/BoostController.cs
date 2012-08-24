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
        private Body body;
        public Vector3 force = Vector3.Up*15;
        public Vector3 torque = Vector3.Zero;

        public BoostController()
        {
        }

        public void Initialize(Body b)
        {
            //EnableController();
            this.body = b;
        }

        public override void UpdateController(float dt)
        {
            if (body == null)
                return;

            if (force != null && force != Vector3.Zero)
            {
                body.AddBodyForce(force);
                if (!body.IsActive)
                    body.SetActive();
            }
            if (torque != null && torque != Vector3.Zero)
            {
                body.AddBodyTorque(torque);
                if (!body.IsActive)
                    body.SetActive();
            }

        }

    }
}
