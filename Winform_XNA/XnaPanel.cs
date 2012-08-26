﻿using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;
using JigLibX.Math;

namespace Winform_XNA
{
    class XnaPanel : XnaControl
    {
        #region Todo
        /* Refactor Physics Controllers
         * 
         */
        #endregion

        #region Fields
        Camera cam;
        LunarVehicle lv;
        Texture2D moon;
        Camera objectCam;
        float SimFactor = 1.0f;
        Gobject currentSelectedObject = null;
        Terrain terrain;
        enum CameraModes
        {
            Fixed,
            ObjectFirstPerson,
            ObjectChase,
            ObjectWatch
        }
        CameraModes cameraMode = CameraModes.Fixed;
        enum InputModes
        { 
            Camera,
            Object            
        }
        InputModes inputMode = InputModes.Object;

        #region Content
        public ContentManager Content { get; private set; }
        Model cubeModel;
        Model sphereModel;
        #endregion

        #region Debug
        private SpriteBatch spriteBatch;
        private SpriteFont debugFont;        
        public bool Debug { get; set; }
        public bool DebugPhysics { get; set; }
        public bool DrawingEnabled { get; set; }
        public bool PhysicsEnabled { get; set; }
        #endregion

        #region Physics
        //BoostController bController;
        public PhysicsSystem PhysicsSystem { get; private set; }
        private System.Timers.Timer tmrPhysicsUpdate;

        #endregion

        #region Game
        private Stopwatch tmrDrawElapsed;
        private Stopwatch tmrPhysicsElapsed;
        private double lastPhysicsElapsed;
        private List<Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        private List<Gobject> newObjects;
        
        double TIME_STEP = .01; // Recommended timestep
        #endregion
        #endregion

