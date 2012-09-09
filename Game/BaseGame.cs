using System.Collections.Generic;
using Physics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.ComponentModel.Design;
using System;
using Microsoft.Xna.Framework;
using Physics.PhysicsObjects;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using Helper;

namespace Game
{
    public class BaseGame 
    {
        /* Class should delegate most Game processing such as:
         * content loading
         * handling input
         * updating physics
         * drawing
         */
        // Both the client and server need the game
        // Both the client and server's game can run physics
        // The clients should correct objects based on what the server tells it
        // Both the client and server need the communications package

        /*
         * Camera management?
         * network management?
         */

        #region Physics
        public Physics.PhysicsManager physicsManager;
        public static BaseGame Instance { get; private set; }
        #endregion

        /* Keymapping
         * Keep a list of possible bindings
         * each binding has a name / purpose, and an assigned key.
         * To set the binding, we need a special screen on which bindings are listed, input is monitored, and changes can be made.
         * save the binding somewhere outside the solution, per game, per user.
         * read in the binding per game, per user.
         * 
         */

        #region Content
        public ContentManager Content { get; private set; }
        Model terrainModel;
        Model planeModel;
        Model staticFloatObjects;
        Model carModel, wheelModel;
        Texture2D moon;
        Physics.Terrain terrain;
        PlaneObject planeObj;
        #endregion

        #region Graphics
        /// <summary>
        /// Gets an IServiceProvider containing our IGraphicsDeviceService.
        /// This can be used with components such as the ContentManager,
        /// which use this service to look up the GraphicsDevice.
        /// </summary>
        public ServiceContainer Services;
        /// <summary>
        /// Gets a GraphicsDevice that can be used to draw onto this control.
        /// </summary>
        public GraphicsDevice graphicsDevice;
        public delegate void myCallbackDelegate(Camera c, Matrix v, Matrix p);
        myCallbackDelegate UpdateCameraCallback;
        public Camera cam;
        public string name = "BaseGame";
        #endregion

        #region Input
        internal Input.InputManager inputManager;
        Timer tmrCamUpdate;
        #endregion

        #region Game
        public List<Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        public List<Gobject> newObjects; // This member is accessed from multiple threads and needs to be locked
        public Gobject currentSelectedObject;
        #endregion
        public Matrix view = Matrix.Identity;
        public Matrix proj = Matrix.Identity;


        enum CameraModes
        {
            Fixed,
            ObjectFirstPerson,
            ObjectChase,
            ObjectWatch
        }
        CameraModes cameraMode = CameraModes.Fixed;

        public BaseGame()
        {
            CommonInit(10, 10);
            //CommonInit(10, 15);
        }

        public BaseGame(int camUpdateInterval)
        {
            CommonInit(10, camUpdateInterval);
        }

        
        private void CommonInit(double physicsUpdateInterval, double cameraUpdateInterval)
        {
            graphicsDevice = null;
            gameObjects = new List<Gobject>();
            newObjects = new List<Gobject>();
            Instance = this;

            tmrCamUpdate = new Timer();
            tmrCamUpdate.Interval = cameraUpdateInterval;
            tmrCamUpdate.Elapsed +=new ElapsedEventHandler(tmrCamUpdate_Elapsed);
            tmrCamUpdate.AutoReset=true;
            tmrCamUpdate.Start();

            physicsManager = new Physics.PhysicsManager(ref gameObjects, ref newObjects, physicsUpdateInterval);
            inputManager = new Input.InputManager();
        }

        void  tmrCamUpdate_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (cam == null)
                return;

            PreUpdateCameraCallback();
            if (UpdateCameraCallback == null)
                return;
            GetCameraViewProjection();
 	        UpdateCameraCallback(cam, view, proj);
        }
        
