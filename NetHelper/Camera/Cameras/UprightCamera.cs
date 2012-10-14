using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Helper.Camera.Cameras
{
    public class UprightCamera : BaseCamera
    {
        public Vector3 TargetPosition = new Vector3(); 
        public float positionLagFactor = 1.0f;        
        public Vector3 TargetLookAt;        
        public float lookAtLagFactor = .1f;

        bool useLhs;
        public UprightCamera()
        {
        }
        
        public Matrix RhsLevelViewMatrix
        {
            get
            {
                Vector3 camRotation = Matrix.CreateFromQuaternion(Orientation).Forward;
                // Side x camRotation gives the correct Up vector WITHOUT roll, if you do -Z,0,X instead, you will be upsidedown
                // There is still an issue when nearing a "1" in camRotation in the positive or negative Y, in that it rotates weird,
                // This does not appear to be related to the up vector.
                Vector3 side = new Vector3(camRotation.Z, 0, -camRotation.X);
                Vector3 up = Vector3.Cross(camRotation, side);
                return Matrix.CreateLookAt(
                    CurrentPosition,
                    CurrentPosition + camRotation,
                    up);
            }
        }

        public Matrix LhsLevelViewMatrix
        {
            get
            {
                return Matrix.Invert(RhsLevelViewMatrix);
            }
        }

        public Matrix RhsViewMatrix
        {
            get
            {
                Vector3 camRotation = Matrix.CreateFromQuaternion(Orientation).Forward;
                // Side x camRotation gives the correct Up vector WITHOUT roll, if you do -Z,0,X instead, you will be upsidedown
                // There is still an issue when nearing a "1" in camRotation in the positive or negative Y, in that it rotates weird,
                // This does not appear to be related to the up vector.
                Vector3 cameraRotatedUpVector = Vector3.Transform(Vector3.Up, Orientation);
                return Matrix.CreateLookAt(
                    CurrentPosition,
                    CurrentPosition + camRotation,
                    cameraRotatedUpVector);
            }
        }


        public override Matrix GetViewMatrix()
        {
            if (useLhs)
            {
                return LhsLevelViewMatrix;
            }
            else
            {
                return RhsLevelViewMatrix;
            }
            
        }


    }
}
