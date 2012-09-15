using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Timers;
using Helper;
using Helper.Multiplayer.Packets;
using Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Multiplayer;
using Physics;
using Physics.PhysicsObjects;

namespace Game
{
    public class BaseGame 
    {

        #region Physics
        public Physics.PhysicsManager physicsManager;
        public static BaseGame Instance { get; private set; }
        #endregion

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

        internal Chat ChatManager;
        internal SpriteFont chatFont;
        #endregion

        #region Game
        public SortedList<int, Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        public SortedList<int, Gobject> newObjects; // This member is accessed from multiple threads and needs to be locked
        public Gobject currentSelectedObject;
        #endregion
        
        public Matrix view = Matrix.Identity;
        public Matrix proj = Matrix.Identity;
        KeyMap keyMap;
        internal List<ObjectUpdatePacket> physicsUpdateList = new List<ObjectUpdatePacket>();

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
        public event Handlers.StringStringEH ChatMessageReceived;
        #endregion

        #region Multiplayer
        public List<int> clientControlledObjects = new List<int>();
        public List<object> MultiplayerUpdateQueue = new List<object>();
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

            //tmrUpdateServer = new Timer();
            //tmrUpdateServer.Interval = 20;
            //tmrUpdateServer.Elapsed += new ElapsedEventHandler(tmrUpdateMultiplayer_Elapsed);
            //tmrUpdateServer.AutoReset = true;
            //tmrUpdateServer.Start();

            physicsManager = new Physics.PhysicsManager(ref gameObjects, ref newObjects, physicsUpdateInterval);
            physicsManager.PreIntegrate += new Handlers.voidEH(physicsManager_PreIntegrate);
            physicsManager.PostIntegrate += new Handlers.voidEH(physicsManager_PostIntegrate);
        }

        void physicsManager_PreIntegrate()
        {
            if (CommType == CommTypes.Client)
            {
                lock (gameObjects)
                {
                    foreach (int i in clientControlledObjects)
                    {
                        if (!gameObjects.ContainsKey(i))
                            continue;
                        Gobject go = gameObjects[i];
                        if (!go.actionManager.actionApplied)
                            continue;
                        object[] vals = go.actionManager.GetActionValues();
                        
                        commClient.SendObjectAction(go.ID, vals);
                    }
                }

                System.Diagnostics.Trace.WriteLine("MultiplayerQueue:" + MultiplayerUpdateQueue.Count);
                //MultiplayerUpdateQueue
                while (MultiplayerUpdateQueue.Count > 0)
                {
                    ObjectUpdatePacket oup = MultiplayerUpdateQueue[0] as ObjectUpdatePacket;
                    if (!gameObjects.ContainsKey(oup.objectId))
                    {
                        AddNewObject(oup.objectId, oup.assetName);
                        continue;
                    }
                    Gobject go = gameObjects[oup.objectId];
                    go.MoveTo(oup.position, oup.orientation);
                    go.SetVelocity(oup.velocity);
                    MultiplayerUpdateQueue.RemoveAt(0);
                }

            }
            else if (CommType == CommTypes.Server)
            {
                while (MultiplayerUpdateQueue.Count>0)
                {
                    ObjectActionPacket oap = MultiplayerUpdateQueue[0] as ObjectActionPacket;
                    if (!gameObjects.ContainsKey(oap.objectId))
                        continue;
                    Gobject go = gameObjects[oap.objectId];
                    go.actionManager.ProcessActionValues(oap.actionParameters);
                    MultiplayerUpdateQueue.RemoveAt(0);
                }
            }

            //lock (gameObjects)
            //{
            //    foreach (ObjectUpdatePacket p in physicsUpdateList)
            //    {
            //        if (!gameObjects.ContainsKey(p.objectId))
            //            return;
            //        Gobject go = gameObjects[p.objectId];

            //        //go.SetOrientation(orient);

            //        go.SetVelocity(p.velocity);
            //        // angular velocity

            //    }
            //}
        }

        void physicsManager_PostIntegrate()
        {
            if (CommType == CommTypes.Client)
            {
                
            }
            else if (CommType == CommTypes.Server)
            {
                foreach(Gobject go in gameObjects.Values)
                {
                    if(!go.isMoveable)
                        continue;

                    ObjectUpdatePacket oup = new ObjectUpdatePacket(go.ID, go.Asset, go.BodyPosition(), go.BodyOrientation(), go.BodyVelocity());
                    commServer.BroadcastObjectUpdate(oup);
                }
            }
        }