        public virtual void GetCameraViewProjection()
        {
            Vector3 bodyPosition = new Vector3(0,0,0);
            if(currentSelectedObject!=null)
                bodyPosition= currentSelectedObject.BodyPosition();
            switch (cameraMode)
            {
                case CameraModes.Fixed:
                    view = cam.RhsLevelViewMatrix;
                    proj = cam._projection;
                    break;
                case CameraModes.ObjectFirstPerson:
                    if (currentSelectedObject == null)
                        return;
                    Matrix bodyOrientation = currentSelectedObject.BodyOrientation();
                    //cam.SetOrientation(currentSelectedObject.BodyOrientation());
                    Matrix forward = Matrix.CreateFromAxisAngle(bodyOrientation.Up, (float)-Math.PI/2);
                    
                    cam.SetOrientation(bodyOrientation * forward);
                    Matrix ForwardOrientation = bodyOrientation * forward;
                    //cam.positionLagFactor = 1.0f;

                    Vector3 firstPersonOffsetPosition = new Vector3(-.45f, 1.4f, .05f); // To the driver's seat in car coordinates!
                    Vector3 firstTransRef = Vector3.Transform(firstPersonOffsetPosition, ForwardOrientation);
                    cam.CurrentPosition = bodyPosition + firstTransRef;

                    view = cam.RhsViewMatrix;
                    proj = cam._projection;
                    break;
                case CameraModes.ObjectChase:
                    if (currentSelectedObject == null)
                        return;

                    // bodyPosition is the physical location of the body
                    // the location of where it's headed
                    Vector3 WhereItsHeaded = bodyPosition + currentSelectedObject.BodyVelocity()*2;
                    // a vector point toward direction of travel
                    Vector3 Direction = (WhereItsHeaded - bodyPosition);
                    Direction.Normalize();
                    Direction *= 10f;
                    Vector3 WhereItCameFrom = bodyPosition - (Direction);
                    WhereItCameFrom += new Vector3(0, 3, 0);
                    cam.positionLagFactor = .2f;
                    //Vector3 ThirdPersonOffsetPosition = new Vector3(-10, 3, 0);
                    //Vector3 TransRef = Vector3.Transform(ThirdPersonOffsetPosition, currentSelectedObject.BodyOrientation());
                    //cam.TargetPosition = TransRef + bodyPosition;
                    cam.TargetPosition = WhereItCameFrom;
                    //cam.LookAtLocation(bodyPosition);
                    cam.TargetLookAt = WhereItsHeaded;

                    view = cam.RhsViewMatrix;
                    proj = cam._projection;
                    break;
                case CameraModes.ObjectWatch:
                    if (currentSelectedObject == null)
                        return;
                    cam.LookAtLocation(currentSelectedObject.BodyPosition());
                    view = cam.RhsLevelViewMatrix;
                    proj = cam._projection;
                     
                    break;
                default:
                    view = Matrix.Identity;
                    proj = Matrix.Identity;
                    break;
            }
        }

        public virtual void PreUpdateCameraCallback()
        {
            cam.UpdatePosition();
            if(cameraMode != CameraModes.Fixed)
                cam.UpdateLookAt();
        }
        
        public void Initialize(ServiceContainer services, GraphicsDevice gd, myCallbackDelegate updateCamCallback)
        {
            Services = services;
            graphicsDevice = gd;
            UpdateCameraCallback = updateCamCallback;
            try
            {

                InitializeContent();
                InitializeCameras();
                InitializeEnvironment();
                InitializeInputs();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }

        }

        public virtual void InitializeCameras()
        {
            cam = new Camera(new Vector3(0, 0, 50));
            cam.AdjustOrientation(-.07f, 0);
            cam.positionLagFactor = .07f;
        }

        public virtual void InitializeInputs()
        {
            inputManager.AddWatch(new Input.KeyWatch(Keys.W, false, false, false, Input.KeyWatch.keyEvent.Down, CameraMoveForward));
            inputManager.AddWatch(new Input.KeyWatch(Keys.A, false, false, false, Input.KeyWatch.keyEvent.Down, CameraMoveLeft));
            inputManager.AddWatch(new Input.KeyWatch(Keys.S, false, false, false, Input.KeyWatch.keyEvent.Down, CameraMoveBackward));
            inputManager.AddWatch(new Input.KeyWatch(Keys.D, false, false, false, Input.KeyWatch.keyEvent.Down, CameraMoveRight));
            inputManager.AddWatch(new Input.KeyWatch(Keys.Q, false, false, false, Input.KeyWatch.keyEvent.Down, CameraMoveSpeedIncrease));
            inputManager.AddWatch(new Input.KeyWatch(Keys.Z, false, false, false, Input.KeyWatch.keyEvent.Down, CameraMoveSpeedDecrease));
            inputManager.AddWatch(new Input.KeyWatch(Keys.C, false, false, false, Input.KeyWatch.keyEvent.Pressed, CameraModeCycle));
        }

