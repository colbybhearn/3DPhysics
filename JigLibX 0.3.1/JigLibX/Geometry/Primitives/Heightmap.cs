#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using JigLibX.Math;
using JigLibX.Utils;
using JigLibX.Geometry;
#endregion

namespace JigLibX.Geometry
{
    /// <summary>
    /// Defines a heightmap that has up in the "Y" direction 
    /// </summary>
    /// <remarks>
    /// Indicies go from "bottom right" - i.e. (0, 0) -> (xmin, ymin)
    /// heights/normals are obtained by interpolation over triangles,
    /// with each quad being divided up in the same way - the diagonal
    /// going from (i, j) to (i+1, j+1)    
    /// </remarks>
    public class Heightmap : Primitive
    {
        private Array2D mHeights;

        private float x0, z0;
        private float dx, dz;
        private float xMin, zMin;
        private float xMax, zMax;
        private float yMax, yMin;
        /// <summary>
        /// Get new Vector3
        /// </summary>
        public Vector3 Min { get { return new Vector3(xMin, yMin, zMin); } }
        /// <summary>
        /// Get new Vector3
        /// </summary>
        public Vector3 Max { get { return new Vector3(xMax, yMax, zMax); } }

        /// <summary>
        /// Pass in an array of heights, and the axis that represents up
        /// Also the centre of the heightmap (assuming z is up), and the grid size
        /// </summary>
        /// <param name="heights"></param>
        /// <param name="x0"></param>
        /// <param name="z0"></param>
        /// <param name="dx"></param>
        /// <param name="dz"></param>
        public Heightmap(Array2D heights, float x0, float z0, float dx, float dz)
            : base((int)PrimitiveType.Heightmap)
        {
            mHeights = heights;
            this.x0 = x0;
            this.z0 = z0;
            this.dx = dx;
            this.dz = dz;

            this.xMin = x0 - (mHeights.Nx - 1) * 0.5f * dx;
            this.zMin = z0 - (mHeights.Nz - 1) * 0.5f * dz;
            this.xMax = x0 + (mHeights.Nx - 1) * 0.5f * dx;
            this.zMax = z0 + (mHeights.Nz - 1) * 0.5f * dz;

            // Save this, so we don't need to recalc every time
            this.yMin = mHeights.Min;
            this.yMax = mHeights.Max;
        }

        /// <summary>
        /// Call this after changing heights. So the bounding
        /// box for collision detection gets recalculated.
        /// </summary>
        public void RecalculateBoundingBox()
        {
            this.xMin = x0 - (mHeights.Nx - 1) * 0.5f * dx;
            this.zMin = z0 - (mHeights.Nz - 1) * 0.5f * dz;
            this.xMax = x0 + (mHeights.Nx - 1) * 0.5f * dx;
            this.zMax = z0 + (mHeights.Nz - 1) * 0.5f * dz;
            this.yMin = mHeights.Min;
            this.yMax = mHeights.Max;
        }

        /// <summary>
        /// GetBoundingBox
        /// </summary>
        /// <param name="box"></param>
        public override void GetBoundingBox(out AABox box)
        {
            box = new AABox(this.Min, this.Max);
        }

        /// <summary>
        /// Get the height at a particular index, indices are clamped
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns>float</returns>
        public float GetHeight(int i, int j)
        {
            i = (int)MathHelper.Clamp(i, 0, mHeights.Nx - 1);
            j = (int)MathHelper.Clamp(j, 0, mHeights.Nz - 1);

            return mHeights[i, j];
        }

        /// <summary>
        /// Get the normal
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetNormal(int i, int j)
        {
            int i0 = i - 1;
            int i1 = i + 1;
            int j0 = j - 1;
            int j1 = j + 1;
            i0 = (int)MathHelper.Clamp(i0, 0, (int)mHeights.Nx - 1);
            j0 = (int)MathHelper.Clamp(j0, 0, (int)mHeights.Nz - 1);
            i1 = (int)MathHelper.Clamp(i1, 0, (int)mHeights.Nx - 1);
            j1 = (int)MathHelper.Clamp(j1, 0, (int)mHeights.Nz - 1);

            float dx = (i1 - i0) * this.dx;
            float dz = (j1 - j0) * this.dz;

            if (i0 == i1) dx = 1.0f;
            if (j0 == j1) dz = 1.0f;

            if (i0 == i1 && j0 == j1) return Vector3.Up;

            float hFwd = mHeights[i1, j];
            float hBack = mHeights[i0, j];
            float hLeft = mHeights[i, j1];
            float hRight = mHeights[i, j0];

            Vector3 v1 = new Vector3(dx, hFwd - hBack,0.0f);
            Vector3 v2 = new Vector3(0.0f, hLeft - hRight,dz);

            #region REFERENCE: Vector3 normal = Vector3.Cross(v1,v2);
            Vector3 normal;
            Vector3.Cross(ref v1, ref v2, out normal);
            #endregion
            normal.Normalize();

            return normal;
        }

