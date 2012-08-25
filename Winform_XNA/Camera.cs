using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Winform_XNA
{
    class Camera
    {
        public Matrix _projection;
        public Vector3 Position = new Vector3();
        public Vector3 PitchYawRoll = new Vector3(); // Named this way Becuase X,Y,Z = Pitch,Yaw,Roll when stored
        public Quaternion Orientation;
        public float Speed = 10;
        public float SpeedChangeRate = 1.2f;

        public Camera(Vector3 pos)
        {
            PitchYawRoll = Vector3.Zero;
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
                    Position,
                    Position + camRotation,
                    cameraRotatedUpVector);
            }
        }

        public Ray GetMouseRay(Vector2 mousePosition, Viewport viewport)
        {            
            Vector3 nearPoint = new Vector3(mousePosition, 0);
            Vector3 farPoint = new Vector3(mousePosition, 1);

            nearPoint = viewport.Unproject(nearPoint, _projection, RhsLevelViewMatrix, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, _projection, RhsLevelViewMatrix, Matrix.Identity);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
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
            PitchYawRoll.X = (PitchYawRoll.X + pitch);
            PitchYawRoll.Y = (PitchYawRoll.Y + yaw);

            //prevents camera from flipping over, Math.Pi/2 = 1.57f with rounding
            // I use 1.57f because Math.PI causes constant "flipping"
            PitchYawRoll = Vector3.Clamp(PitchYawRoll, new Vector3(-1.57f, float.MinValue, float.MinValue), new Vector3(1.57f, float.MaxValue, float.MaxValue));

            //Quaternion cameraChange =
            Orientation = 
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, PitchYawRoll.Y) *
            Quaternion.CreateFromAxisAngle(Vector3.UnitX, PitchYawRoll.X);
            //Quaternion.CreateFromAxisAngle(GetLevelCameraLhs.Right, -dY * .001f) *
            //Quaternion.CreateFromAxisAngle(Vector3.UnitY, -dX * .001f);
            //Orientation = Orientation * cameraChange;
        }
        public void SetOrientation(Matrix o)
        {
            Orientation = Quaternion.CreateFromRotationMatrix(o);
        }
        public void LookAtLocation(Vector3 location)
        {
            Orientation = Quaternion.CreateFromRotationMatrix(Matrix.Invert(Matrix.CreateLookAt(Position, location, Vector3.Up)));
        }
        public void LookToward(Vector3 direction)
        {
            LookAtLocation(Position + direction);
        }
    }
}