        #region Init
        protected override void Initialize()
        {
            Content = new ContentManager(Services, "content");

            try
            {
                InitializeContent();                
                InitializePhysics();
                InitializeCameras();
                InitializeObjects();
                InitializeEnvironment();

                tmrDrawElapsed = Stopwatch.StartNew();
                tmrPhysicsElapsed = new Stopwatch();
                spriteBatch = new SpriteBatch(GraphicsDevice);
                
                new Game();
                debugFont = Content.Load<SpriteFont>("DebugFont");
                
                // From the example code, should this be a timer instead?
                Application.Idle += delegate { Invalidate(); };

                tmrPhysicsUpdate = new System.Timers.Timer();
                tmrPhysicsUpdate.AutoReset = false;
                tmrPhysicsUpdate.Enabled = false;
                tmrPhysicsUpdate.Interval = 10;
                tmrPhysicsUpdate.Elapsed += new System.Timers.ElapsedEventHandler(tmrPhysicsUpdate_Elapsed);
                tmrPhysicsUpdate.Start();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            
        }
        
        private void InitializeContent()
        {
            cubeModel = Content.Load<Model>("Cube");
            sphereModel = Content.Load<Model>("Sphere");
            moon = Content.Load<Texture2D>("Moon");
        }
        private void InitializePhysics()
        {
            gameObjects = new List<Gobject>();
            newObjects = new List<Gobject>();

            PhysicsSystem = new PhysicsSystem();
            PhysicsSystem.CollisionSystem = new CollisionSystemSAP();
            PhysicsSystem.EnableFreezing = true;
            PhysicsSystem.SolverType = PhysicsSystem.Solver.Normal;

            PhysicsSystem.CollisionSystem.UseSweepTests = true;
            PhysicsSystem.Gravity = new Vector3(0, -2f, 0);
            // CollisionTOllerance and Allowed Penetration
            // changed because our objects were "too small"
            PhysicsSystem.CollisionTollerance = 0.01f;
            PhysicsSystem.AllowedPenetration = 0.001f;

            PhysicsSystem.NumCollisionIterations = 8;
            PhysicsSystem.NumContactIterations = 8;
            PhysicsSystem.NumPenetrationRelaxtionTimesteps = 15;
        }
        private void InitializeCameras()
        {

            cam = new Camera(new Vector3(.05f, 1.5f, 2.7f));
            cam.lagFactor = .07f;
            objectCam = new Camera(new Vector3(0, 0, 0));
            objectCam.lagFactor = 1.0f;

        }
        private void InitializeObjects()
        {            
            //AddSphere(new Vector3(0, 0, .2f), 1f, sphereModel, false);
            //AddSphere(new Vector3(0, -3, 0), 2f, sphereModel, true);
            /*
            AddBox(new Vector3(0, 10, 0), new Vector3(1f, 1f, 1f), Matrix.Identity, cubeModel, true);
            AddBox(new Vector3(0, -5, 0), new Vector3(50f, 5f, 50f), Matrix.CreateFromAxisAngle(Vector3.UnitX, (float)(10 * Math.PI / 180)), cubeModel, false);
            AddBox(new Vector3(0, 1.25f, 27.5f), new Vector3(50f, 7.5f, 5f), Matrix.Identity, cubeModel, false);
            AddBox(new Vector3(0, 1.25f, -27.5f), new Vector3(50f, 7.5f, 5f), Matrix.Identity, cubeModel, false);
            AddBox(new Vector3(27.5f, 1.25f, 0), new Vector3(5f, 7.5f, 60f), Matrix.Identity, cubeModel, false);
            AddBox(new Vector3(-27.5f, 1.25f, 0), new Vector3(5f, 7.5f, 60f), Matrix.Identity, cubeModel, false);
            */
            //AddBox(new Vector3(0, 0, 0), new Vector3(.5f, .5f, .5f),  Matrix.Identity, cubeModel, false);
            //AddBox(new Vector3(-1, .5f, 1), new Vector3(.5f, .5f, .5f),  Matrix.Identity, cubeModel, false);
            //AddBox(new Vector3(0, .4f, -5), new Vector3(.5f, .5f, .5f), Matrix.Identity, cubeModel, false);

            // Giant Floor
            //AddBox(new Vector3(0, -1, 0), new Vector3(50f, 1f, 50f), Matrix.Identity, cubeModel, false);

            AddNewObjects();
        }
        private void InitializeEnvironment()
        {
            try
            {
                terrain = new Terrain(new Vector3(0, .5f, 0), new Vector3(15, 1, 15), 100, 100,  this.GraphicsDevice, moon);
                //Gobject terraingo = new Gobject(, terrain.GetMesh(), null, false);
                //terrain.Body = terraingo.Body;
                //terrain.Skin = terraingo.Body.CollisionSkin;
                newObjects.Add(terrain);
            }
            catch (Exception E)
            {
            }
        }
        
        #endregion

        #region Methods
        private Gobject AddBox(Vector3 pos, Vector3 size, Matrix orient, Model model, bool moveable)
        {
            Box boxPrimitive = new Box(-.5f*size, orient, size);
            Gobject box = new Gobject(
                pos,
                size/2,
                boxPrimitive,
                model,
                moveable
                );

            newObjects.Add(box);
            return box;
        }
        private Gobject AddSphere(Vector3 pos, float radius, Model model, bool moveable)
        {
            Sphere spherePrimitive = new Sphere(pos, radius);
            Gobject sphere = new Gobject(
                pos,
                Vector3.One * radius,
                spherePrimitive,
                model,
                moveable);

            newObjects.Add(sphere);
            return sphere;
        }
        private LunarVehicle AddLunarLander(Vector3 pos, Vector3 size, Matrix orient, Model model)
        {
            Box boxPrimitive = new Box(pos, orient, size);
            // 2012.08.25 CBH
            // I'm very confused by the different size and scale notations.
            //The triangle mesh may be working well for these large objects.
            //boxes do still get stuck sometimes in the mesh
            
            LunarVehicle lander = new LunarVehicle(
                pos,
                size,// /2
                boxPrimitive,
                model
                );            

            newObjects.Add(lander);
            return lander;

        }
        private void AddNewObjects()
        {
            while (newObjects.Count > 0)
            {
                // Remove from end of list so no shuffling occurs? (maybe)
                int i = newObjects.Count - 1;
                newObjects[i].FinalizeBody();
                gameObjects.Add(newObjects[i]);
                newObjects.RemoveAt(i);
            }
        }
        private void SelectGameObject(Gobject go)
        {
            if (currentSelectedObject != null)
                currentSelectedObject.Selected = false;
            currentSelectedObject = go;
            currentSelectedObject.Selected = true;
            objectCam.TargetPosition = currentSelectedObject.Position;
        }

        #endregion

        #region User Input
        private void ProcessCameraControl(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                cam.IncreaseSpeed();
            }
            if (e.KeyCode == Keys.Z)
            {
                cam.DecreaseSpeed();
            }
            if (e.KeyCode == Keys.W)
            {
                cam.MoveForward();
            }
            if (e.KeyCode == Keys.A)
            {
                cam.MoveLeft();
            }
            if (e.KeyCode == Keys.S)
            {
                cam.MoveBackward();
            }
            if (e.KeyCode == Keys.D)
            {
                cam.MoveRight();
            }

            if (e.KeyCode == Keys.N)
            {
                if (e.Shift)
                    AddSpheres(5);
                else
                    AddSphere();
            }
        }
        private void ProcessObjectControlKeyDown(KeyEventArgs e)
        {

            if (e.KeyCode == Keys.L)
            {
                lv = AddLunarLander(new Vector3(0, 3, 0), new Vector3(1.0f, 1.0f, 1.0f), Matrix.CreateRotationY((float)Math.PI), cubeModel);
            }
            if (lv != null)
            {
                lv.ProcessInputKeyDown(e);
            }

            if (e.KeyCode == Keys.C)
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
        }
        private void ProcessObjectControlKeyUp(KeyEventArgs e)
        {
            if (lv != null)
                lv.ProcessInputKeyUp(e);
            
        }
        public void ProcessKeyDown(KeyEventArgs e)
        {
            switch (inputMode)
            {
                case InputModes.Camera:
                    ProcessCameraControl(e);
                    break;
                case InputModes.Object:
                    ProcessObjectControlKeyDown(e);
                    break;
                default:
                    break;
            }           

            if (e.KeyCode == Keys.M)
            {
                if (inputMode == InputModes.Object)
                    inputMode = InputModes.Camera;
                else
                    inputMode = InputModes.Object;
            }
        }
        internal void ProcessKeyUp(KeyEventArgs e)
        {
            switch (inputMode)
            {
                case InputModes.Camera:
                    break;
                case InputModes.Object:
                    ProcessObjectControlKeyUp(e);
                    break;
                default:
                    break;
            }
        }
        internal void ProcessMouseDown(MouseEventArgs e, System.Drawing.Rectangle bounds)
        {
            Viewport view = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            Vector2 mouse = new Vector2(e.Location.X, e.Location.Y);
            Microsoft.Xna.Framework.Ray r = cam.GetMouseRay(mouse, view);
            Gobject nearest = null;
            float min = float.MaxValue;
            float dist = 0;
            Vector3 pos;
            Vector3 norm;
            CollisionSkin cs = new CollisionSkin();

            lock (gameObjects)
            {
                if (PhysicsSystem.CollisionSystem.SegmentIntersect(out dist, out cs, out pos, out norm, new Segment(r.Position, r.Direction * 1000), new MyCollisionPredicate()))
                {
                    Body b = cs.Owner;
                    Gobject go = b.ExternalData as Gobject;
                    SelectGameObject(go);
                }
            }
        }
        private void AddSphere()
        {
            AddSphere(new Vector3(0, 30, 0), .5f, sphereModel, true);
        }
        private void AddSpheres(int n)
        {
            Random r = new Random();
            for (int i = 0; i < n; i++)
            {
                AddSphere(
                    new Vector3(
                        (float)(10 - r.NextDouble() * 20),
                        (float)(40 - r.NextDouble() * 20),
                        (float)(10 - r.NextDouble() * 20)),
                    (float)(.5f + r.NextDouble()), sphereModel, true);
            }
        }
        internal void PanCam(float dX, float dY)
        {
            cam.AdjustOrientation(-dY*.001f,-dX*.001f);
        }
        #endregion