        /// <summary>
        /// Get height and normal (quicker than calling both)
        /// </summary>
        /// <param name="h"></param>
        /// <param name="normal"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public void GetHeightAndNormal(out float h, out Vector3 normal, int i, int j)
        {
            h = GetHeight(i, j);
            normal = GetNormal(i, j);
        }

        /// <summary>
        /// GetSurfacePos
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public void GetSurfacePos(out Vector3 pos, int i, int j)
        {
            float h = GetHeight(i, j);
            pos = new Vector3(xMin + i * dx, h, zMin + j * dz);
        }

        /// <summary>
        /// GetSurfacePosAndNormal
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public void GetSurfacePosAndNormal(out Vector3 pos, out Vector3 normal, int i, int j)
        {
            float h = GetHeight(i, j);
            pos = new Vector3(xMin + i * dx, h, zMin + j * dz);
            normal = GetNormal(i, j);
        }

        /// <summary>
        /// GetHeight
        /// </summary>
        /// <param name="point"></param>
        /// <returns>float</returns>
        public float GetHeight(Vector3 point)
        {
            // todo - optimise
            float h;
            Vector3 normal;
            GetHeightAndNormal(out h, out normal,point);
            return h;
        }

        /// <summary>
        /// GetNormal
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Vector3</returns>
        public Vector3 GetNormal(Vector3 point)
        {
            // todo - optimise
            float h;
            Vector3 normal;
            GetHeightAndNormal(out h, out normal,point);
            return normal;
        }

        /// <summary>
        /// GetHeightAndNormal
        /// </summary>
        /// <param name="h"></param>
        /// <param name="normal"></param>
        /// <param name="point"></param>
        public void GetHeightAndNormal(out float h, out Vector3 normal,Vector3 point)
        {
            float x = point.X;
            float z = point.Z;

            x = MathHelper.Clamp(x, xMin, xMax);
            z = MathHelper.Clamp(z, zMin, zMax);

            int i0 = (int)((x - xMin) / dx);
            int j0 = (int)((point.Z - zMin) / dz);

            i0 = (int)MathHelper.Clamp((int)i0, 0, mHeights.Nx - 1);
            j0 = (int)MathHelper.Clamp((int)j0, 0, mHeights.Nz - 1);

            int i1 = i0 + 1;
            int j1 = j0 + 1;

            if (i1 >= (int)mHeights.Nx) i1 = mHeights.Nx - 1;
            if (j1 >= (int)mHeights.Nz) j1 = mHeights.Nz - 1;

            float iFrac = (x - (i0 * dx + xMin)) / dx;
            float jFrac = (z - (j0 * dz + zMin)) / dz;

            iFrac = MathHelper.Clamp(iFrac, 0.0f, 1.0f);
            jFrac = MathHelper.Clamp(jFrac, 0.0f, 1.0f);

            float h00 = mHeights[i0, j0];
            float h01 = mHeights[i0, j1];
            float h10 = mHeights[i1, j0];
            float h11 = mHeights[i1, j1];

            // All the triangles are orientated the same way.
            // work out the normal, then z is in the plane of this normal
            if ((i0 == i1) && (j0 == j1))
            {
                normal = Vector3.Up;
            }
            else if (i0 == i1)
            {
                Vector3 right = Vector3.Right;
                normal = Vector3.Cross(new Vector3(0.0f, h01 - h00, dz),right);
                normal.Normalize();
            }

            if (j0 == j1)
            {
                Vector3 backw = Vector3.Backward;
                normal = Vector3.Cross(backw, new Vector3(dx, h10 - h00, 0.0f));
                normal.Normalize();
            }
            else if (iFrac > jFrac)
            {
                normal = Vector3.Cross(new Vector3(dx, h11 - h00, dz), new Vector3(dx, h10 - h00, 0.0f));
                normal.Normalize();
            }
            else
            {
                normal = Vector3.Cross(new Vector3(0.0f, h01 - h00, dz), new Vector3(dx, h11 - h00, dz));
                normal.Normalize();
            }

             // get the plane equation
             // h00 is in all the triangles
             JiggleMath.NormalizeSafe(ref normal);
             Vector3 pos = new Vector3((i0 * dx + xMin), h00, (j0 * dz + zMin));
             float d; Vector3.Dot(ref normal, ref pos, out d); d = -d;
             h = Distance.PointPlaneDistance(ref point,ref normal, d);
        }

        /// <summary>
        /// GetSurfacePos
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="point"></param>
        public void GetSurfacePos(out Vector3 pos, Vector3 point)
        {
            // todo - optimise
            float h = GetHeight(point);
            pos = new Vector3(point.X,h, point.Z);
        }

