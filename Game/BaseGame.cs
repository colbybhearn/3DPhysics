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
using Input;
using Multiplayer;

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
        public bool debug = false;
        #endregion

        #region Input
        internal Input.InputManager inputManager;
        Timer tmrCamUpdate;
        Timer tmrUpdateServer;
        #endregion

        #region Game
        public SortedList<int, Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        public SortedList<int, Gobject> newObjects; // This member is accessed from multiple threads and needs to be locked
        public Gobject currentSelectedObject;
        #endregion

        public Matrix view = Matrix.Identity;
        public Matrix proj = Matrix.Identity;
        KeyMap keyMap;

        enum CameraModes
        {
            Fixed,
            ObjectFirstPerson,
            ObjectChase,
            ObjectWatch
        }
        CameraModes cameraMode = CameraModes.Fixed;

        #region Communication
        public enum CommTypes
        {
            Client,
            Server
        }
        public CommTypes CommType;
        public CommClient commClient;
        public CommServer commServer;
        #endregion

        #region Events
        public event Handlers.StringEH ChatMessageReceived;
        #endregion

        #region Multiplayer
        public List<int> clientControlledObjects = new List<int>();
        #endregion

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
            gameObjects = new SortedList<int, Gobject>();
            newObjects = new SortedList<int, Gobject>();
            Instance = this;

            tmrCamUpdate = new Timer();
            tmrCamUpdate.Interval = cameraUpdateInterval;
            tmrCamUpdate.Elapsed +=new ElapsedEventHandler(tmrCamUpdate_Elapsed);
            tmrCamUpdate.AutoReset=true;
            tmrCamUpdate.Start();

            tmrUpdateServer = new Timer();
            tmrUpdateServer.Interval = 50;
            tmrUpdateServer.Elapsed += new ElapsedEventHandler(tmrUpdateServer_Elapsed);
            tmrUpdateServer.AutoReset = true;
            tmrUpdateServer.Start();

            physicsManager = new Physics.PhysicsManager(ref gameObjects, ref newObjects, physicsUpdateInterval);
                       
        }

        void tmrUpdateServer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(CommType == CommTypes.Client)
            {
                foreach(int id in clientControlledObjects)
                {
                    if(!gameObjects.ContainsKey(id))
                        continue;
                    Gobject go = gameObjects[id];
                    commClient.SendObjectUpdate(go.ID, go.Position, go.BodyOrientation(), go.BodyVelocity());
                }
            }
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
            keyMap = GetDefaultKeyMap();
        }

        public virtual void InitializeMultiplayer()
        {
        }

        public virtual List<KeyBinding> GetDefaultKeyBindings()
        {
            List<KeyBinding> defaults = new List<KeyBinding>();
            defaults.Add(new KeyBinding("CameraMoveForward", Keys.W, false, false, false, Input.KeyEvent.Down, CameraMoveForward));
            defaults.Add(new KeyBinding("CameraMoveLeft", Keys.A, false, false, false, Input.KeyEvent.Down, CameraMoveLeft));
            defaults.Add(new KeyBinding("CameraMoveBackward", Keys.S, false, false, false, Input.KeyEvent.Down, CameraMoveBackward));
            defaults.Add(new KeyBinding("CameraMoveRight", Keys.D, false, false, false, Input.KeyEvent.Down, CameraMoveRight));
            defaults.Add(new KeyBinding("CameraMoveSpeedIncrease", Keys.Q, false, false, false, Input.KeyEvent.Down, CameraMoveSpeedIncrease));
            defaults.Add(new KeyBinding("CameraMoveSpeedDecrease", Keys.Z, false, false, false, Input.KeyEvent.Down, CameraMoveSpeedDecrease));
            defaults.Add(new KeyBinding("CameraMoveCycle", Keys.C, false, false, false, Input.KeyEvent.Pressed, CameraModeCycle));
            defaults.Add(new KeyBinding("ToggleDebugInfo", Keys.F1, false, false, false, Input.KeyEvent.Pressed, ToggleDebugInfo));
            return defaults;
        }

        // this should be overriden in every game for the default keys
        public virtual KeyMap GetDefaultKeyMap()
        {
            return new KeyMap(this.name, GetDefaultKeyBindings());
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

        public void ToggleDebugInfo()
        {
            debug = !debug;
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

                    newObjects.Add(terrain.ID, terrain);
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
                    newObjects.Add(heightmapObj.ID, heightmapObj);
                }
                catch (Exception E)
                {
                    // if that happens just create a ground plane 
                    planeObj = new PlaneObject(planeModel, 0.0f, new Vector3(0, -15, 0));
                    newObjects.Add(planeObj.ID, planeObj);
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

        public virtual void EditSettings()
        {
            inputManager.EditSettings();
        }

        
        public void ServerObjectRequest(int clientId, string asset, out int objectId)
        {
            
            objectId = AddOwnedObject(clientId, asset);
            commServer.SendObjectResponsePacket(clientId, objectId, asset);
            
        }
        SortedList<int, List<int>> ClientObjectIds = new SortedList<int, List<int>>();
        /// <summary>
        /// Server adds an object and associates it with its owning client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        private int AddOwnedObject(int clientId, string asset)
        {
            int objectid = GetAvailableObjectId();
            // setup dual reference for flexible and speedy accesses, whether by objectID, or by clientId 
            if (!ClientObjectIds.ContainsKey(clientId))
                ClientObjectIds.Add(clientId, new List<int>());
            // this is the list of objects owned by client ClientID
            List<int> objects = ClientObjectIds[clientId];
            objects.Add(objectid);
            
            AddNewObject(objectid, asset);

            return objectid;
        }


        //SortedList<string, List<>
        /// <summary>
        /// allows flexibility with that is added, accoding to the asset requested
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public virtual void AddNewObject(int objectid, string asset)
        {
            // all we have here is the name.
            // that tells us a model to load
            // but we don't know the primitives or 
            // if we were in CarObject, we would know the model, and have specific logic
            
        }
        

        private int GetAvailableObjectId()
        {
            int id=1;
            bool found =true;
            while(found)
            {
                if (gameObjects.ContainsKey(id) || newObjects.ContainsKey(id))
                    id++;
                else
                    found = false;
            }

            return id;
        }

        #region Communication Methods

        // CLIENT only
        public virtual void ConnectToServer(string ip, int port, string alias)
        {
            CommType = CommTypes.Client;
            commClient = new CommClient(ip, port, alias);
            InitializeMultiplayer(CommType);
            commClient.Connect();
        }

        public virtual void InitializeMultiplayer(CommTypes CommType)
        {
            switch (CommType)
            {
                case CommTypes.Client:
                    commClient.ChatMessageReceived += new Handlers.StringEH(commClient_ChatMessageReceived);
                    commClient.ObjectRequestResponseReceived += new Handlers.ObjectRequestResponseEH(commClient_ObjectRequestResponseReceived);
                    break;
                case CommTypes.Server:
                    commServer.ClientConnected += new Handlers.StringEH(commServer_ClientConnected);
                    commServer.ChatMessageReceived += new Handlers.StringEH(commServer_ChatMessageReceived);
                    //commServer.ObjectRequestReceived += new Handlers.ObjectRequestEH(commServer_ObjectRequestReceived);
                    commServer.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commServer_ObjectUpdateReceived);
                    break;
                default:
                    break;
            }
        }


        
        void commClient_ChatMessageReceived(string s)
        {
            CallChatMessageReceived(s);
        }
        void commClient_ObjectRequestResponseReceived(int i, string asset)
        {
            // MINE!
            clientControlledObjects.Add(i);
            ProcessObjectRequestResponse(i, asset);
        }

        public virtual void ProcessObjectRequestResponse(int i, string asset)
        {
        }
        void commServer_ObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            if(!gameObjects.ContainsKey(id))
                return;
            Gobject go = gameObjects[id];
            //go.Body.MoveTo(pos);
            //go.SetPosition(pos);
            go.SetOrientation(orient);
            go.SetVelocity(vel);
        }

        // COMMON
        private void CallChatMessageReceived(string msg)
        {
            if (ChatMessageReceived == null)
                return;
             ChatMessageReceived(msg);
        }
        public virtual void ProcessChatMessage(string s)
        {
        }
        public void SendChatPacket(string msg)
        {
            if (CommType == CommTypes.Client)
                commClient.SendChatPacket(msg);
            else
                commServer.SendChatPacket(msg);
        }

        // SERVER only
        public void ListenForClients(int port)
        {
            CommType = CommTypes.Server;
            commServer = new CommServer(port);
            InitializeMultiplayer(CommType);
            commServer.Start();
            
        }
        void commServer_ObjectRequestReceived(int clientId, string asset)
        {
            
        }
        void commServer_ChatMessageReceived(string s)
        {
            ProcessChatMessage(s);
        }
        void commServer_ClientConnected(string s)
        {
            ProcessClientConnected(s);
        }

        public virtual void ProcessClientConnected(string msg)
        {            
            CallClientConnected(msg);
        }

        public event Helper.Handlers.StringEH ClientConnected;
        private void CallClientConnected(string msg)
        {
            if (ClientConnected == null)
                return;
            ClientConnected(msg);
            
        }
        /// <summary>
        /// the server received an object request
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="asset"></param>
        public virtual int ProcessObjectRequest(int clientId, string asset)
        {
            int objectId = -1;
            ServerObjectRequest(clientId, asset, out objectId);
            return objectId;
        }
        
        #endregion
    }
}