        #region Physics
        internal void SetSimFactor(float value)
        {
            SimFactor = value;
        }
        public void ResetTimer()
        {
            tmrPhysicsElapsed.Restart();

            tmrPhysicsUpdate.Stop();
            tmrPhysicsUpdate.Start();
        }
        void tmrPhysicsUpdate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Add our new objects
            lock (gameObjects)
            {
                AddNewObjects();

                // Should use a variable timerate to keep up a steady "feel" if we bog down?
                if (PhysicsEnabled)
                {
                    float step = (float)TIME_STEP * SimFactor;
                    PhysicsSystem.CurrentPhysicsSystem.Integrate(step);
                }
            }
            cam.UpdatePosition();
            objectCam.UpdatePosition();
            lastPhysicsElapsed = tmrPhysicsElapsed.ElapsedMilliseconds;

            ResetTimer();
        }
        #endregion

        #region Draw
        protected override void Draw()
        {
            try
            {
                Matrix proj = Matrix.Identity;
                GraphicsDevice.Clear(Color.Gray);

                /* Do Drawing Here!
                 * Should probably call Game.Draw(GraphicsDevice);
                 * Allow Game to handle all of the Drawing independantly
                 *   thus this form just exist heres?
                 * Need to think of a good structure for this
                 *
                 * Answer to above question!
                 * This Control should only handle the world VIEW
                 *    possibly passing a camera to Game when calling draw.
                 * This allows for a 4x split panel for world editing, or 2-4x splitscreen
                 */

                

                DrawObjects();

                Matrix v = Matrix.Identity;
                Matrix p = Matrix.Identity;
                switch (cameraMode)
                {
                    case CameraModes.Fixed:
                    case CameraModes.ObjectWatch:
                        v = cam.RhsLevelViewMatrix;
                        p = cam._projection;
                        break;
                    case CameraModes.ObjectFirstPerson:
                    case CameraModes.ObjectChase:
                        v = objectCam.RhsViewMatrix;
                        p = objectCam._projection;
                        break;
                    default:
                        break;
                }

                //terrain.DrawWireframe(GraphicsDevice, v, p);
                terrain.Draw(GraphicsDevice, v, p); 

                if (Debug)
                {
                    double time = tmrDrawElapsed.ElapsedMilliseconds;
                    spriteBatch.Begin();
                    Vector2 position = new Vector2(5, 5);
                    spriteBatch.DrawString(debugFont, "Debug Text: Enabled", position, Color.LightGray);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "FPS: " + (1000.0 / time), position, Color.LightGray);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "TPS: " + (1000.0 / lastPhysicsElapsed), position, Color.LightGray); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraPosition", cam.TargetPosition);
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraOrientation", Matrix.CreateFromQuaternion(cam.Orientation).Forward);

                    spriteBatch.DrawString(debugFont, "Cam Mode: " + cameraMode.ToString(), position, Color.LightGray); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "Input Mode: " + inputMode.ToString(), position, Color.LightGray); // physics Ticks Per Second