        /// <summary>
        /// GetSurfacePosAndNormal
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <param name="point"></param>
        public void GetSurfacePosAndNormal(out Vector3 pos, out Vector3 normal, Vector3 point)
        {
            float h;
            GetHeightAndNormal(out h, out normal,point);
            pos = new Vector3(point.X,h, point.Z);
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>new Heightmap</returns>
        public override Primitive Clone()
        {
            return new Heightmap(new Array2D(mHeights), x0, z0, dx, dz);
        }

        /// <summary>
        /// Gets Transform.Identity
        /// </summary>
        public override Transform Transform
        {
            get {return Transform.Identity;}
            set {}
        }

        /// <summary>
        /// SegmentIntersect
        /// </summary>
        /// <param name="frac"></param>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <param name="seg"></param>
        /// <returns>bool</returns>
        public override bool SegmentIntersect(out float frac, out Vector3 pos, out Vector3 normal,Segment seg)
        {
            /* Bug with SegmentIntersect, if the segment >= 1 square, then the code fails.
             * We need to start at seg.Origin, and move to seg.Origin + (dx,dz) until one is below and one is above 0)
             */
            frac = 0;
            pos = Vector3.Zero;
            normal = Vector3.Up;
            Vector3 normalStart = Vector3.Zero;
            float heightStart = 0.0f;
            Vector3 normalEnd;
            float heightEnd;

            Vector3 segPos = Vector3.Zero;
            Vector3 segPosEnd = seg.Origin;
            Vector3 segEnd = seg.GetEnd();
            Vector3 segDirection = seg.Delta;
            segDirection.Y = 0;
            JiggleMath.NormalizeSafe(ref segDirection);
            segDirection.Y = seg.Delta.Y * (segDirection.X / seg.Delta.X); // Scale the Y to be make segDirection a scaler of segDelta, but normalized in the XY plane

            float increment = JiggleMath.Min(dx, dz, float.MaxValue); // Move by the smaller of the two dx/dz values so we dont "jump" over a point ... this could be done better

            GetHeightAndNormal(out heightEnd, out normalEnd, seg.Origin);
            bool done = false;
            while (!done)
            {
                segPos = segPosEnd;
                normalStart = normalEnd;
                heightStart = heightEnd;

                segPosEnd += segDirection * increment;

                if ((((segPos.X - segEnd.X) > 0.0f) != ((segPosEnd.X - segEnd.X) > 0.0f)) // We have moved onto the other side of the seg end in X
                    || (((segPos.Z - segEnd.Z) > 0.0f) != ((segPosEnd.Z - segEnd.Z) > 0.0f))) // We have moved onto the other side of the seg end in Z
                {
                    segPosEnd = segEnd;
                }


                GetHeightAndNormal(out heightEnd, out normalEnd, segPosEnd);

                if ((heightStart >= 0.0f && heightEnd <= 0.0f) // Passes down through
                    || (heightStart <= 0.0f && heightEnd >= 0.0f)) // Passes up through
                {
                    done = true;
                    //We intersect here
                }

                if (segEnd.X == segPosEnd.X && segEnd.Z == segPosEnd.Z && !done) // Checking for X,Z handles cases of segments only in Y direction
                {
                    // No intersection found and we are at the end of the ray
                    return false;
                }
                
            }
            
            /*Vector3 normalStart;
            float heightStart;

            GetHeightAndNormal(out heightStart, out normalStart,seg.Origin);

            if (heightStart < 0.0f)
                return false;

            Vector3 normalEnd;
            float heightEnd;
            Vector3 end = seg.GetEnd();
            GetHeightAndNormal(out heightEnd, out normalEnd,end);

            if (heightEnd > 0.0f)
                return false;*/
            
            //Rest of calculations are % based, do not need negatives
            heightStart = System.Math.Abs(heightStart);
            heightEnd = System.Math.Abs(heightEnd);
            // normal is the weighted mean of these...
            float weightStart = 1.0f / (JiggleMath.Epsilon + heightStart);
            float weightEnd = 1.0f / (JiggleMath.Epsilon + heightEnd);

            normal = (normalStart * weightStart + normalEnd * weightEnd) /
              (weightStart + weightEnd);

            frac = heightStart / (heightStart + heightEnd + JiggleMath.Epsilon);

            //pos = seg.GetPoint(frac);
            pos = segPos + (segPosEnd - segPos) * frac;
            return true;
        }

        /// <summary>
        /// GetVolume
        /// </summary>
        /// <returns>0.0f</returns>
        public override float GetVolume()
        {
            return 0.0f;
        }

        /// <summary>
        /// GetSurfaceArea
        /// </summary>
        /// <returns>0.0f</returns>
        public override float GetSurfaceArea()
        {
            return 0.0f;
        }

        /// <summary>
        /// GetMassProperties
        /// </summary>
        /// <param name="primitiveProperties"></param>
        /// <param name="mass"></param>
        /// <param name="centerOfMass"></param>
        /// <param name="inertiaTensor"></param>
        public override void GetMassProperties(PrimitiveProperties primitiveProperties, out float mass, out Vector3 centerOfMass, out Matrix inertiaTensor)
        {
            mass = 0.0f;
            centerOfMass = Vector3.Zero;
            inertiaTensor = Matrix.Identity;
        }

        /// <summary>
        /// Get mHeights
        /// </summary>
        public Array2D Heights
        {
            get
            {
                return mHeights;
            }
        }

    }
}
