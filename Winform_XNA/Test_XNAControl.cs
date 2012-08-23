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

        Matrix _view;
        Matrix _projection;
        Model bullet;


        private System.Timers.Timer t;
        private Stopwatch timer;

        private SpriteBatch spriteBatch;

        private List<Gobject> testGobjects;

        private SpriteFont debugFont;
        public Vector3 CameraPosition = new Vector3();
        public Quaternion CameraOrientation;
        /// <summary>
        /// Tells the control to draw Debug information
        /// </summary>
        public bool Debug { get; set; }

        public ContentManager Content { get; private set; }

        protected override void Initialize()
        {
            Content = new ContentManager(Services, "content");

            // Potential timers for drawing
            // Any GraphicsDevice effects
            try
            {
                InitializePhysics();
                InitializeObjects();

                CameraPosition = new Vector3(0, 0, 800);
                CameraOrientation = Quaternion.Identity;

                timer = Stopwatch.StartNew();
                spriteBatch = new SpriteBatch(GraphicsDevice);
                
                new Game();
                debugFont = Content.Load<SpriteFont>("DebugFont");
                
                effectCam = new BasicEffect(GraphicsDevice);
                effectCam.VertexColorEnabled = true;
                
                _projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float)GraphicsDeviceManager.DefaultBackBufferWidth / (float)GraphicsDeviceManager.DefaultBackBufferHeight,
                0.1f,
                5000.0f);
                
                // From the example code, should this be a timer instead?
                Application.Idle += delegate { Invalidate(); };

                t = new System.Timers.Timer();
                t.AutoReset = false;
                t.Enabled = false;
                t.Interval = 10;
                t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            }
            catch (Exception e)
            {
            }
        }
        public PhysicsSystem PhysicsSystem { get; private set; }

        private void InitializeObjects()
        {
            bullet = Content.Load<Model>("bullet");


            Sphere spherePrimitive = new Sphere(new Vector3(0, 600, 0), 5);
            Gobject sphere = new Gobject(
                new Vector3(0, 500, 0),
                Vector3.One,
                spherePrimitive,
                MaterialTable.MaterialID.BouncyNormal);

            sphere.Model = bullet;
            testGobjects.Add(sphere);


            Box boxPrimitive = new Box(new Vector3(0, 30, 0), Matrix.Identity, new Vector3(5, 5, 5));
            Gobject box = new Gobject(
                Vector3.Zero,
                Vector3.One,
                boxPrimitive,
                MaterialTable.MaterialID.BouncyNormal);

            box.Model = bullet;
            box.Body.Immovable = true;
            testGobjects.Add(box);
        }

        private void InitializePhysics()
        {
            testGobjects = new List<Gobject>();

            PhysicsSystem = new PhysicsSystem();
            PhysicsSystem.CollisionSystem = new CollisionSystemSAP();
            PhysicsSystem.SolverType = PhysicsSystem.Solver.Normal;
            PhysicsSystem.Gravity = new Vector3(0,-9.8f,0);
        }

        public void WireUpTimer()
        {
            t.Enabled = true;
            t.Stop();
            t.Start();
        }
        
        double TIME_STEP = .01; // Recommended timestep

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // every 10 milliseconds
            // Should use a variable timerate to keep up a steady "feel"?
            PhysicsSystem.CurrentPhysicsSystem.Integrate((float)TIME_STEP);


            t.Stop();
            t.Start();
        }

        BasicEffect effectCam;
        protected override void Draw()
        {
            Matrix proj = Matrix.Identity;
            GraphicsDevice.Clear(Color.Gray);

            double time = timer.ElapsedMilliseconds;
            if (Debug)
            {
                try
                {
                    spriteBatch.Begin();
                    Vector2 position = new Vector2(5, 5);
                    spriteBatch.DrawString(debugFont, "Debug Text: Enabled", position, Color.LightGray);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "FPS: " + (1000.0 / time), position, Color.LightGray);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.End();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }

            timer.Restart();

            /*
            // Set transform matrices.
            float aspect = GraphicsDevice.Viewport.AspectRatio;

            /*
            float yaw = 0;// (float)time * 0.7f;
            float pitch = 0;//time * 0.8f;
            float roll = 0;// time * 0.9f;
            
            effectCam.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            effectCam.View = Matrix.CreateLookAt(new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z),
                                              new Vector3(CameraPosition.X, CameraPosition.Y - 1, CameraPosition.Z), Vector3.Forward);
            */


            //effectCam.Projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, .0001f, 10000);
            // Draw the triangle.
            //effectCam.CurrentTechnique.Passes[0].Apply();

            

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

            Vector3 camOrientation = Vector3.Transform(Vector3.Forward, Matrix.CreateFromQuaternion(CameraOrientation));
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, CameraOrientation);
            Vector3 cameraFinalTarget = CameraPosition + cameraRotatedTarget;
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, CameraOrientation);

            Vector3.Clamp(cameraRotatedUpVector, new Vector3(-1, 0, -1), new Vector3(1, 1, 1)); ;
            _view = Matrix.CreateLookAt(
                //new Vector3(550, 00, 300),
                CameraPosition,
                CameraPosition + camOrientation,
                cameraRotatedUpVector);

            DrawObjects();
        }

        internal void PanCam(float dX, float dY)
        {
            Quaternion cameraChange =
            Quaternion.CreateFromAxisAngle(Vector3.UnitX, -dY * .001f) *
            Quaternion.CreateFromAxisAngle(Vector3.UnitY, -dX * .001f);
            CameraOrientation = CameraOrientation * cameraChange;
        }

        float walkSpeed=10;
        float walkChangeRate = 1.2f;
        public void ProcessKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Q)
            {
                walkSpeed *= walkChangeRate;
            }
            if (e.KeyCode == Keys.Z)
            {
                walkSpeed /= walkChangeRate;
            }
            if (e.KeyCode == Keys.W)
            {
                CameraPosition += Vector3.Transform(Vector3.Forward, CameraOrientation) * walkSpeed;
            }            
            if (e.KeyCode == Keys.A)
            {
                CameraPosition += Vector3.Transform(Vector3.Left, CameraOrientation) * walkSpeed;
            }
            if (e.KeyCode == Keys.S)
            {
                CameraPosition += Vector3.Transform(Vector3.Backward, CameraOrientation) * walkSpeed;
            }
            if (e.KeyCode == Keys.D)
            {
                CameraPosition += Vector3.Transform(Vector3.Right, CameraOrientation) * walkSpeed;
            }
        }

        public void DrawObjects()
        {
            //sb.Begin();

            //sb.End();
            //TEST CODE

            foreach (Gobject go in testGobjects)
            {
                go.Draw(_view, _projection);
            }

            /*
            foreach (Body b in testBodies)
            {
                Matrix[] transforms = new Matrix[bullet.Bones.Count];
                bullet.CopyAbsoluteBoneTransformsTo(transforms);


                Matrix worldMatrix = Matrix.CreateScale(Vector3.One) *
                                    b.CollisionSkin.GetPrimitiveLocal(0).Transform.Orientation *
                                    b.Orientation *
                                    Matrix.CreateTranslation(b.Position);

                foreach (ModelMesh mesh in bullet.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                        effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                        effect.View = _view;
                        effect.Projection = _projection;
                    }
                    mesh.Draw();
                }

            }*/
            //END TEST
        }

    }
}
