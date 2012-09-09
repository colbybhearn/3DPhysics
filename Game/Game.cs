using System.Collections.Generic;
using Physics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.ComponentModel.Design;
using System;
using Microsoft.Xna.Framework;
using Physics.PhysicsObjects;
using Microsoft.Xna.Framework.Input;

namespace Game
{
    public class PhysGame 
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

        #region Physics
        public Physics.PhysicsManager physicsManager;
        public static PhysGame Instance { get; private set; }
        #endregion

        #region Content
        public ContentManager Content { get; private set; }
        Model cubeModel;
        Model sphereModel;
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
        #endregion

        #region Input
        Input.InputManager inputManager;
        #endregion

        #region Game
        public List<Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        public List<Gobject> newObjects; // This member is accessed from multiple threads and needs to be locked
        Gobject currentSelectedObject;
        #endregion

        public PhysGame()
        {
            graphicsDevice = null;
            gameObjects = new List<Gobject>();
            newObjects = new List<Gobject>();
            Instance = this;
            
            physicsManager = new Physics.PhysicsManager(ref gameObjects, ref newObjects);
            inputManager = new Input.InputManager(50);
        }

        public void Initialize(ServiceContainer services, GraphicsDevice gd)
        {
            Services = services;            
            graphicsDevice = gd;
            try
            {

                InitializeContent();
                //InitializeCameras();
                //InitializeObjects();
                InitializeEnvironment();
                InitializeInputs();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }

        }

        private void InitializeInputs()
        {
            inputManager.AddWatch(new Input.KeyWatch(Keys.Up, false, false, false, Input.KeyWatch.keyEvent.Down, Forward));
            inputManager.AddWatch(new Input.KeyWatch(Keys.Down, false, false, false, Input.KeyWatch.keyEvent.Down, Down));
        }

        private void Forward()
        {

        }

        private void Down()
        {

        }


        private void InitializeContent()
        {
            Content = new ContentManager(Services, "content");

            try
            {
                LoadModel(cubeModel, "Cube");
                LoadModel(sphereModel, "Sphere");            

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
        private void InitializeEnvironment()
        {            
            physicsManager.AddCar(carModel, wheelModel);
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

        private void SelectGameObject(Gobject go)
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


    }
}
