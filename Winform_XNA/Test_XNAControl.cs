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
        public Vector3 CameraForward = new Vector3();

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

                CameraPosition = new Vector3(550, 0, 0);
                CameraForward = new Vector3(-1, 0, 0);
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
                1000.0f);
                
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

            //throw new NotImplementedException();
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


            /*
            
            float junk;
            Vector3 com;
            Matrix it;
            Matrix itCoM;
            
            
            Body sphere = new Body();
            sphere.CollisionSkin = new CollisionSkin(sphere);

            sphere.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Sphere(new Vector3(0, 600, 0), 5), (int)MaterialTable.MaterialID.BouncyNormal);
            sphere.Position = new Vector3(0, 500, 0);
            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Mass, 5);

            sphere.CollisionSkin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);
            sphere.BodyInertia = itCoM;
            sphere.Mass = junk;
            sphere.CollisionSkin.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            sphere.EnableBody();
             

            Body box = new Body();
            box.CollisionSkin = new CollisionSkin(box);
            box.CollisionSkin.AddPrimitive(new JigLibX.Geometry.Box(new Vector3(0, 30, 0), Matrix.Identity, new Vector3(5, 5, 5)), (int)MaterialTable.MaterialID.BouncyNormal);
            box.CollisionSkin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);
            box.BodyInertia = itCoM;
            box.Mass = junk;
            box.CollisionSkin.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            box.EnableBody();

            testBodies.Add(sphere);
            testBodies.Add(box);
            box.Immovable = true;
            PhysicsSystem.AddBody(sphere);
            PhysicsSystem.AddBody(box);*/
        }

        private void InitializePhysics()
        {
            testGobjects = new List<Gobject>();

            PhysicsSystem = new PhysicsSystem();
            PhysicsSystem.CollisionSystem = new CollisionSystemSAP();
            PhysicsSystem.SolverType = PhysicsSystem.Solver.Normal;
            //PhysicsSystem.AddController(Co

            
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
            //Game.Instance.Update();
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

                //Game.Instance.DebugView.RenderDebugData(ref proj);
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
            
            _view = Matrix.CreateLookAt(
                //new Vector3(550, 00, 300),
                new Vector3(CameraPosition.X, CameraPosition.Y, CameraPosition.Z),
                CameraPosition + CameraForward,
                Vector3.Up);

            //effectCam.Projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, .0001f, 10000);
            // Draw the triangle.
            //effectCam.CurrentTechnique.Passes[0].Apply();
             
            DrawObjects();

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

            //throw new NotImplementedException();
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

        Vector3 walkIncrement = Vector3.Forward;
        public void ProcessKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                CameraPosition += walkIncrement;
            }
        }

        internal void PanCam(float dX, float dY)
        {
            Matrix cam = Matrix.CreateRotationX(.001f * dX);
            cam *= Matrix.CreateRotationY(.001f * dY);

            CameraForward = Vector3.Transform(CameraForward, Matrix.CreateRotationX(.001f * dX));
            CameraForward = Vector3.Transform(CameraForward, Matrix.CreateRotationY(.001f * dY));
        }
    }
}
