using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Helper.Physics;
using Helper.Objects;

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
        public int id;
        public SortedList<int, ViewProfile> profiles = new SortedList<int,ViewProfile>();
        public Matrix view = Matrix.Identity;
        public Matrix _projection;
        // allows multiple Gobjects to be used by a camera for calculation, or reference points.
        public List<Gobject> Gobjects = new List<Gobject>();
        public Vector3 PitchYawRoll = new Vector3(); // Named this way Becuase X,Y,Z = Pitch,Yaw,Roll when stored
        
        public float Speed = .1f;
        public float SpeedChangeRate = 1.2f;

        public Quaternion Orientation;
        public Vector3 CurrentLookAt;
        public Vector3 CurrentPosition;
        
        public float fieldOfView = 45.0f;
        public float zoomRate = 1f;
        public float MinimumFieldOfView = 10;
        public float MaximumFieldOfView = 80;


        public Vector3 TargetPosition = new Vector3(); 
        public float positionLagFactor = 1.0f;        
        public Vector3 TargetLookAt;        
        public float lookAtLagFactor = .1f;

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
       
        public Ray GetMouseRay(Vector2 mousePosition, Viewport viewport)
        {            
            Vector3 nearPoint = new Vector3(mousePosition, 0);
            Vector3 farPoint = new Vector3(mousePosition, 1);

            /*nearPoint = viewport.Unproject(nearPoint, _projection, RhsLevelViewMatrix, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, _projection, RhsLevelViewMatrix, Matrix.Identity);*/
            nearPoint = viewport.Unproject(nearPoint, GetProjectionMatrix(), GetViewMatrix(), Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, GetProjectionMatrix(), GetViewMatrix(), Matrix.Identity);

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

        public void AdjustTargetPosition(Vector3 delta)
        {
            TargetPosition += delta * Speed;
        }

        public void SetCurrentOrientation(Quaternion o)
        {
            Orientation = o;
        }
        public void SetCurrentOrientation(Matrix o)
        {
            Orientation = Quaternion.CreateFromRotationMatrix(o);
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
        public virtual void AdjustTargetOrientationTo(float pitch, float yaw)
        {
        }
        public virtual void AdjustTargetOrientationBy(float pitch, float yaw)
        {
        }

        public void LookAtLocation(Vector3 location)
        {
            LookAtLocation(location, Vector3.Up);
        }
        public void LookAtLocation(Vector3 location, Vector3 up)
        {
            Orientation = Quaternion.CreateFromRotationMatrix(Matrix.Invert(Matrix.CreateLookAt(TargetPosition, location, up)));
            if (float.IsNaN(Orientation.W))
            {
                Orientation = Quaternion.Identity;
            }
        }
        public void LookInDirection(Vector3 direction, Vector3 up)
        {
            LookAtLocation(TargetPosition + direction, up);
        }
        public void LookInDirection(Vector3 direction)
        {
            LookInDirection(direction);
        }

        public void SetGobjectList(List<Gobject> gobs)
        {
            Gobjects = gobs;
        }

        public virtual Matrix GetViewMatrix()
        {
            Matrix camOrient = Matrix.CreateFromQuaternion(Orientation);
            Vector3 camForward = camOrient.Forward;
            return Matrix.CreateLookAt(
                CurrentPosition,
                CurrentPosition + camForward,
                camOrient.Up);
        }

        public virtual Matrix GetProjectionMatrix()
        {
            return _projection;
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