        void tmrUpdateMultiplayer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CommType == CommTypes.Client)
            {
                // update the server about the objects this client controls
                foreach (int id in clientControlledObjects)
                {
                    if (!gameObjects.ContainsKey(id))
                        continue;
                    Gobject go = gameObjects[id];
                    commClient.SendObjectUpdate(go.ID, go.Position, go.BodyOrientation(), go.BodyVelocity());
                }
            }
            else
            {
                if (commServer == null)
                    return;
                foreach (Gobject go in gameObjects.Values)
                {
                    if (!go.isMoveable)
                        continue;
                    if (go.ID == 1)
                    {
                    }
                    // update all clients about all objects!
                    // tell them what kind of model this is by asset name
                    commServer.BroadcastObjectUpdate(new ObjectUpdatePacket(go.ID, go.Asset, go.BodyPosition(), go.BodyOrientation(), go.BodyVelocity()));
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

        #region Camera Manipulation
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
        #endregion

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
                    planeObj = new PlaneObject(planeModel, 0.0f, new Vector3(0, -15, 0), "");
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

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            
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

        #region Common to Server and Client

        public virtual void InitializeMultiplayer(CommTypes CommType)
        {
            switch (CommType)
            {
                case CommTypes.Client:
                    commClient.ChatMessageReceived += new Handlers.StringStringEH(commClient_ChatMessageReceived);
                    commClient.ObjectRequestResponseReceived += new Handlers.ObjectRequestResponseEH(commClient_ObjectRequestResponseReceived);
                    commClient.ObjectActionReceived += new Handlers.ObjectActionEH(commClient_ObjectActionReceived);
                    commClient.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commClient_ObjectUpdateReceived);
                    break;
                case CommTypes.Server:
                    commServer.ClientConnected += new Handlers.StringEH(commServer_ClientConnected);
                    commServer.ChatMessageReceived += new Handlers.StringStringEH(commServer_ChatMessageReceived);
                    //commServer.ObjectRequestReceived += new Handlers.ObjectRequestEH(commServer_ObjectRequestReceived);
                    commServer.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commServer_ObjectUpdateReceived);
                    commServer.ObjectActionReceived += new Handlers.ObjectActionEH(commServer_ObjectActionReceived);
                    break;
                default:
                    break;
            }
        }

        void commClient_ObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            MultiplayerUpdateQueue.Add(new Helper.Multiplayer.Packets.ObjectUpdatePacket(id, asset, pos, orient, vel));
        }

        // COMMON
        private void CallChatMessageReceived(string msg, string player)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(msg, player);
        }
        public virtual void ProcessChatMessage(string m, string p)
        {
        }
        public void SendChatPacket(ChatMessage msg)
        {
            if (CommType == CommTypes.Client)
            {
                if (commClient != null)
                    commClient.SendChatPacket(msg.Message, msg.Owner);
            }
            else
            {
                if (commServer != null)
                    commServer.SendChatPacket(msg.Message, msg.Owner);
            }
        }
        #endregion

        #region Client Side
        // CLIENT only
        public virtual void ConnectToServer(string ip, int port, string alias)
        {
            CommType = CommTypes.Client;
            commClient = new CommClient(ip, port, alias);
            InitializeMultiplayer(CommType);
            commClient.Connect();
        }

        void commClient_ChatMessageReceived(string m, string p)
        {
            CallChatMessageReceived(m, p);
        }
        /// <summary>
        /// CLIENT SIDE
        /// The client has received an Object Action packet and it needs to be processed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        void commClient_ObjectActionReceived(int id, object[] parameters)
        {
            // TODO, fill in
        }

        /// <summary>
        /// CLIENT SIDE
        /// The client has received a response back from the server about the object the client requested
        /// </summary>
        /// <param name="i"></param>
        /// <param name="asset"></param>
        void commClient_ObjectRequestResponseReceived(int i, string asset)
        {
            // MINE!
            clientControlledObjects.Add(i);
            ProcessObjectRequestResponse(i, asset);
        }

        /// <summary>
        /// CLIENT SIDE 
        /// This should be handled in the specific game, to do something game-specific, like adding a specific model by asset name.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="asset"></param>
        public virtual void ProcessObjectRequestResponse(int i, string asset)
        {

        } 
        #endregion

        #region Server Side

        /// <summary>
        /// SERVER SIDE
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

        /// <summary>
        /// SERVER SIDE
        /// Add the object being requested 
        /// Reply to the client to let them know that their object was added, what ID it has, and what type of asset they originally requested.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="asset"></param>
        /// <param name="objectId"></param>
        public void ServeObjectRequest(int clientId, string asset, out int objectId)
        {

            objectId = AddOwnedObject(clientId, asset);
            commServer.SendObjectResponsePacket(clientId, objectId, asset);

        }

        SortedList<int, List<int>> ClientObjectIds = new SortedList<int, List<int>>();
        /// <summary>
        /// SERVER SIDE
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

        /// <summary>
        /// SERVER SIDE
        /// Server has received an Object Action packet and it should be processed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        void commServer_ObjectActionReceived(int id, object[] parameters)
        {
            MultiplayerUpdateQueue.Add(new ObjectActionPacket(id, parameters));
        }

        /// <summary>
        /// SERVER SIDE
        /// Server has received a request for a new object from a client.
        /// This is how a client requests an object it can "own"
        /// </summary>
        /// <param name="id"></param>
        /// <param name="asset"></param>
        /// <param name="pos"></param>
        /// <param name="orient"></param>
        /// <param name="vel"></param>
        void commServer_ObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            physicsUpdateList.Add(new ObjectUpdatePacket(id, asset, pos, orient, vel));
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
        void commServer_ChatMessageReceived(string m, string p)
        {
            ProcessChatMessage(m, p);
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
        /// SERVER SIDE
        /// the server received an object request
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="asset"></param>
        public virtual int ProcessObjectRequest(int clientId, string asset)
        {
            int objectId = -1;
            ServeObjectRequest(clientId, asset, out objectId);
            return objectId;
        } 
        #endregion
        
        #endregion
    }
}
