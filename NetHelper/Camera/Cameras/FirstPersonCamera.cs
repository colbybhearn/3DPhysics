
using Microsoft.Xna.Framework;
using System;
using Helper.Physics;

namespace Helper.Camera.Cameras
{
    public class FirstPersonCamera : BaseCamera
    {
        public FirstPersonCamera()
        {
            positionLagFactor = .1f;
            lookAtLagFactor = .1f;
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
            Matrix bodyOrientation = gob.BodyOrientation();
            Vector3 bodyPosition = gob.BodyPosition();

            // get the correction value from the profile
            //float ObjectYaxisCorrectionValue = (float)-Math.PI / 2; // for the car
            float ObjectYaxisCorrectionValue = 0; // for the airplane
            // create a correction matrix for the orientation
            Matrix forward = Matrix.CreateFromAxisAngle(bodyOrientation.Up, ObjectYaxisCorrectionValue);
            // Create a corrected matrix for orientation
            Matrix AdjustedOrientation = bodyOrientation * forward;
            // update the orientation
            SetTargetOrientation(AdjustedOrientation);
            
            // get the correction value from the profile
            Vector3 firstPersonOffsetPosition = new Vector3(-.45f, 1.4f, .05f); // To the driver's seat in car coordinates!
            // create a correction vector for the position
            Vector3 firstTransRef = Vector3.Transform(firstPersonOffsetPosition, AdjustedOrientation);
            // update the position
            CurrentPosition = bodyPosition + firstTransRef;

        }
    }
}
