using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using JigLibX.Physics;
using JigLibX.Collision;
using Microsoft.Xna.Framework.Graphics;

namespace Helper.Physics.PhysicObjects
{
    public class Planet : Gobject
    {
        List<TriangleMesh> triangleMeshes = new List<TriangleMesh>();
        List<List<TriangleVertexIndices>> meshesIndices = new List<List<TriangleVertexIndices>>();
        List<VertexPositionNormalTexture[]> verts = new List<VertexPositionNormalTexture[]>();
        List<int[]> indices = new List<int[]>();
        Texture2D texture;

        public Planet(Vector3 center, Vector3 radius, double maxDeviation, double radianMeshSize, int subdivides, GraphicsDevice g, Texture2D texture)
            : base()
        {
            this.texture = texture;
            Body = new Body();
            Skin = new CollisionSkin(null);

            for (double i = 0; i < MathHelper.TwoPi - JigLibX.Math.JiggleMath.Epsilon; i += radianMeshSize)
            {
                for (double j = 0; j < MathHelper.TwoPi - JigLibX.Math.JiggleMath.Epsilon; j += radianMeshSize)
                {
                    //TriangleMesh tm = CreateSphericalMeshSegment(i, i + radianMeshSize, j, j + radianMeshSize, trianglesPerMesh, maxDeviation);
                    TriangleMesh tm = new TriangleMesh();
                    List<Vector3> vl = new List<Vector3>();
                    // Polar -> Cartesion in 3-Spcae
                    // X = R * Sin(Θ) * Cos(ϕ)
                    // Y = R * Sin(Θ) * Sin(ϕ)
                    // Z = R * Cos(Θ)

                    vl.Add(SphericalToCartesian(i, j, radius));
                    vl.Add(SphericalToCartesian(i + radianMeshSize, j, radius));
                    vl.Add(SphericalToCartesian(i + radianMeshSize, j + radianMeshSize, radius));
                    vl.Add(SphericalToCartesian(i, j + radianMeshSize, radius));
                    List<TriangleVertexIndices> tvi = new List<TriangleVertexIndices>();
                    tvi.Add(new TriangleVertexIndices(0, 1, 2));
                    tvi.Add(new TriangleVertexIndices(1, 3, 2));

                    VertexPositionNormalTexture[] v = new VertexPositionNormalTexture[4];
                    Vector3 tmp = SphericalToCartesian(i, j, Vector3.One);
                    v[0].Position = tmp * radius;
                    v[0].Normal = Vector3.Zero;
                    v[0].TextureCoordinate.X = tmp.X;
                    v[0].TextureCoordinate.Y = tmp.Y;

                    tmp = SphericalToCartesian(i + radianMeshSize, j, Vector3.One);
                    v[1].Position = tmp * radius;
                    v[1].Normal = Vector3.Zero;
                    v[1].TextureCoordinate.X = tmp.X;
                    v[1].TextureCoordinate.Y = tmp.Y;

                    tmp = SphericalToCartesian(i + radianMeshSize, j + radianMeshSize, Vector3.One);
                    v[2].Position = tmp * radius;
                    v[2].Normal = Vector3.Zero;
                    v[2].TextureCoordinate.X = tmp.X;
                    v[2].TextureCoordinate.Y = tmp.Y;

                    tmp = SphericalToCartesian(i, j + radianMeshSize, Vector3.One);
                    v[3].Position = tmp * radius;
                    v[3].Normal = Vector3.Zero;
                    v[3].TextureCoordinate.X = tmp.X;
                    v[3].TextureCoordinate.Y = tmp.Y;

                    verts.Add(v);

                    indices.Add(new int[] { 0, 1, 2, 1, 3, 2});
                    SetNormalOfTriangleAtIndices(0, 1, 2, verts.Count - 1);
                    SetNormalOfTriangleAtIndices(1, 3, 2, verts.Count - 1);
                    tm.CreateMesh(vl, tvi, 1, 1f); // Last two parameters are listed as not used on the octree

                    triangleMeshes.Add(tm);
                    Skin.AddPrimitive(tm, (int)MaterialTable.MaterialID.NormalRough);
                }
            }

            List<Vector3> vertexList = new List<Vector3>();
            List<TriangleVertexIndices> indexList = new List<TriangleVertexIndices>();


            Effect = new BasicEffect(g);

            //ExtractData(vertexList, indexList, model);

            //triangleMesh.CreateMesh(vertexList,indexList, 4, 1.0f);
            //Skin.AddPrimitive(triangleMesh, new MaterialProperties(0.8f, 0.7f, 0.6f));
            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.AddCollisionSkin(Skin);

            // Transform
            Skin.ApplyLocalTransform(new JigLibX.Math.Transform(center, Matrix.Identity));
            // we also need to move this dummy, so the object is *rendered* at the correct positiob
            Body.MoveTo(center, Matrix.Identity);
            CommonInit(center, new Vector3(1, 1, 1), null, false, -1);
        }

        private Vector3 SphericalToCartesian(double i, double j, Vector3 radius)
        {
            return new Vector3((float)(Math.Sin(i) * Math.Cos(j)), (float)(Math.Sin(i) * Math.Sin(j)), (float)Math.Cos(i)) * radius;
        }

        /*public static TriangleMesh CreateSphericalMeshSegment(double minTheta, double maxTheta, double minPhi, double maxPhi, int triangles, double maxDeviation)
        {
            TriangleMesh tm = new TriangleMesh();

            tm.CreateMesh(vertexList, indexList, triangles, 1.0f);


            return tm;
        }*/

        public void Draw(GraphicsDevice g, Matrix view, Matrix projection)
        {
            if (verts == null)
                return;

            try
            {
                this.Effect.View = view;
                Effect.Texture = texture;
                Effect.Projection = projection;
                Effect.EnableDefaultLighting();
                Effect.AmbientLightColor = Color.Black.ToVector3();
                Effect.DiffuseColor = Color.DarkGray.ToVector3();
                Effect.TextureEnabled = true;
                
                for(int i = 0; i < verts.Count; i++)
                {
                    VertexPositionNormalTexture[] v = verts[i];
                    int[] ind = indices[i];
                    VertexPositionNormalTexture[] worldly = TransformVPNTs(v); // because the verts are relative to the body, but here we need the real deal for drawing in the world
                    foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        g.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, worldly, 0, v.Length, ind, 0, ind.Length / 3);
                    }
                }
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        private VertexPositionNormalTexture[] TransformVPNTs(VertexPositionNormalTexture[] vpnt)
        {
            VertexPositionNormalTexture[] tverts = new VertexPositionNormalTexture[vpnt.Length];
            for (int i = 0; i < vpnt.Length; i++)
            {
                tverts[i] = vpnt[i];
                tverts[i].Position = Vector3.Transform(vpnt[i].Position,
                                            Body.Orientation * Matrix.CreateTranslation(Body.Position));
            }
            return tverts;
        }


        private void SetNormalOfTriangleAtIndices(int a, int b, int c, int v)
        {
            Vector3 vA = verts[v][a].Position;
            Vector3 vB = verts[v][b].Position;
            Vector3 vC = verts[v][c].Position;
            Triangle t = new Triangle(vA, vC, vB);
            Vector3 n = t.Normal;
            verts[v][a].Normal += n;
            verts[v][b].Normal += n;
            verts[v][c].Normal += n;
            verts[v][a].Normal.Normalize();
            verts[v][b].Normal.Normalize();
            verts[v][c].Normal.Normalize();
        }
    }

}
