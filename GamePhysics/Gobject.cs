﻿using JigLibX.Collision;
using JigLibX.Physics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Geometry;
using System.Collections.Generic;
using System;
using Physics.PhysicsObjects;

namespace Physics
{
    public class Gobject
    {
        public int ID;
        public Body Body { get; internal set; }
        public CollisionSkin Skin { get; internal set; }
        public Model Model { get; set; }
        public Vector3 Position { get; internal set; }
        public Vector3 Scale { get; private set; }
        public bool Selected;
        public Helper.Input.ActionManager actionManager = new Helper.Input.ActionManager();

        internal BasicEffect Effect { get; set; }

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
        }

        /// <summary>
        /// Single Primitive Constructor with custom MaterialProperty
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primative">Primitive to add to Skin</param>
        /// <param name="prop">Material Properties of Primitive</param>
        public Gobject(Vector3 position, Vector3 scale, Primitive primative, MaterialProperties prop, Model model, string asset)
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
        public Gobject(Vector3 position, Vector3 scale, Primitive primative, MaterialTable.MaterialID propId, Model model, string asset)
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
        public Gobject(Vector3 position, Vector3 scale, List<Primitive> primatives, List<MaterialProperties> props, Model model, string asset)
            : this()
        {
            for (int i = 0; i < primatives.Count && i < props.Count; i++)
                Skin.AddPrimitive(primatives[i], props[i]);

            CommonInit(position, scale, model, true, asset);
        }

        public Gobject(Vector3 position, Vector3 scale, Primitive primitive, Model model, bool moveable, string asset)
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
            }
        }
        public string Asset;
        internal void CommonInit(Vector3 pos, Vector3 scale, Model model, bool moveable, string asset)
        {
            Position = pos;
            Scale = scale;
            Model = model;
            Body.Immovable = !moveable;
            Asset = asset;
            
            // MOVED TO BEFORE INTEGRATE
            //FinalizeBody();
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
            if (this is PlaneObject)
            {
            }
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
                Velocity[0] = new VertexPositionColor(Body.Position, Color.Black);
                Velocity[1] = new VertexPositionColor(Body.Position + Body.Velocity, Color.Blue);

                foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Graphics.DrawUserPrimitives<VertexPositionColor>(
                        Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
                        Velocity, 0, Velocity.Length - 1);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
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
    }
}
