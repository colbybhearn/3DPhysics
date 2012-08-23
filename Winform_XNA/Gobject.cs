using JigLibX.Collision;
using JigLibX.Physics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Geometry;
using System.Collections.Generic;

namespace Winform_XNA
{
    class Gobject
    {
        public Body Body { get; private set; }
        public CollisionSkin Skin { get; private set; }
        public Model Model { get; set; }
        public Vector3 Position { get; private set; }
        public Vector3 Scale { get; private set; }

        /// <summary>
        /// Default Constructor
        /// Initalizes the Body and a CollisionSkin
        /// No Primatives are added to the Body
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        Gobject()
        {
            Body = new Body();
            Skin = new CollisionSkin(Body);
            Body.CollisionSkin = Skin;
        }

        /// <summary>
        /// Single Primitive Constructor with custom MaterialProperty
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primative">Primitive to add to Skin</param>
        /// <param name="prop">Material Properties of Primitive</param>
        public Gobject(Vector3 position, Vector3 scale, Primitive primative, MaterialProperties prop, Model model)
            : this()
        {
            Skin.AddPrimitive(primative, prop);

            CommonInit(position, scale, model, true);
        }

        /// <summary>
        /// Single Primitive Constructor with predefined MaterialProperty
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primative">Primitive to add to Skin</param>
        /// <param name="propId">Predefined Material Properties of Primitive</param>
        public Gobject(Vector3 position, Vector3 scale, Primitive primative, MaterialTable.MaterialID propId, Model model)
            : this()
        {
            Skin.AddPrimitive(primative, (int)propId);

            CommonInit(position, scale, model, true);
        }

        /// <summary>
        /// Multiple Primitive Constructor
        /// Each Primitive needs a Material Property
        /// </summary>
        /// <param name="position">Initial Body Position</param>
        /// <param name="scale">Scale</param>
        /// <param name="primatives">Primitives to add to Skin</param>
        /// <param name="props">Material Properties of Primitives to add</param>
        public Gobject(Vector3 position, Vector3 scale, List<Primitive> primatives, List<MaterialProperties> props, Model model)
            : this()
        {
            for (int i = 0; i < primatives.Count && i < props.Count; i++)
                Skin.AddPrimitive(primatives[i], props[i]);

            CommonInit(position, scale, model, true);
        }

        public Gobject(Vector3 position, Vector3 scale, Primitive primitive, Model model, bool moveable)
            : this()
        {
            Skin.AddPrimitive(primitive, (int) MaterialTable.MaterialID.BouncyNormal);
            CommonInit(position, scale, model, moveable);
        }

        private void CommonInit(Vector3 pos, Vector3 scale, Model model, bool moveable)
        {
            Position = pos;
            Scale = scale;
            Model = model;
            Body.Immovable = !moveable;
            FinalizeBody();
        }



        private void FinalizeBody()
        {
            Vector3 com = SetMass(1.0f);

            Body.MoveTo(Position, Matrix.Identity);

            Skin.ApplyLocalTransform(new JigLibX.Math.Transform(-com, Matrix.Identity));
            Body.EnableBody();

            PhysicsSystem.CurrentPhysicsSystem.AddBody(Body);
        }

        private Vector3 SetMass(float mass)
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

        public void Draw(Matrix View, Matrix Projection)
        {
            Matrix[] transforms = new Matrix[Model.Bones.Count];

            Model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix worldMatrix = GetWorldMatrix();

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
        }

        private Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Scale) * Skin.GetPrimitiveLocal(0).Transform.Orientation * Body.Orientation * Matrix.CreateTranslation(Body.Position);
        } //
    }
}
