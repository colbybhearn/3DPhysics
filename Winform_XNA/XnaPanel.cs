using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Helper.Camera.Cameras;
using Helper.Collections;
using Helper.Physics;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Helper.Lighting;
using Helper.Physics.PhysicsObjects;
using Helper.Physics.PhysicObjects;
using Helper.Objects;

namespace XnaView
{

    /* I had to reference the WindowsGameLibrary from Clientapp in order for the ContentManager to load any models when invoked from the client
     * (it worked fine in XNA_Panel and the missing reference was the only difference)
     */
    public class XnaPanel : XnaControl
    {
        #region Fields
        BaseCamera cam;
        Matrix view = Matrix.Identity;
        Matrix proj = Matrix.Identity;

        #region Debug
        private SpriteBatch spriteBatch;
        private SpriteFont debugFont;
        public bool Debug
        {
            get { return game.DebugInfo; }
            set { game.DebugInfo = value; }
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
        private SortedList<int, Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        private SortedList<int, Gobject> newObjects;
        Game.BaseGame game;
        #endregion
        
        #endregion

        #region Init
        PointLight pl;
        public XnaPanel(ref Game.BaseGame g)
        {
            pl = new PointLight();
            game = g;
            PhysicsSystem = g.physicsManager.PhysicsSystem;
            gameObjects = g.gameObjects;
            newObjects = g.objectsToAdd;
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
                System.Diagnostics.Trace.WriteLine(e.StackTrace);
            }
        }
        #endregion

        #region Camera
        public void UpdateCamera(BaseCamera c, Matrix v, Matrix p)
        {
            cam = c;
            view = v;
            proj = p;
        }
        #endregion

        #region Mouse Input
        
        
        #endregion

        #region Draw
        protected override void Draw()
        {
            try
            {
                Counter.AddTick("fps");
                //Counter.AddTick("average_fps", Counter.GetAveragePerSecond("fps"));

                Matrix proj = Matrix.Identity;
                GraphicsDevice.Clear(Color.SkyBlue);

                DrawObjects();

                
                
                // SpriteBatch drawing!
                
                spriteBatch.Begin();
                
                game.Draw(spriteBatch);

                if (Debug)
                {
                    double time = tmrDrawElapsed.ElapsedMilliseconds;
                    Vector2 position = new Vector2(5, 50);
                    Color debugfontColor = Color.LightGray;
                    spriteBatch.DrawString(debugFont, "FPS: " + String.Format("{0:0}", Counter.GetAveragePerSecond("fps")), position, debugfontColor);
                    position.Y += debugFont.LineSpacing;

                    spriteBatch.DrawString(debugFont, "Physics Updates PS: " + String.Format("{0:0}", Counter.GetAveragePerSecond("pups")), position, debugfontColor); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;

                    spriteBatch.DrawString(debugFont, "Packets PS Out: " + String.Format("{0:0.0}", Counter.GetAveragePerSecond("pps_out")), position, debugfontColor); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;
                    spriteBatch.DrawString(debugFont, "Packets PS In: " + String.Format("{0:0.0}", Counter.GetAveragePerSecond("pps_in")), position, debugfontColor); // physics Ticks Per Second
                    position.Y += debugFont.LineSpacing;

                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraPosition", cam.TargetPosition);
                    position = DebugShowVector(spriteBatch, debugFont, position, "CameraOrientation", Matrix.CreateFromQuaternion(cam.Orientation).Forward);

                    spriteBatch.DrawString(debugFont, "Objects Drawn: " + ObjectsDrawn + "/" + gameObjects.Count, position, debugfontColor);
                    position.Y += debugFont.LineSpacing;

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
                //System.Console.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
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
                    foreach (Gobject go in gameObjects.Values)
                    {
                        BoundingFrustum frustum = new BoundingFrustum(view * proj);
                        if (frustum.Contains(go.Skin.WorldBoundingBox) != ContainmentType.Disjoint)
                        {
                            ObjectsDrawn++;
                            if (DrawingEnabled)
                            {
                                if (go is Terrain)
                                {
                                    (go as Terrain).Draw(GraphicsDevice, view, proj);
                                }
                                else if (go is Planet)
                                {
                                    (go as Planet).Draw(GraphicsDevice, view, proj);
                                }
                                else
                                    go.Draw(ref view, ref proj);
                            }
                            if (game.DebugPhysics)
                                go.DrawWireframe(GraphicsDevice, view, proj);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }
        #endregion
    }

}
