using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Geometry;
using JigLibX.Physics;
using JigLibX.Collision;

namespace JiggleGame.PhysicObjects
{
    class TriangleMeshObject : PhysicObject
    {
        TriangleMesh triangleMesh;

        public TriangleMeshObject(Game game, Model model, Matrix orientation, Vector3 position)
            : base(game, model)
        {
            body = new Body();
            collision = new CollisionSkin(null);

            triangleMesh = new TriangleMesh();

            List<Vector3> vertexList = new List<Vector3>();
            List<TriangleVertexIndices> indexList = new List<TriangleVertexIndices>();

            ExtractData(vertexList, indexList, model);

            triangleMesh.CreateMesh(vertexList,indexList, 4, 1.0f);
            collision.AddPrimitive(triangleMesh,new MaterialProperties(0.8f,0.7f,0.6f));
            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.AddCollisionSkin(collision);
        }


        // Extracts the neccesary information from a model to
        // simulate physics on it
        public void ExtractData(List<Vector3> vertices, List<TriangleVertexIndices> indices, Model model)
        {
            Matrix[] bones_ = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones_);
            foreach (ModelMesh mm in model.Meshes)
            {
                int offset = vertices.Count;
                Matrix xform = bones_[mm.ParentBone.Index];
                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    Vector3[] a = new Vector3[mmp.NumVertices];
                    int stride = mmp.VertexBuffer.VertexDeclaration.VertexStride;

                    byte[] bRaw = new byte[mmp.VertexBuffer.VertexCount * mmp.VertexBuffer.VertexDeclaration.VertexStride];
                    mmp.VertexBuffer.GetData<byte>(bRaw);

                    for (int i = 0; i < mmp.NumVertices; i++)
                    {
                        int iStart = (i + mmp.VertexOffset) * stride;
                        a[i] = new Vector3(BitConverter.ToSingle(bRaw, iStart), BitConverter.ToSingle(bRaw, iStart + 4), BitConverter.ToSingle(bRaw, iStart + 8));
                    }

                    for (int i = 0; i != a.Length; ++i)
                        Vector3.Transform(ref a[i], ref xform, out a[i]);
                    vertices.AddRange(a);

                    if (mmp.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                        throw new Exception(String.Format("Model uses 32-bit indices, which are not supported."));

                    short[] s = new short[mmp.PrimitiveCount * 3];
                    mmp.IndexBuffer.GetData(mmp.StartIndex * 2, s, 0, mmp.PrimitiveCount * 3);

                    JigLibX.Geometry.TriangleVertexIndices[] tvi = new JigLibX.Geometry.TriangleVertexIndices[mmp.PrimitiveCount];
                    for (int i = 0; i != tvi.Length; ++i)
                    {
                        tvi[i].I0 = s[i * 3 + 2] + offset;
                        tvi[i].I1 = s[i * 3 + 1] + offset;
                        tvi[i].I2 = s[i * 3 + 0] + offset;
                    }
                    indices.AddRange(tvi);
                }
            }
        }

        
        public override void ApplyEffects(BasicEffect effect)
        {
            effect.DiffuseColor = Vector3.One * 0.8f;
        }
    }
}
