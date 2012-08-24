using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Winform_XNA
{
    class Camera
    {
        public Matrix _projection;
        public Vector3 Position = new Vector3();
        public Quaternion Orientation;
        public float Speed = 10;
        public float SpeedChangeRate = 1.2f;

        public Camera(Vector3 pos)
        {
            Orientation = Quaternion.Identity;
            Position = pos;
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float)GraphicsDeviceManager.DefaultBackBufferWidth / (float)GraphicsDeviceManager.DefaultBackBufferHeight,
                0.1f,
                5000.0f);
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
                    Position,
                    Position + camRotation,
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

        public void IncreaseSpeed()
        {
            Speed *= SpeedChangeRate;
        }

        public void DecreaseSpeed()
        {
            Speed /= SpeedChangeRate;
        }

        private void AdjustPosition(Vector3 delta)
        {
            Position += delta * Speed;
        }
        public void MoveRight()
        {
            AdjustPosition(LhsLevelViewMatrix.Right);
        }
        public void MoveLeft()
        {
            AdjustPosition(LhsLevelViewMatrix.Left);
        }
        public void MoveForward()
        {
            AdjustPosition(Vector3.Normalize(LhsLevelViewMatrix.Forward) * .1f);
        }
        public void MoveBackward()
        {
            AdjustPosition(LhsLevelViewMatrix.Backward * .1f);
        }
        public void AdjustOrientation(float pitch, float yaw)
        {
            Quaternion cameraChange =
            Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch) *
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
            //Quaternion.CreateFromAxisAngle(GetLevelCameraLhs.Right, -dY * .001f) *
            //Quaternion.CreateFromAxisAngle(Vector3.UnitY, -dX * .001f);
            Orientation = Orientation * cameraChange;
        }
    }
}