        public void CameraMoveForward()
        {
            cam.MoveForward();
        }
        public void CameraMoveBackward()
        {
            cam.MoveBackward();
        }
        public void CameraMoveLeft()
        {
            cam.MoveLeft();
        }
        public void CameraMoveRight()
        {
            cam.MoveRight();
        }
        
        public void CameraMoveSpeedIncrease()
        {
            cam.IncreaseSpeed();
        }

        public void CameraMoveSpeedDecrease()
        {
            cam.DecreaseSpeed();
        }


        public void AdjustCameraOrientation(float pitch, float yaw)
        {
            cam.AdjustOrientation(pitch, yaw);
        }

        public void CameraModeCycle()
        {
            switch (cameraMode)
            {
                case CameraModes.Fixed:
                    cameraMode = CameraModes.ObjectFirstPerson;
                    break;
                case CameraModes.ObjectFirstPerson:
                    cameraMode = CameraModes.ObjectChase;
                    break;
                case CameraModes.ObjectChase:
                    cameraMode = CameraModes.ObjectWatch;
                    break;
                case CameraModes.ObjectWatch:
                    cameraMode = CameraModes.Fixed;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Should contain all model, and texture loading
        /// </summary>
        public virtual void InitializeContent()
        {
            Content = new ContentManager(Services, "content");

            try
            {
                //cubeModel = Content.Load<Model>("Cube");
                //sphereModel = Content.Load<Model>("Sphere");
                moon = Content.Load<Texture2D>("Moon");
                staticFloatObjects = Content.Load<Model>("StaticMesh");
                planeModel = Content.Load<Model>("plane");
                terrainModel = Content.Load<Model>("terrain");
                carModel = Content.Load<Model>("car");
                wheelModel = Content.Load<Model>("wheel");
                //debugFont = Content.Load<SpriteFont>("DebugFont");
            }
            catch (Exception E)
            {
            }
        }
        private void LoadModel(Model m, string name)
        {
            try
            {
                m = Content.Load<Model>(name);
            }
            catch (Exception E)
            {
            }
        }

        /// <summary>
        /// Should contain scene and object initialization
        /// </summary>
        public virtual void InitializeEnvironment()
        {            
            
            bool useCustomTerrain = false;

            if (useCustomTerrain)
            {
                try
                {
                    terrain = new Terrain(new Vector3(0, -15, 0), // position
                        //new Vector3(100f, .1f, 100f),  // X with, possible y range, Z depth 
                                            new Vector3(50f, .55f, 50f),  // X with, possible y range, Z depth 
                                            100, 100, graphicsDevice, moon);

                    newObjects.Add(terrain);
                }
                catch (Exception E)
                {
                }
            }
            else
            {
                try
                {
                    // some video cards can't handle the >16 bit index type of the terrain
                    
                    HeightmapObject heightmapObj = new HeightmapObject(terrainModel, Vector2.Zero, new Vector3(0, 0, 0));
                    newObjects.Add(heightmapObj);
                }
                catch (Exception E)
                {
                    // if that happens just create a ground plane 
                    planeObj = new PlaneObject(planeModel, 0.0f, new Vector3(0, -15, 0));
                    newObjects.Add(planeObj);
                }
            }
        }

        public void SelectGameObject(Gobject go)
        {
            if (go == null)
                return;
            if (currentSelectedObject != null)
                currentSelectedObject.Selected = false;
            currentSelectedObject = go;
            currentSelectedObject.Selected = true;
            //objectCam.TargetPosition = currentSelectedObject.Position;
        }

        public void SetSimFactor(float value)
        {
            physicsManager.SetSimFactor(value);
        }


        /// <summary>
        /// override SetNominalInputState to set nominal states (like zero acceleration on a car)
        /// </summary>
        public void ProcessInput()
        {
            SetNominalInputState();
            inputManager.Update();
        }

        public virtual void SetNominalInputState()
        {
        }
    }
}
