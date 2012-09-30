using Helper.Camera.Cameras;
using Microsoft.Xna.Framework;

namespace Helper.Camera
{
    public class FreeCamera : BaseCamera
    {
        public FreeCamera() 
        {
            
            //AdjustTargetOrientation(-.07f, 0);
            TargetPosition = new Vector3(0, 30, 80);
            CurrentPosition = TargetPosition;
            TargetLookAt = new Vector3(0, 0, 0);
            CurrentLookAt = TargetLookAt;

            positionLagFactor = .07f;
        }

        public override Matrix GetViewMatrix()
        {
            return RhsLevelViewMatrix;
        }

        public override Matrix GetProjectionMatrix()
        {
            return _projection;
        }

        public override void Update()
        {
            UpdatePosition(); // unsure
            //UpdateLookAt();

        }


        public override void AdjustTargetOrientation(float pitch, float yaw)
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

        public override void MoveRight()
        {
            AdjustTargetPosition(LhsLevelViewMatrix.Right * .1f);
        }
        public override void MoveLeft()
        {
            AdjustTargetPosition(LhsLevelViewMatrix.Left * .1f);
        }
        public override void MoveForward()
        {
            AdjustTargetPosition(Vector3.Normalize(LhsLevelViewMatrix.Forward) * .1f);
        }
        public override void MoveBackward()
        {
            AdjustTargetPosition(LhsLevelViewMatrix.Backward * .1f);
        }
        public override void MoveDown()
        {
            AdjustTargetPosition(Matrix.Identity.Down * .1f);
        }
        public override void MoveUp()
        {
            AdjustTargetPosition(Matrix.Identity.Up * .1f);
        }

    }
}
