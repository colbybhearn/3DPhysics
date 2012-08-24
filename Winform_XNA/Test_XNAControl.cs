using System;
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
    class Test_XNAControl : XNAControl
    {
        #region Todo
        /*
         * Add Primitive Models
         * Refactor Camera control
         * Refactor Physics Controllers
         */
        #endregion

        #region Camera
        Matrix _view;
        Matrix _projection;
        public Vector3 camPosition = new Vector3();
        public Quaternion camOrientation;
        float camSpeed = 10;
        float camSpeedChangeRate = 1.2f;
        #endregion

        #region Content
        public ContentManager Content { get; private set; }
        Model cubeModel;
        Model sphereModel;
        #endregion

        #region Debug
        private SpriteBatch spriteBatch;
        private SpriteFont debugFont;        
        public bool Debug { get; set; }
        #endregion

        #region Physics
        BoostController bController = new BoostController();
        public PhysicsSystem PhysicsSystem { get; private set; }
        private System.Timers.Timer tmrPhysicsUpdate;
        #endregion

        #region Game
        private Stopwatch tmrElapsed;
        private List<Gobject> gameObjects;
        
        double TIME_STEP = .01; // Recommended timestep
        #endregion

        #region Init
        protected override void Initialize()
        {
            Content = new ContentManager(Services, "content");

            try
            {
                InitializePhysics();
                InitializeObjects();

                camPosition = new Vector3(0, 0, 80);
                camOrientation = Quaternion.Identity;

                tmrElapsed = Stopwatch.StartNew();
                spriteBatch = new SpriteBatch(GraphicsDevice);
                
                new Game();
                debugFont = Content.Load<SpriteFont>("DebugFont");
                
                _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float)GraphicsDeviceManager.DefaultBackBufferWidth / (float)GraphicsDeviceManager.DefaultBackBufferHeight,
                0.1f,
                5000.0f);
                
                // From the example code, should this be a timer instead?
                Application.Idle += delegate { Invalidate(); };

                tmrPhysicsUpdate = new System.Timers.Timer();
                tmrPhysicsUpdate.AutoReset = false;
                tmrPhysicsUpdate.Enabled = false;
                tmrPhysicsUpdate.Interval = 10;
                tmrPhysicsUpdate.Elapsed += new System.Timers.ElapsedEventHandler(tmrPhysicsUpdate_Elapsed);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
        private void InitializeObjects()
        {
            cubeModel = Content.Load<Model>("Cube");
            sphereModel = Content.Load<Model>("Sphere");
            AddSphere(new Vector3(0, -2, 0), 1f, sphereModel, true);
            AddBox(new Vector3(0, 0, 0), new Vector3(.5f, .5f, .5f), cubeModel, false);
        }
        private void InitializePhysics()
        {
            gameObjects = new List<Gobject>();

            PhysicsSystem = new PhysicsSystem();
            PhysicsSystem.CollisionSystem = new CollisionSystemSAP();
            PhysicsSystem.SolverType = PhysicsSystem.Solver.Normal;
            PhysicsSystem.Gravity = new Vector3(0, -9.8f, 0);

            

        }
        #endregion

        #region Methods
        private void AddBox(Vector3 pos, Vector3 size, Model model, bool moveable)
        {
            Box boxPrimitive = new Box(pos, Matrix.Identity, size);
            Gobject box = new Gobject(
                pos,
                size,
                boxPrimitive,
                model,
                moveable
                );

           
            gameObjects.Add(box);
        }
        private void AddSphere(Vector3 pos, float radius, Model model, bool moveable)
        {
            Sphere spherePrimitive = new Sphere(pos, radius);
            Gobject sphere = new Gobject(
                pos,
                Vector3.One*radius*1.0f,
                spherePrimitive,
                model,
                moveable);
            gameObjects.Add(sphere);
            
            if(PhysicsSystem.Controllers.Contains(bController))
                PhysicsSystem.RemoveController(bController);
            bController.Initialize(sphere.Body);
            bController.DisableController();
            PhysicsSystem.AddController(bController);
        }
        #endregion

        #region User Input
        public void ProcessKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                camSpeed *= camSpeedChangeRate;
            }
            if (e.KeyCode == Keys.Z)
            {
                camSpeed /= camSpeedChangeRate;
            }
            if (e.KeyCode == Keys.W)
            {
                camPosition += GetLevelCameraLhs.Forward * camSpeed;
            }
            if (e.KeyCode == Keys.A)
            {
                camPosition += GetLevelCameraLhs.Left * camSpeed;
            }
            if (e.KeyCode == Keys.S)
            {
                camPosition += GetLevelCameraLhs.Backward * camSpeed;
            }
            if (e.KeyCode == Keys.D)
            {
                 camPosition += GetLevelCameraLhs.Right * camSpeed;
            }
            if (e.KeyCode == Keys.N)
                AddSphere();
            if (e.KeyCode == Keys.B)
            {
                bController.EnableController();
            }

        }
        private void AddSphere()
        {
            AddSphere(new Vector3(0, 300, 0), .5f, sphereModel, true);
        }
        internal void PanCam(float dX, float dY)
        {
            Quaternion cameraChange =
            Quaternion.CreateFromAxisAngle(Vector3.UnitX, -dY * .001f) *
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, -dX * .001f);
            //Quaternion.CreateFromAxisAngle(GetLevelCameraLhs.Right, -dY * .001f) *
            //Quaternion.CreateFromAxisAngle(Vector3.UnitY, -dX * .001f);
            camOrientation = camOrientation * cameraChange;
        }
        #endregion

        public void ResetTimer()
        {
            tmrPhysicsUpdate.Stop();
            tmrPhysicsUpdate.Start();
        }
        void tmrPhysicsUpdate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Should use a variable timerate to keep up a steady "feel" if we bog down?
            PhysicsSystem.CurrentPhysicsSystem.Integrate((float)TIME_STEP);


            tmrPhysicsUpdate.Stop();
            tmrPhysicsUpdate.Start();
        }


        public Matrix GetLevelCameraLhs
        {
            get
            {
                Vector3 camRotation = Matrix.CreateFromQuaternion(camOrientation).Forward;
                // Side x camRotation gives the correct Up vector WITHOUT roll, if you do -Z,0,X instead, you will be upsidedown
                // There is still an issue when nearing a "1" in camRotation in the positive or negative Y, in that it rotates weird,
                // This does not appear to be related to the up vector.
                Vector3 side = new Vector3(camRotation.Z, 0, -camRotation.X);
                Vector3 up = Vector3.Cross(camRotation, side);
                Matrix m = Matrix.CreateLookAt(
                    camPosition,
                    camPosition + camRotation,
                    up);
                return Matrix.Invert(m);
            }
        }

        #region Draw
        protected override void Draw()
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

            Vector3 cameraOriginalTarget = Vector3.Forward;
            Vector3 cameraOriginalUpVector = Vector3.Up;

            //Vector3 camRotation = Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(camOrientation));
            // ^ is the same as v - I thought it was simplier to use v
            Vector3 camRotation = Matrix.CreateFromQuaternion(camOrientation).Forward;
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, camOrientation);
            Vector3 cameraFinalTarget = camPosition + cameraRotatedTarget;
            //Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, camOrientation);

            // Side x camRotation gives the correct Up vector WITHOUT roll, if you do -Z,0,X instead, you will be upsidedown
            // There is still an issue when nearing a "1" in camRotation in the positive or negative Y, in that it rotates weird,
            // This does not appear to be related to the up vector.
            Vector3 side = new Vector3(camRotation.Z, 0, -camRotation.X);
            // camera orientation points in the camera Z axis
            // to get a perpendicular vector, swap components and negate one
            // this side vector points left initially (-1, 0, 0)
            // moving to the right with D
            Vector3 up = Vector3.Cross(camRotation, side);
            _view = Matrix.CreateLookAt(
                camPosition,
                camPosition + camRotation,
                up);

            DrawObjects();

            if (Debug)
            {
                try
                {

                    double time = tmrElapsed.ElapsedMilliseconds;
                    spriteBatch.Begin();
                    Vector2 position = new Vector2(5, 5);
                    spriteBatch.DrawString(debugFont, "Debug Text: Enabled", position, Color.LightGray);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "FPS: " + (1000.0 / time), position, Color.LightGray);
                    position.Y += debugFont.LineSpacing;
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraPosition", camPosition);
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraOrientation", Matrix.CreateFromQuaternion(camOrientation).Forward);
                    spriteBatch.End();
                    tmrElapsed.Restart();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
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
            foreach (Gobject go in gameObjects)
            {
                go.Draw(_view, _projection);
            }
        }
        #endregion

        internal void ProcessKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.B)
            {
                bController.DisableController();
            }
        }
    }
}
