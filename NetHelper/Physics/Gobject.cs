using JigLibX.Collision;
using JigLibX.Physics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Geometry;
using System.Collections.Generic;
using System;
using Helper.Physics.PhysicsObjects;
using Helper.Lighting;
using Helper.Physics.PhysicObjects;

namespace Helper.Physics
{
    public class Gobject
    {
        public int ID;
        public Body Body { get; internal set; }
        public CollisionSkin Skin { get; internal set; }
        public Model Model { get; set; }
        public Vector3 Position { get; set; }
        public bool isOnServer;
        public bool isOnClient;
        public int type;
        public Matrix Orientation
        {
            get
            {
                return Body.Orientation;
            }
            set
            {
                Body.Orientation = value;
            }

        }
        public Vector3 Scale { get; set; }
        public bool Selected;
        public Helper.Input.ActionManager actionManager = new Helper.Input.ActionManager();
        public List<Controller> controllers = new List<Controller>();
        internal BasicEffect Effect { get; set; }
        public bool hasNotDoneFirstInterpoladation = true; // this object has never processed an update and interpolated (with any factor) 

        /// <summary>
        /// Default Constructor
        /// Initalizes the Body and a CollisionSkin
        /// No Primatives are added to the Body
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        public Gobject()
        {
            Body = new Body();
            Skin = new CollisionSkin(Body);
            Body.CollisionSkin = Skin;
            Body.ExternalData = this;
            Body.CollisionSkin.callbackFn += new CollisionCallbackFn(CollisionSkin_callbackFn);

        }
        private bool CollidedRecently = true; // we want the object to update immediately once created

        bool CollisionSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            CollidedRecently = true; // we just want to know when it collided
            return true; // let the physics system handle the collision
        }

