using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Timers;
using Helper;
using Helper.Input;
using Helper.Multiplayer;
using Helper.Multiplayer.Packets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper.Physics;
using Helper.Physics.PhysicsObjects;
using Helper.Camera;
using Helper.Camera.Cameras;

namespace Game
{

    /*
     * Asynchronous sending / receiving in ComServer/Client
     * 
     */
    // Wiki: https://github.com/colbybhearn/3DPhysics/wiki
    public class BaseGame 
    {



        #region Physics
        public PhysicsManager physicsManager;
        public static BaseGame Instance { get; private set; }
        #endregion

        #region Content
        public ContentManager Content { get; private set; }
        Model terrainModel;
        Model planeModel;
        Model staticFloatObjects;
        Model carModel, wheelModel;
        Texture2D moon;
        Terrain terrain;
        PlaneObject planeObj;
        #endregion

        #region Graphics

        public Effect lighteffect;

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
        public delegate void myCallbackDelegate(BaseCamera c, Matrix v, Matrix p);
        public enum GenericCameraModes
        {
            FreeLook,
            ObjectWatch,
            ObjectFirstPerson,
            ObjectChase
        }
        myCallbackDelegate UpdateCameraCallback;

        public string name = "BaseGame";
        public bool DebugInfo = false;
        public bool DebugPhysics = false;
        #endregion

        #region Input
        public InputManager inputManager;
        public Chat ChatManager;
        public SpriteFont chatFont;
        public enum GenericInputGroups
        {
            Camera,
            Client,
        }
        KeyMapCollection keyMapCollections;
        #endregion

        #region Game
        public SortedList<int, Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        public SortedList<int, Gobject> newObjects; // This member is accessed from multiple threads and needs to be locked
        public Gobject currentSelectedObject;
        #endregion
        
        
        internal List<ObjectUpdatePacket> physicsUpdateList = new List<ObjectUpdatePacket>();
        public  CameraManager cameraManager = new CameraManager();
        GenericCameraModes cameraMode = GenericCameraModes.FreeLook;

        #region Communication
        private bool isConnectedToServer;
        public bool IsConnectedToServer
        {
            get
            {
                return isConnectedToServer;
            }
        }

        public CommClient commClient;
        public CommServer commServer;
        #endregion

        #region Events
        public event Helper.Handlers.voidEH Stopped;
        public event Handlers.ChatMessageEH ChatMessageReceived;
        #endregion

        #region Multiplayer
        // Todo - turn the players into a SortedList<int, Player> type? (thus allowing more information than an alias to be stored
        // This can also be used server side
        public SortedList<int, string> players = new SortedList<int, string>(); // User ID, User ID Alias
        public SortedList<int, int> objectsOwned = new SortedList<int, int>(); // Object ID, User ID who owns them
        public List<int> clientControlledObjects = new List<int>(); // TODO - Client Side only, merge with ownedObjects somehow?
        public List<object> MultiplayerUpdateQueue = new List<object>();
        public int MyClientID; // Used by the client
        public bool isClient=false;
        public bool isServer=false;
        #endregion

        public BaseGame()
        {
            CommonInit(10, 10);
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

            physicsManager = new PhysicsManager(ref gameObjects, ref newObjects, physicsUpdateInterval);
            physicsManager.PreIntegrate += new Handlers.voidEH(physicsManager_PreIntegrate);
            physicsManager.PostIntegrate += new Handlers.voidEH(physicsManager_PostIntegrate);
        }

