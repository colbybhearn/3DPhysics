using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Physics;
using Physics.PhysicsObjects;
using Helper;


namespace Winform_XNA
{

    /*
     * I had to reference the WindowsGameLibrary from Clientapp in order for the ContentManager to load any models when invoked from the client (it worked fine in XNA_Panel and the missing reference was the only difference)
     * 
     * 
     */
    public class XnaPanel : XnaControl
    {
        #region Todo
        /* Refactor Physics Controllers
         * fix mesh wireframe connections being drawn
         * fix location of wireframe to match physics
         * fix collision with objects
         */
        #endregion

        #region Fields
        Camera cam;
        Camera objectCam;

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
        //public ContentManager Content { get; private set; }
        #endregion

        #region Debug
        private SpriteBatch spriteBatch;
        private SpriteFont debugFont;        
        public bool Debug { get; set; }
        public bool DebugPhysics { get; set; } 
        public bool DrawingEnabled { get; set; }
        public bool PhysicsEnabled { get; set; }
        private int ObjectsDrawn { get; set; }
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
        private List<Physics.Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        private List<Physics.Gobject> newObjects;
        
        double TIME_STEP = .01; // Recommended timestep
        #endregion
        Game.PhysGame game;
        #endregion

        #region Init

        public XnaPanel() 
        {
            gameObjects = new List<Gobject>();
            newObjects = new List<Gobject>();
            InitializeCameras();
        }

        public XnaPanel(ref Game.PhysGame g)
        {
            game = g;
            PhysicsSystem = g.physicsManager.PhysicsSystem;
            gameObjects = g.gameObjects;
            newObjects = g.newObjects;
            
            InitializeCameras();
        }

        protected override void Initialize()
        {
            try
            {
                game.Initialize(Services, GraphicsDevice);

                tmrDrawElapsed = Stopwatch.StartNew();
                //
                spriteBatch = new SpriteBatch(GraphicsDevice);
                
                // From the example code, should this be a timer instead?
                Application.Idle += delegate { Invalidate(); };
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }
            
        }
        
        private void InitializeCameras()
        {
            cam = new Camera(new Vector3(0, 10.25f, 80.7f));
            cam.AdjustOrientation(-.07f, 0);
            cam.lagFactor = .07f;
            objectCam = new Camera(new Vector3(0, 0, 0));
            objectCam.lagFactor = 1.0f;
        }
        
        #endregion

        #region Methods
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


        #endregion

        #region User Input
        private void ProcessCameraControl(KeyEventArgs e)
        {/*
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
            }*/
        }