                    spriteBatch.End();

                    // Following 3 lines are to reset changes to graphics device made by spritebatch
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                    tmrDrawElapsed.Restart();
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
        private Vector2 DebugShowVector(SpriteBatch sb, SpriteFont font, Vector2 p, string s, Vector3 vector)
        {
            sb.DrawString(font, s + ".X = " + vector.X, p, Color.LightGray);
            p.Y += font.LineSpacing;

            sb.DrawString(font, s + ".Y = " + vector.Y, p, Color.LightGray);
            p.Y += font.LineSpacing;

            sb.DrawString(font, s + ".Z = " + vector.Z, p, Color.LightGray);
            p.Y += font.LineSpacing;

            return p;
        }
        public void DrawObjects()
        {
            lock (gameObjects)
            {
                foreach (Gobject go in gameObjects)
                {
                    switch (cameraMode)
                    {
                        case CameraModes.Fixed:
                            if (DrawingEnabled)
                                go.Draw(cam.RhsLevelViewMatrix, cam._projection);
                            if (DebugPhysics)
                                go.DrawWireframe(GraphicsDevice, cam.RhsLevelViewMatrix, cam._projection);
                            break;
                        case CameraModes.ObjectFirstPerson:
                            objectCam.SetOrientation(currentSelectedObject.Body.Orientation);
                            objectCam.TargetPosition = currentSelectedObject.Body.Position;
                            if (DrawingEnabled)
                                go.Draw(objectCam.RhsViewMatrix, objectCam._projection);
                            if (DebugPhysics)
                                go.DrawWireframe(GraphicsDevice, objectCam.RhsViewMatrix, objectCam._projection);
                            break;
                        case CameraModes.ObjectChase:
                            Vector3 ThirdPersonRef = new Vector3(0, 1, 5);
                            Vector3 TransRef = Vector3.Transform(ThirdPersonRef, currentSelectedObject.Body.Orientation);
                            objectCam.TargetPosition = TransRef + currentSelectedObject.Body.Position;
                            objectCam.LookAtLocation(currentSelectedObject.Body.Position);

                            if (DrawingEnabled)
                                go.Draw(objectCam.RhsViewMatrix, objectCam._projection);
                            if (DebugPhysics)
                                go.DrawWireframe(GraphicsDevice, objectCam.RhsLevelViewMatrix, objectCam._projection);
                            break;
                        case CameraModes.ObjectWatch:
                            cam.LookAtLocation(currentSelectedObject.Body.Position);
                            go.Draw(cam.RhsLevelViewMatrix, cam._projection);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        #endregion
    }

    public class MyCollisionPredicate : CollisionSkinPredicate1
    {
        public override bool ConsiderSkin(CollisionSkin skin0)
        {
            return true;
        }
    }
}
