using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Helper.Physics;

namespace Helper.Camera.Cameras
{
    /*This is the base class for all cameras
     * A specifc camera class defines the behavior of the camera, but not the properties or attributes of a camera.
     * For example, a chase camera and free-look camera have very different behavior.
     * Meanwhile, a ViewProfile defines the properties or attributes for a specific camera.
     * For example, a first-person camera for a car and first-person camera for an airplane might need to have very different properties, but same behavior.
     * 
     * 
     * 
     * 
     */
    
    public class BaseCamera
    {
        public SortedList<int, ViewProfile> profiles = new SortedList<int,ViewProfile>();
        public Matrix view;
        // allows multiple Gobjects to be used by a camera for calculation, or reference points.
        public List<Gobject> Gobjects = new List<Gobject>();
        public Vector3 PitchYawRoll = new Vector3(); // Named this way Becuase X,Y,Z = Pitch,Yaw,Roll when stored
        
        public float Speed = .1f;
        public float SpeedChangeRate = 1.2f;

        public Quaternion Orientation;
        public Vector3 CurrentPosition;
        public Vector3 TargetPosition = new Vector3(); 
        public float positionLagFactor = 1.0f;

        public Vector3 CurrentLookAt;
        public Vector3 TargetLookAt;        
        public float lookAtLagFactor = .1f;

        public Matrix _projection;

        public float fieldOfView = 45.0f;
        public float zoomRate = 1f;
        public float MinimumFieldOfView = 10;
        public float MaximumFieldOfView = 80;

        public BaseCamera(Vector3 pos)
        {
            pos = Initialize(pos);
        }

        private Vector3 Initialize(Vector3 pos)
        {
            PitchYawRoll = Vector3.Zero;
            Orientation = Quaternion.Identity;

            TargetPosition = pos;
            CurrentPosition = TargetPosition;
            SetupProjection();
            return pos;
        }

        private void SetupProjection()
        {
            BoundFieldOfView();
            _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(fieldOfView),
                (float)GraphicsDeviceManager.DefaultBackBufferWidth / (float)GraphicsDeviceManager.DefaultBackBufferHeight,
                0.1f,
                5000.0f);
        }

        public BaseCamera()
        {
            Initialize(Vector3.Zero);
        }

        public void SetProfiles(SortedList<int, ViewProfile> vps)
        {
            profiles = vps;
            Update();
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

        public Vector3 HomePosition {get; set;}
        public void MoveHome()
        {
            TargetPosition = HomePosition;
            CurrentPosition = HomePosition;
        }

        public void UpdatePosition()
        {
            CurrentPosition += (TargetPosition - CurrentPosition) * positionLagFactor;
        }
        
        public void UpdateLookAt()
        {
            CurrentLookAt += (TargetLookAt - CurrentLookAt) * lookAtLagFactor;
            LookAtLocation(CurrentLookAt);
        }

        public Matrix LhsLevelViewMatrix
        {
            get
            {
                return Matrix.Invert(RhsLevelViewMatrix);
            }
        }

        public void AdjustTargetPosition(Vector3 delta)
        {
            TargetPosition += delta * Speed;
        }
        
        public void SetTargetOrientation(Matrix o)
        {
            Orientation = Quaternion.CreateFromRotationMatrix(o);
        }

        public void SetTargetOrientation(Quaternion q)
        {
            Orientation = q;
        }

        public virtual void IncreaseMovementSpeed()
        {
            Speed *= SpeedChangeRate;
        }
        public virtual void DecreaseMovementSpeed()
        {
            Speed /= SpeedChangeRate;
            System.Diagnostics.Debug.WriteLine("DecreaseSpeeD");
        }

        public virtual void MoveRight()
        {
        }
        public virtual void MoveLeft()
        {
        }
        public virtual void MoveForward()
        {
        }
        public virtual void MoveBackward()
        {
        }
        public virtual void MoveDown()
        {
        }
        public virtual void MoveUp()
        {
        }
        public virtual void AdjustTargetOrientation(float pitch, float yaw)
        {

        }

        public void LookAtLocation(Vector3 location)
        {
            Orientation = Quaternion.CreateFromRotationMatrix(Matrix.Invert(Matrix.CreateLookAt(TargetPosition, location, Vector3.Up)));
        }
        public void LookToward(Vector3 direction)
        {
            LookAtLocation(TargetPosition + direction);
        }

        /*NEW METHODS!!!*/

        public void SetGobjectList(List<Gobject> gobs)
        {
            Gobjects = gobs;
        }

        public virtual Matrix GetViewMatrix()
        {
            return Matrix.Identity;
        }

        public virtual Matrix GetProjectionMatrix()
        {
            return Matrix.Identity;
        }

        public virtual void Update()
        {
            UpdatePosition();
            UpdateLookAt();
        }

        public Gobject GetFirstGobject()
        {
            if (Gobjects == null)
                return null;
            if (Gobjects.Count == 0)
                return null;
            return Gobjects[0];
        }


        
        public virtual void ZoomOut()
        {
            fieldOfView -= zoomRate;
            SetupProjection();
        }

        public virtual void ZoomIn()
        {
            fieldOfView += zoomRate;
            SetupProjection();
            
        }

        private void BoundFieldOfView()
        {
            if (fieldOfView > MaximumFieldOfView)
                fieldOfView = MaximumFieldOfView;
            if (fieldOfView < MinimumFieldOfView)
                fieldOfView = MinimumFieldOfView;
        }
    }
}