        private void ProcessObjectControlKeyDown(KeyEventArgs e)
        {/*
            Keys key = e.KeyCode;

            if (e.KeyCode == Keys.L)
            {
                lv = AddLunarLander(new Vector3(0, 3, 0), new Vector3(1.0f, 1.0f, 1.0f), Matrix.CreateRotationY((float)Math.PI), cubeModel);
            }

            if (lv != null)
            {
                lv.ProcessInputKeyDown(e);
            }

            if (key == Keys.C)
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
            }*/
        }
        private void ProcessObjectControlKeyUp(KeyEventArgs e)
        {/*
            if (lv != null)
                lv.ProcessInputKeyUp(e);


            Keys key = e.KeyCode;

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                carObject.Car.Accelerate = 0.0f;

            if (key == Keys.Left || key == Keys.Right)
                carObject.Car.Steer = 0.0f;

            if (key == Keys.B)
                carObject.Car.HBrake = 0.0f;
            */
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
        public void ProcessKeyDown(PreviewKeyDownEventArgs e)
        {/*
            Keys key = e.KeyCode;

            if (key == Keys.Up || key == Keys.Down)
            {
                if (e.KeyCode == Keys.Up)
                    carObject.Car.Accelerate = 1.0f;
                else
                    carObject.Car.Accelerate = -1.0f;
            }
            //else
            //    carObject.Car.Accelerate = 0.0f;

            if (key == Keys.Left || key == Keys.Right)
            {
                if (key == Keys.Left)
                    carObject.Car.Steer = 1.0f;
                else
                    carObject.Car.Steer = -1.0f;
            }
            //else
            //    carObject.Car.Steer = 0.0f;

            if (key == Keys.B)
                carObject.Car.HBrake = 1.0f;
            //else
            //    carObject.Car.HBrake = 0.0f;*/
        }
        public void ProcessKeyUp(KeyEventArgs e)
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
        public void ProcessMouseDown(MouseEventArgs e, System.Drawing.Rectangle bounds)
        {/*
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
                    if (b == null)
                        return;
                    Gobject go = b.ExternalData as Gobject;
                    SelectGameObject(go);
                }
            }*/
        }
       
        public void PanCam(float dX, float dY)
        {
            cam.AdjustOrientation(-dY*.001f,-dX*.001f);
        }
        #endregion
        /*
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
        #endregion*/

        #region Draw
        protected override void Draw()
        {
            try
            {
                Matrix proj = Matrix.Identity;
                GraphicsDevice.Clear(Color.Gray);

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

                /*
                if(DrawingEnabled)
                    if(terrain!=null)
                        terrain.Draw(GraphicsDevice, v, p);*/
                /*if(DebugPhysics)
                    if (terrain != null)
                        terrain.DrawWireframe(GraphicsDevice, v, p);*/
                //planeObj.DrawWireframe(GraphicsDevice, v,p);

                
                if (Debug)
                {
                    double time = tmrDrawElapsed.ElapsedMilliseconds;
                    spriteBatch.Begin();
                    Vector2 position = new Vector2(5, 5);
                    Color debugfontColor = Color.Black;
                    spriteBatch.DrawString(debugFont, "FPS: " + (1000.0 / time), position, debugfontColor);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "TPS: " + (1000.0 / lastPhysicsElapsed), position, debugfontColor); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraPosition", cam.TargetPosition);
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraOrientation", Matrix.CreateFromQuaternion(cam.Orientation).Forward);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "Objects Drawn: " + gameObjects.Count + "/" + ObjectsDrawn, position, debugfontColor);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "Cam Mode: " + cameraMode.ToString(), position, debugfontColor); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "Input Mode: " + inputMode.ToString(), position, debugfontColor); // physics Ticks Per Second

                    spriteBatch.End();

                    // Following 3 lines are to reset changes to graphics device made by spritebatch
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap; // Described as "may not be needed"

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
                ObjectsDrawn = 0;
                foreach (Gobject go in gameObjects)
                {
                    if (go is CarObject)
                    { 

                    }
                    Matrix view, proj;
                    GetCameraViewProjection(out view, out proj);
                    BoundingFrustum frustum = new BoundingFrustum(view * proj);
                    if (frustum.Contains(go.Skin.WorldBoundingBox) != ContainmentType.Disjoint)
                    {
                        ObjectsDrawn++;
                        if (DrawingEnabled)
                            go.Draw(view, proj);
                        if (DebugPhysics)
                            go.DrawWireframe(GraphicsDevice, view, proj);
                    }
                }
            }
        }


        private void GetCameraViewProjection(out Matrix view, out Matrix proj)
        {
            switch (cameraMode)
            {
                case CameraModes.Fixed:
                    view = cam.RhsLevelViewMatrix;
                    proj = cam._projection;
                    break;
                case CameraModes.ObjectFirstPerson:
                    /*
                    objectCam.SetOrientation(currentSelectedObject.Body.Orientation);
                    objectCam.TargetPosition = currentSelectedObject.Body.Position;
                    */
                    view = objectCam.RhsViewMatrix;
                    proj = objectCam._projection;
                    break;
                case CameraModes.ObjectChase:
                
                    Vector3 ThirdPersonRef = new Vector3(0, 1, 5);
                    /*
                    Vector3 TransRef = Vector3.Transform(ThirdPersonRef, currentSelectedObject.Body.Orientation);
                    objectCam.TargetPosition = TransRef + currentSelectedObject.Body.Position;
                    objectCam.LookAtLocation(currentSelectedObject.Body.Position);
                 */

                    view = objectCam.RhsViewMatrix;
                    proj = objectCam._projection;
                    break;
                case CameraModes.ObjectWatch:
                    /*
                    cam.LookAtLocation(currentSelectedObject.Body.Position);
                     */
                    view = cam.RhsLevelViewMatrix;
                    proj = cam._projection;
                     
                    break;
                default:
                    view = Matrix.Identity;
                    proj = Matrix.Identity;
                    break;
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
