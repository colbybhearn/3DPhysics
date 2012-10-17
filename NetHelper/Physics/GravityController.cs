using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using Microsoft.Xna.Framework;

namespace Helper.Physics
{
    class GravityController : Controller
    {
        double gravityForce;
        Vector3 position;
        double radius;

        public GravityController(Vector3 pos, double r, double g)
        {
            position = pos;
            radius = r;
            gravityForce = g;
        }


        public override void UpdateController(float dt)
        {
            foreach (Body b in PhysicsSystem.CurrentPhysicsSystem.Bodies)
            {
                if (b.ApplyGravity == false) // Ignore those who arn't affected by gravity
                    continue;

                double distance = (b.Position - position).Length();
                if (distance < 1)
                    distance = 1;
                else
                    distance /= radius; // Normalize distance, so that "1 radius" away from center gets "Full" gravity (full gravity on surface)

                Vector3 forceDirection = position - b.Position;
                Vector3 force = forceDirection / (float)(distance*distance);
                // Newton Gravity Equation
                // F = G * [(m1 * m2) / r^2]
                // m1 = mass of one object
                // m2 = mass of other
                // r^2 = distance^2
                // G = gravity constant
                // I take G,m1,m2 as GravityForce constant (ignore mass of the other object, its "small" compared to the planet)
                // Thus I use r^2 as distance from other object to center of planet, normalized by the radius of the planet (surface is "1" away, so you get G force)


                b.AddWorldForce(force * b.Mass);

            }
        }
    }
}