        /// <summary>
        /// The physics engine is about to integrate, so we need to process things from the server about "reality"
        /// now is the time for the client to send ObjectUpdatePackets the server about inputs
        /// now is the tine for the client to process ObjectAttributePackets from the server about changes (shape, mode, behavior).
        /// now is the time for the client to process ObjectUpdatePackets from the server about pos/orient/vel
        /// now is the time for the server to process ObjectActionPackets the client about pos/orient/vel
        /// </summary>
        void physicsManager_PreIntegrate()
        {
            if (isClient)
            {
                lock (gameObjects)
                {
                    #region Send Action Updates to the server
                    foreach (int i in clientControlledObjects)
                    {
                        if (!gameObjects.ContainsKey(i))
                            continue;
                        Gobject go = gameObjects[i];
                        if (!go.actionManager.actionApplied)
                            continue;
                        
                        object[] vals = go.actionManager.GetActionValues();
                        go.actionManager.ValueSwap();
                        commClient.SendObjectAction(go.ID, vals);
                    }
                    #endregion

                    #region Process packets from the server
                    while (MultiplayerUpdateQueue.Count > 0)
                    {
                        lock (MultiplayerUpdateQueue)
                        {
                            Packet p = MultiplayerUpdateQueue[0] as Packet;
                            MultiplayerUpdateQueue.RemoveAt(0);

                            if (p is ObjectUpdatePacket)
                            {
                                #region Process Update Packets from the server
                                ObjectUpdatePacket oup = p as ObjectUpdatePacket;

                                if (!gameObjects.ContainsKey(oup.objectId))
                                {
                                    AddNewObject(oup.objectId, oup.assetName);
                                    continue;
                                    // TODO -  Should we continue instead of not updating this frame?
                                }
                                // (can't yet due to AddNewObject waiting until the next integrate to actually add it)
                                Gobject go = gameObjects[oup.objectId];
                                go.Interpoladate(oup.position, oup.orientation, oup.velocity);
                                #endregion
                            }
                            else if (p is ObjectAttributePacket)
                            {
                                #region Process Attribute Packets from the server
                                ObjectAttributePacket oap = p as ObjectAttributePacket;
                                if (gameObjects.ContainsKey(oap.objectId))
                                {
                                    Gobject go = gameObjects[oap.objectId];
                                    go.SetObjectAttributes(oap.booleans, oap.ints, oap.floats);
                                }
                                #endregion
                            }                            
                        }
                    }
                    #endregion
                }
            }
            else if (isServer)
            {
                lock (gameObjects)
                {
                    #region Process Action Updates from the client
                    lock (MultiplayerUpdateQueue)
                    {
                        while (MultiplayerUpdateQueue.Count > 0)
                        {
                            ObjectActionPacket oap = MultiplayerUpdateQueue[0] as ObjectActionPacket;
                            if (!gameObjects.ContainsKey(oap.objectId))
                                continue; // TODO - infinite loop if this is hit
                            Gobject go = gameObjects[oap.objectId];
                            go.actionManager.ProcessActionValues(oap.actionParameters);
                            MultiplayerUpdateQueue.RemoveAt(0);
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// the physics engine just integrated, so this is the newest information about "reality"
        /// now is the time for the server to send ObjectUpdatePackets to the client for objects that can move
        /// now is the time for the server to send ObjectAttributePackets to the client for objects whose attributes have changed
        /// </summary>
        void physicsManager_PostIntegrate()
        {
            if (isClient)
            {
                
            }
            else if (isServer)
            {
                if (commServer != null)
                {
                    lock (gameObjects)
                    {
                        foreach (Gobject go in gameObjects.Values)
                        {
                            #region Send Attribute Updates to the client
                            if (go.hasAttributeChanged)
                            {
                                bool[] bools = null;
                                int[] ints = null;
                                float[] floats = null;
                                go.GetObjectAttributes(out bools, out ints, out floats);
                                ObjectAttributePacket oap = new ObjectAttributePacket(go.ID, bools, ints, floats);
                                commServer.BroadcastPacket(oap);
                            }
                            #endregion

                            #region Send Object Updates to the client
                            if (go.isMoveable && go.IsActive)
                            {
                                ObjectUpdatePacket oup = new ObjectUpdatePacket(go.ID, go.Asset, go.BodyPosition(), go.BodyOrientation(), go.BodyVelocity());
                                commServer.BroadcastObjectUpdate(oup);
                            }
                            #endregion
                        }
                    }
                }
            }

            if (cameraManager != null)
            {
                if (UpdateCameraCallback == null)
                    return;
                cameraManager.Update();

                UpdateCameraCallback(cameraManager.currentCamera, cameraManager.ViewMatrix(), cameraManager.ProjectionMatrix());
            }
        }

        public virtual void PreUpdateCameraCallback()
        {
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
                System.Diagnostics.Trace.WriteLine(e.StackTrace);
            }
        }

        public virtual void InitializeCameras()
        {
            cameraManager.AddCamera(GenericCameraModes.FreeLook.ToString(), new FreeCamera());
            cameraManager.AddCamera(GenericCameraModes.ObjectChase.ToString(), new ChaseCamera());
            cameraManager.AddCamera(GenericCameraModes.ObjectFirstPerson.ToString(), new FirstPersonCamera());
            cameraManager.AddCamera(GenericCameraModes.ObjectWatch.ToString(), new WatchCamera());
            cameraManager.SetCurrentCamera(GenericCameraModes.FreeLook.ToString());
            foreach (ViewProfile vp in GetViewProfiles())
                cameraManager.AddProfile(vp);
        }

        public virtual List<ViewProfile> GetViewProfiles()
        {
            List<ViewProfile> views = new List<ViewProfile>();
            views.Add(new ViewProfile(GenericCameraModes.ObjectChase.ToString(), "Airplane", new Vector3(0,3,10), .25f, Vector3.Zero, 1.0f));
            views.Add(new ViewProfile(GenericCameraModes.ObjectFirstPerson.ToString(), "car", new Vector3(-.45f, 1.4f, .05f), .25f, new Vector3(0, (float)-Math.PI / 2, 0), 1.0f));
            views.Add(new ViewProfile(GenericCameraModes.ObjectFirstPerson.ToString(), "Airplane", new Vector3(0, 3, 10), .25f, new Vector3(0, 0, 0), 1.0f));
            
            return views;

        }

        public virtual void InitializeInputs()
        {
            keyMapCollections = GetDefaultControls();
        }

        public virtual KeyMapCollection GetDefaultControls()
        {
            KeyMapCollection defControls = new KeyMapCollection();
            List<KeyBinding> cameraDefaults = new List<KeyBinding>();
            cameraDefaults.Add(new KeyBinding("Camera Move Forward", Keys.NumPad8, false, false, false, KeyEvent.Down, CameraMoveForward));
            cameraDefaults.Add(new KeyBinding("Camera Move Left", Keys.NumPad4, false, false, false, KeyEvent.Down, CameraMoveLeft));
            cameraDefaults.Add(new KeyBinding("Camera Move Backward", Keys.NumPad5, false, false, false, KeyEvent.Down, CameraMoveBackward));
            cameraDefaults.Add(new KeyBinding("Camera Move Right", Keys.NumPad6, false, false, false, KeyEvent.Down, CameraMoveRight));
            cameraDefaults.Add(new KeyBinding("Camera Move Speed Increase", Keys.NumPad7, false, false, false, KeyEvent.Pressed, CameraMoveSpeedIncrease));
            cameraDefaults.Add(new KeyBinding("Camera Move Speed Decrease", Keys.NumPad1, false, false, false, KeyEvent.Pressed, CameraMoveSpeedDecrease));
            cameraDefaults.Add(new KeyBinding("Camera Move Height Increase", Keys.NumPad9, false, false, false, KeyEvent.Down, CameraMoveHeightIncrease));
            cameraDefaults.Add(new KeyBinding("Camera Move Height Decrease", Keys.NumPad3, false, false, false, KeyEvent.Down, CameraMoveHeightDecrease));

            cameraDefaults.Add(new KeyBinding("Camera Move Cycle", Keys.Decimal, false, false, false, KeyEvent.Pressed, CameraModeCycle));
            cameraDefaults.Add(new KeyBinding("Camera Home", Keys.Multiply, false, false, false, KeyEvent.Pressed, CameraMoveHome));
            //
            cameraDefaults.Add(new KeyBinding("Toggle Debug Info", Keys.F1, false, false, false, KeyEvent.Pressed, ToggleDebugInfo));
            cameraDefaults.Add(new KeyBinding("Toggle Physics Debug", Keys.F2, false, false, false, KeyEvent.Pressed, TogglePhsyicsDebug));
            
            KeyMap camControls = new KeyMap(GenericInputGroups.Camera.ToString(), cameraDefaults);

            List<KeyBinding> ClientDefs = new List<KeyBinding>();
            ClientDefs.Add(new KeyBinding("Escape", Keys.Escape, false, false, false, KeyEvent.Pressed, Stop));
            KeyMap clientControls = new KeyMap(GenericInputGroups.Client.ToString(), ClientDefs);

            defControls.AddMap(camControls);
            defControls.AddMap(clientControls);
            return defControls;
        }

        #region Camera Manipulation
        public void CameraMoveForward()
        {
            cameraManager.MoveForward();
        }

        public void CameraMoveBackward()
        {
            cameraManager.MoveBackward();
        }

        public void CameraMoveLeft()
        {
            cameraManager.MoveLeft();
        }

        public void CameraMoveRight()
        {
            cameraManager.MoveRight();
        }

        public void CameraMoveSpeedIncrease()
        {
            cameraManager.IncreaseMovementSpeed();
        }

        public void CameraMoveSpeedDecrease()
        {
            cameraManager.DecreaseMovementSpeed();
        }

        public void AdjustCameraOrientation(float pitch, float yaw)
        {
            cameraManager.AdjustTargetOrientation(pitch, yaw);
        }

        public void CameraModeCycle()
        {
            switch (cameraMode)
            {
                case GenericCameraModes.FreeLook:
                    cameraMode = GenericCameraModes.ObjectWatch;
                    break;
                case GenericCameraModes.ObjectWatch:
                    cameraMode = GenericCameraModes.ObjectChase;
                    break;
                case GenericCameraModes.ObjectChase:
                    cameraMode = GenericCameraModes.ObjectFirstPerson;
                    break;
                case GenericCameraModes.ObjectFirstPerson:
                    cameraMode = GenericCameraModes.FreeLook;                    
                    break;
                default:
                    break;
            }
            cameraManager.SetCurrentCamera(cameraMode.ToString());
            cameraManager.SetGobjectList(cameraMode.ToString(), new List<Gobject> { currentSelectedObject });
            
        }


        public void CameraMoveHeightIncrease()
        {
            cameraManager.MoveUp();
        }

        public void CameraMoveHeightDecrease()
        {
            cameraManager.MoveDown();
        }

        public void CameraMoveHome()
        {
            cameraMode = GenericCameraModes.FreeLook;
            //cam.MoveHome();
        }
        #endregion

        public void ToggleDebugInfo()
        {
            DebugInfo = !DebugInfo;
        }

        public void TogglePhsyicsDebug()
        {
            DebugPhysics = !DebugPhysics;
        }

        public void AdjustZoom(float z)
        {
            if (z > 0)
                cameraManager.ZoomOut();
            else
                cameraManager.ZoomIn();
                
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
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
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
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
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
                                            new Vector3(15000f, .55f, 15000f),  // X with, possible y range, Z depth 
                                            100, 100, graphicsDevice, moon);

                    newObjects.Add(terrain.ID, terrain);
                }
                catch (Exception E)
                {
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
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
                    System.Diagnostics.Debug.WriteLine(E.StackTrace);
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

        /// <summary>
        /// This is called by BaseGame immediately before Keyboard state is used to process the KeyBindings
        /// we don't want to handle keydowns and keyups, so revert to nominal states and then immediately process key actions to arrive at a current state
        /// </summary>
        public virtual void SetNominalInputState()
        {
            foreach (int i in clientControlledObjects)
            {
                if (!gameObjects.ContainsKey(i))
                    return;
                gameObjects[i].SetNominalInput();
            }
            // for client-side only created object (mainly for testing new aircraft without the server. this can be removed after 2012.09.25)
            foreach (Gobject go in gameObjects.Values)
                go.SetNominalInput();
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

        public virtual void InitializeMultiplayer()
        {
            if(isClient)
            {
                commClient.ClientInfoRequestReceived += new Handlers.IntEH(commClient_ClientInfoRequestReceived);
                commClient.ChatMessageReceived += new Handlers.ChatMessageEH(commClient_ChatMessageReceived);
                commClient.ObjectAddedReceived += new Handlers.ObjectAddedResponseEH(commClient_ObjectAddedReceived);
                commClient.ObjectActionReceived += new Handlers.ObjectActionEH(commClient_ObjectActionReceived);
                commClient.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commClient_ObjectUpdateReceived);
                commClient.DisconnectedFromServer += new Handlers.IntEH(commClient_NotConnectedToServer);
                commClient.ConnectedToServer += new Handlers.ClientConnectedEH(commClient_ClientConnected);
                commClient.ObjectAttributeReceived += new Handlers.ObjectAttributeEH(commClient_ObjectAttributeReceived);
            }
            else if(isServer)
            {
                // TODO: Should client connected and ChatMessage Received be handled elsewhere (not in BaseGame) for the server?
                commServer.ClientConnected += new Handlers.IntStringEH(commServer_ClientConnected);
                commServer.ChatMessageReceived += new Handlers.ChatMessageEH(commServer_ChatMessageReceived);
                commServer.ObjectUpdateReceived += new Handlers.ObjectUpdateEH(commServer_ObjectUpdateReceived);
                commServer.ObjectActionReceived += new Handlers.ObjectActionEH(commServer_ObjectActionReceived);
            }
        }

        void commClient_ObjectAttributeReceived(ObjectAttributePacket oap)
        {
            lock (MultiplayerUpdateQueue)
            {
                MultiplayerUpdateQueue.Add(oap);
            }
        }

        public event Handlers.ClientConnectedEH ConnectedToServer; 
        void commClient_ClientConnected(int id, string alias)
        {
            if (players.ContainsKey(id) == false)
                players.Add(id, alias);

            isConnectedToServer = true;
            if (ConnectedToServer == null)
                return;
            ConnectedToServer(id, alias);
        }

        public event Handlers.IntEH DiconnectedFromServer;
        void commClient_NotConnectedToServer(int id)
        {
            isConnectedToServer = false;
            if (DiconnectedFromServer == null)
                return;
            DiconnectedFromServer(id);
            
        }

        void commClient_ObjectUpdateReceived(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel)
        {
            lock (MultiplayerUpdateQueue)
            {
                MultiplayerUpdateQueue.Add(new Helper.Multiplayer.Packets.ObjectUpdatePacket(id, asset, pos, orient, vel));
            }
        }



        // CLIENT
        private void CallChatMessageReceived(ChatMessage cm)
        {
            if (ChatMessageReceived == null)
                return;
            ChatMessageReceived(cm);
        }

        // COMMON
        public virtual void ProcessChatMessage(ChatMessage cm)
        {
        }
        public void SendChatPacket(ChatMessage msg)
        {
            if (isClient)
            {
                if (commClient != null)
                    commClient.SendChatPacket(msg.Message, MyClientID);
            }
            else
            {
                if (commServer != null)
                    commServer.BroadcastChatMessage(msg.Message, msg.Owner);
            }
        }
        #endregion

        #region Client Side
        // CLIENT only
        public virtual bool ConnectToServer(string ip, int port, string alias)
        {
            isClient = true;
            commClient = new CommClient(ip, port, alias);
            InitializeMultiplayer();
            //ChatManager.PlayerAlias = alias;
            return commClient.Connect();
        }
        /// <summary>
        /// CLIENT SIDE
        /// Client received an info request packet from the server, which contains our ID to use
        /// </summary>
        /// <param name="id"></param>
        void commClient_ClientInfoRequestReceived(int id)
        {
            MyClientID = id;
        }

        void commClient_ChatMessageReceived(ChatMessage cm)
        {
            String alias;
            if (players.TryGetValue(cm.Owner, out alias))
                cm.OwnerAlias = alias;

            CallChatMessageReceived(cm);
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
        /// This is called from the Network code, thus in the Network threads
        /// </summary>
        /// <param name="i"></param>
        /// <param name="asset"></param>
        void commClient_ObjectAddedReceived(int owner, int id, string asset)
        {
            // MINE!
            if(owner == MyClientID)
                clientControlledObjects.Add(id);
            objectsOwned.Add(id, owner);
            ProcessObjectAdded(owner, id, asset);
        }

        /// <summary>
        /// CLIENT SIDE 
        /// This should be handled in the specific game, to do something game-specific, like adding a specific model by asset name.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="asset"></param>
        public virtual void ProcessObjectAdded(int owner, int id, string asset)
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
            commServer.BroadcastObjectAddedPacket(clientId, objectId, asset);
        }

        SortedList<int, List<int>> ClientObjectIds = new SortedList<int, List<int>>();
        /// <summary>
        /// SERVER SIDE
        /// Server adds an object and associates it with its owning client
        /// Called from the Network threads
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
            lock (MultiplayerUpdateQueue)
            {
                MultiplayerUpdateQueue.Add(new ObjectActionPacket(id, parameters));
            }
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
            isServer = true;
            commServer = new CommServer(port);
            InitializeMultiplayer();
            commServer.Start();

        }
        void commServer_ObjectRequestReceived(int clientId, string asset)
        {

        }
        void commServer_ChatMessageReceived(ChatMessage cm)
        {
            String alias;
            if (players.TryGetValue(cm.Owner, out alias))
                cm.OwnerAlias = alias;

            ProcessChatMessage(cm);
        }
        void commServer_ClientConnected(int id, string s)
        {
            ProcessClientConnected(id, s);
        }

        public virtual void ProcessClientConnected(int id, string alias)
        {
            CallClientConnected(id, alias);
            commServer.BroadcastChatMessage("Player " + alias + " has joined.", -1);
        }

        public event Helper.Handlers.IntStringEH ClientConnected;
        private void CallClientConnected(int id, string alias)
        {
            players.Add(id, alias);
            // Let new client know about all other clients
            for (int i = 0; i < players.Count; i++)
                if(id != players.Keys[i])
                    commServer.SendPlayerInformation(id, players.Keys[i], players.Values[i]);

            if (ClientConnected == null)
                return;
            ClientConnected(id, alias);

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

        public void Stop()
        {
            physicsManager.Stop();
            if (commClient != null)
                commClient.Stop();
            if(commServer!=null)
                commServer.Stop();
            CallStopped();
        }
        private void CallStopped()
        {
            if (Stopped == null)
                return;
            Stopped();
        }

        /// <summary>
        /// CLIENT SIDE
        /// calls this to disconnect from the server
        /// </summary>
        public void DisconnectFromServer()
        {
            commClient.Stop();
        }
    }
}