        public int UpdateCountdown = 10;
        public bool DidCollideRecently
        {
            get
            {
                if (CollidedRecently)
                {
                    CollidedRecently = false;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Single Primitive Constructor with custom MaterialProperty
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primative">Primitive to add to Skin</param>
        /// <param name="prop">Material Properties of Primitive</param>
        public Gobject(Vector3 position, Vector3 scale, Primitive primative, MaterialProperties prop, Model model, int asset)
            : this()
        {
            Skin.AddPrimitive(primative, prop);

            CommonInit(position, scale, model, true, asset);
        }

        /// <summary>
        /// Single Primitive Constructor with predefined MaterialProperty
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primative">Primitive to add to Skin</param>
        /// <param name="propId">Predefined Material Properties of Primitive</param>
        public Gobject(Vector3 position, Vector3 scale, Primitive primative, MaterialTable.MaterialID propId, Model model, int asset)
            : this()
        {
            Skin.AddPrimitive(primative, (int)propId);

            CommonInit(position, scale, model, true, asset);
        }

        /// <summary>
        /// Multiple Primitive Constructor
        /// Each Primitive needs a Material Property
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primatives">Primitives to add to Skin</param>
        /// <param name="props">Material Properties of Primitives to add</param>
        public Gobject(Vector3 position, Vector3 scale, List<Primitive> primatives, List<MaterialProperties> props, Model model, int asset)
            : this()
        {
            for (int i = 0; i < primatives.Count && i < props.Count; i++)
                Skin.AddPrimitive(primatives[i], props[i]);

            CommonInit(position, scale, model, true, asset);
        }

        public Gobject(Vector3 position, Vector3 scale, Primitive primitive, Model model, bool moveable, int asset)
            : this()
        {
            
            try
            {
                Skin.AddPrimitive(primitive, (int)MaterialTable.MaterialID.NotBouncyNormal);
                //CollisionSkin collision = new CollisionSkin(null);
                //Skin.AddPrimitive(primitive, 2);
                CommonInit(position, scale, model, moveable, asset);
                //Body.CollisionSkin = collision;
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }
        
        internal void CommonInit(Vector3 pos, Vector3 scale, Model model, bool moveable, int asset)
        {
            Position = pos;
            Scale = scale;
            Model = model;
            Body.Immovable = !moveable;
            type = asset;
            
            // MOVED TO BEFORE INTEGRATE
            //FinalizeBody();
        }

        public void AddCollisionCallback(CollisionCallbackFn cbf)
        {
            this.Body.CollisionSkin.callbackFn += cbf;
        }

        

        public void AddController(Controller c)
        {
            controllers.Add(c);
            PhysicsSystem.CurrentPhysicsSystem.AddController(c);
        }

        public Vector3 BodyPosition()
        {
            return Body.Position;
        }

        public Matrix BodyOrientation()
        {
            return Body.Orientation;
        }
        public void SetOrientation(Matrix o)
        {
            Body.Orientation = o;
        }

        public Vector3 BodyVelocity()
        {
            return Body.Velocity;
        }
        public void SetVelocity(Vector3 v)
        {
            Body.Velocity = v;
        }

        public virtual void FinalizeBody()
        {

            try
            {
                Vector3 com = SetMass(1.0f);

                Body.MoveTo(Position, Matrix.Identity);
                Skin.ApplyLocalTransform(new JigLibX.Math.Transform(-com, Matrix.Identity));
                Body.EnableBody(); // adds to CurrentPhysicsSystem
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        internal Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Mass,
                mass);

            float junk;
            Vector3 com;
            Matrix it, itCom;

            Skin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCom);            
            Body.BodyInertia = itCom;
            Body.Mass = junk;

            return com;
        }

        public virtual void Draw(ref Matrix View, ref Matrix Projection)
        {
            if (Model == null)
                return;
            Matrix[] transforms = new Matrix[Model.Bones.Count];

            Model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix worldMatrix = GetWorldMatrix();

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    if(Selected)
                        effect.AmbientLightColor = Color.Red.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
        }

        public virtual void DrawWireframe(GraphicsDevice Graphics, Matrix View, Matrix Projection)
        {
            try
            {
                VertexPositionColor[] wireFrame = Skin.GetLocalSkinWireframe();
                Body.TransformWireframe(wireFrame);
                if (Effect == null)
                {
                    Effect = new BasicEffect(Graphics);
                    Effect.VertexColorEnabled = true;
                }
                Effect.TextureEnabled = false;
                Effect.LightingEnabled = false;
                Effect.View = View;
                Effect.Projection = Projection;

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Graphics.DrawUserPrimitives<VertexPositionColor>(
                        Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                        wireFrame, 0, wireFrame.Length - 1);
                }

                VertexPositionColor[] Velocity = new VertexPositionColor[2];
                Velocity[0] = new VertexPositionColor(Body.Position, Color.Blue);
                Velocity[1] = new VertexPositionColor(Body.Position + Body.Velocity, Color.Red);

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Graphics.DrawUserPrimitives<VertexPositionColor>(
                        Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                        Velocity, 0, Velocity.Length - 1);
                }

                foreach (Controller c in controllers)
                {
                    if (c is BoostController)
                    {
                        VertexPositionColor[] Force = new VertexPositionColor[2];
                        BoostController bc = c as BoostController;
                        Force[0] = new VertexPositionColor(bc.ForcePosition, Color.Green);
                        Force[1] = new VertexPositionColor(bc.ForcePosition + (bc.Force * bc.forceMag), Color.Yellow);
                        if (!bc.worldForce)
                            Body.TransformWireframe(Force);

                        VertexBuffer verts = new VertexBuffer(Graphics, VertexPositionColor.VertexDeclaration, Force.Length, BufferUsage.WriteOnly);
                        verts.SetData(Force);

                        foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            Graphics.SetVertexBuffer(verts);
                            Graphics.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip, 0, Force.Length - 1);
                            /*Graphics.DrawUserPrimitives<VertexPositionColor>(
                                Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                                Force, 0, Force.Length - 1, LightingVertexFormat.VertexDeclaration);*/
                        }
                    }
                }

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// only used for the model
        /// </summary>
        /// <returns></returns>
        public Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Scale) * Skin.GetPrimitiveLocal(0).Transform.Orientation * Body.Orientation * Matrix.CreateTranslation(Body.Position);
        }

        

        public void MoveTo(Vector3 pos, Matrix orient)
        {
            Body.MoveTo(pos, orient);
        }
        public bool isMoveable
        {
            get
            {
                return !Body.Immovable;
            }
        }

        /// <summary>
        /// should be called after MoveTo
        /// </summary>
        /// <param name="vel"></param>
        public void UpdateVelocity(Vector3 vel)
        {
            Body.Velocity = vel;
            //Body.UpdateVelocity(vel.Length);
        }

        public void ProcessSimulatedInput(object[] actionvalues)
        {
            actionManager.ProcessActionValues(actionvalues);
        }

        public virtual Vector3 GetPositionAbove()
        {
            Vector3 ret = Body.Position;
            ret.Y += Math.Abs((Body.CollisionSkin.WorldBoundingBox.Min.Y + Body.CollisionSkin.WorldBoundingBox.Max.Y) / 2f); // Assume body is halfway in this?
            return ret;
        }

        /// <summary>
        /// CLIENT SIDE
        /// Interpolating Update with the default 50/50 interpolation
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="matrix"></param>
        /// <param name="vector3_2"></param>
        public void Interpoladate(Vector3 position, Matrix orientation, Vector3 velocity)
        {
            float interFactor = .5f; // assume simple interpolation

            bool VariableInterp=false;            
            if (VariableInterp)
            {
                float magicFactor = 10; // there's some magic number (varrying per game according to map/unit scale) 
                // the further away the object is from where it should be, the more we should make it where it should be
                // the magic number should be enough to counter-act gravity, perhaps.
                float distSq = Vector3.DistanceSquared(position, Position);
                interFactor = distSq / magicFactor;
            }

            // default interpolation factor is 50/50.
            Interpoladate(position, orientation, velocity, interFactor);
        }

        /// <summary>
        /// CLIENT SIDE
        /// Interpolating Update with a specified interpolation factor.
        /// Factor of 1.0 means make it so! 
        /// Factor of 0 means ignore the update. =(
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="matrix"></param>
        /// <param name="vector3_2"></param>
        public void Interpoladate(Vector3 position, Matrix orientation, Vector3 velocity, float interpFactor)
        {
            if (interpFactor > 1.0f)
                interpFactor = 1.0f;
            if (interpFactor < 0)
                interpFactor = 0;
            //MoveTo(position, orientation);
            //SetVelocity(velocity);
            Vector3 intPosition = BodyPosition() + (position - BodyPosition()) * interpFactor;
            Vector3 intvelocity = BodyVelocity() + (velocity - BodyVelocity()) * interpFactor;
            if(float.IsNaN(intPosition.X) || 
                float.IsNaN(intvelocity.X))
            {
                return;
            }
            MoveTo(intPosition, orientation);
            SetVelocity(intvelocity);
            hasNotDoneFirstInterpoladation = false;
            //SetVelocity(intvelocity);            
        }

        public virtual void SetNominalInput()
        {
            
        }

        public bool hasAttributeChanged = false;
        public virtual void GetObjectAttributes(out bool[] bv, out int[] iv, out float[] fv)
        {
            bv = null;
            iv = null;
            fv = null;
        }
        public virtual void SetObjectAttributes(bool[] bv, int[] iv, float[] fv, bool mine)
        {
        }

        public bool IsActive 
        {
            get
            {
                return Body.IsActive;
            }
        }
    }
}
