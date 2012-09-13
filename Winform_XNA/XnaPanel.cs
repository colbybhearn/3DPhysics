using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Helper;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physics;


namespace XnaView
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
        Matrix view = Matrix.Identity;
        Matrix proj = Matrix.Identity;

        #region Debug
        private SpriteBatch spriteBatch;
        private SpriteFont debugFont;
        public bool Debug
        {
            get { return game.debug; }
            set { game.debug = value; }
        }
        public bool DebugPhysics { get; set; } 
        public bool DrawingEnabled { get; set; }
        public bool PhysicsEnabled { get; set; }
        private int ObjectsDrawn { get; set; }
        #endregion

        #region Physics
        public PhysicsSystem PhysicsSystem { get; private set; }
        #endregion

        #region Game
        private Stopwatch tmrDrawElapsed;
        private List<Physics.Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        private List<Physics.Gobject> newObjects;
        Game.BaseGame game;
        #endregion
        
        #endregion

        #region Init
        public XnaPanel() 
        {
            gameObjects = new List<Gobject>();
            newObjects = new List<Gobject>();
        }
        public XnaPanel(ref Game.BaseGame g)
        {
            game = g;
            PhysicsSystem = g.physicsManager.PhysicsSystem;
            gameObjects = g.gameObjects;
            newObjects = g.newObjects;
        }
        protected override void Initialize()
        {
            try
            {
                game.Initialize(Services, GraphicsDevice, UpdateCamera);

                tmrDrawElapsed = Stopwatch.StartNew();
                //
                spriteBatch = new SpriteBatch(GraphicsDevice);
                debugFont = game.Content.Load<SpriteFont>("DebugFont");
                
                // From the example code, should this be a timer instead?
                Application.Idle += delegate { Invalidate(); };
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }
            
        }
        #endregion

        #region Camera
        public void UpdateCamera(Camera c, Matrix v, Matrix p)
        {
            cam = c;
            view = v;
            proj = p;
        }
        #endregion

        #region Mouse Input
        public void ProcessMouseDown(MouseEventArgs e, System.Drawing.Rectangle bounds)
        {
            try
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
                        if (b == null)
                            return;
                        Gobject go = b.ExternalData as Gobject;
                        game.SelectGameObject(go);
                    }
                }
            }
            catch (Exception E)
            {
            }
        }
        public void PanCam(float dX, float dY)
        {
            game.AdjustCameraOrientation(-dY*.001f,-dX*.001f);
        }
        #endregion

        #region Draw
        protected override void Draw()
        {
            try
            {
                Matrix proj = Matrix.Identity;
                GraphicsDevice.Clear(Color.Gray);

                DrawObjects();
                
                /*
                if(DrawingEnabled)
                    if(terrain!=null)
                        terrain.Draw(GraphicsDevice, v, p);*/
                /*if(DebugPhysics)
                    if (terrain != null)
                        terrain.DrawWireframe(GraphicsDevice, v, p);*/

                // SpriteBatch drawing!
                spriteBatch.Begin();

                game.Draw(spriteBatch);

                if (Debug)
                {
                    double time = tmrDrawElapsed.ElapsedMilliseconds;
                    Vector2 position = new Vector2(5, 5);
                    Color debugfontColor = Color.Black;
                    spriteBatch.DrawString(debugFont, "FPS: " + (1000.0 / time), position, debugfontColor);
                    position.Y += debugFont.LineSpacing;
                    //spriteBatch.DrawString(debugFont, "TPS: " + (1000.0 / lastPhysicsElapsed), position, debugfontColor); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraPosition", cam.TargetPosition);
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraOrientation", Matrix.CreateFromQuaternion(cam.Orientation).Forward);
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "Objects Drawn: " + gameObjects.Count + "/" + ObjectsDrawn, position, debugfontColor);
                    position.Y += debugFont.LineSpacing;
                    //spriteBatch.DrawString(debugFont, "Cam Mode: " + cameraMode.ToString(), position, debugfontColor); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;
                    //spriteBatch.DrawString(debugFont, "Input Mode: " + inputMode.ToString(), position, debugfontColor); // physics Ticks Per Second

                    tmrDrawElapsed.Restart();
                }

                spriteBatch.End();

                // Following 3 lines are to reset changes to graphics device made by spritebatch
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap; // Described as "may not be needed"
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
            try
            {
                lock (gameObjects)
                {
                    
                    ObjectsDrawn = 0;
                    foreach (Gobject go in gameObjects)
                    {
                        BoundingFrustum frustum = new BoundingFrustum(view * proj);
                        if (frustum.Contains(go.Skin.WorldBoundingBox) != ContainmentType.Disjoint)
                        {
                            ObjectsDrawn++;
                            if (DrawingEnabled)
                                go.Draw(ref view, ref proj);
                            if (DebugPhysics)
                                go.DrawWireframe(GraphicsDevice, view, proj);
                        }
                    }
                }
            }
            catch (Exception E)
            {

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
