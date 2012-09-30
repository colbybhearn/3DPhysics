
using Helper.Physics;
using Microsoft.Xna.Framework;
using System;
namespace Helper.Camera.Cameras
{
    public class ChaseCamera : BaseCamera
    {
        
        public ChaseCamera()
        {
            positionLagFactor = .25f;
            lookAtLagFactor = .2f;
        }

        public override Matrix GetViewMatrix()
        {
            return RhsViewMatrix;
        }

        public override Matrix GetProjectionMatrix()
        {            
            return _projection;
        }

        public override void Update()
        {
            base.Update();
            Gobject gob = GetFirstGobject();
            if (gob == null) return;
            // bodyPosition is the physical location of the body
            Vector3 bodyPosition = gob.BodyPosition();
            Matrix bodyOrientation = gob.BodyOrientation();
            try
            {
                // the location of where it's headed
                Vector3 ObjectDirection = gob.BodyVelocity() * 2; // this * 2 value is pointless I think

                if (ObjectDirection.Length() < 2) // this may be here just to prevent slow velocities from making a stalker camera
                    ObjectDirection = gob.BodyOrientation().Right;

                Vector3 WhereItsHeaded = bodyPosition + ObjectDirection;
                
                // a vector pointing toward the direction of travel
                //Vector3 Direction = (WhereItsHeaded - bodyPosition);
                ObjectDirection.Normalize();
                ObjectDirection *= 10f; // this may need to be adjustable per object (planes go faster than cars)
                Vector3 WhereItCameFrom = bodyPosition - (ObjectDirection);

                Vector3 offset = new Vector3(0, 2, 0);
                if(profiles.ContainsKey(gob.Asset))
                    offset = profiles[gob.Asset].PositionOffset;
                offset = Vector3.Transform(offset, bodyOrientation); 
                // get the correction value from the profile
                WhereItCameFrom += offset; 
                TargetPosition = WhereItCameFrom; // this line caused a problem at one time.
                TargetLookAt = WhereItsHeaded;
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }
    }
}
