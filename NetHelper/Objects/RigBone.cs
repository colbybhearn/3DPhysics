using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Helper.Objects
{
    public class RigBone
    {
        public int Id;
        Model Mod;
        
        public Quaternion Orient
        {
            get
            {
                return Quaternion.CreateFromYawPitchRoll(RelativeOrientYPR.X, RelativeOrientYPR.Y, RelativeOrientYPR.Z);
            }
        }
        public Quaternion OrientCorrection
        {
            get
            {
                return Quaternion.CreateFromYawPitchRoll(ModelOrientCorrectionYPR.X, ModelOrientCorrectionYPR.Y, ModelOrientCorrectionYPR.Z);
            }
        }
        Vector3 ModelScaleCorrection = new Vector3(1, 1, 1);
        Vector3 ModelOriginCorrection = Vector3.Zero;
        Vector3 ModelOrientCorrectionYPR = Vector3.Zero;

        // Relative to the parent
        Vector3 RelativeOrigin = Vector3.Zero;
        Vector3 RelativeOrientYPR = Vector3.Zero;
        
        public RigBone ParentBone;
        public SortedList<int, RigBone> ChildBones = new SortedList<int, RigBone>();

        public RigBone(int id, Model m, Vector3 scale, Vector3 relOrientYPR, Vector3 relOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            Id = id;
            Mod = m;
            ModelScaleCorrection = scale;
            RelativeOrientYPR = relOrientYPR;
            RelativeOrigin = relOrigin;
            ModelOrientCorrectionYPR = modelOrientCorrection;
            ModelOriginCorrection = modelOriginCorrection;
        }

        public void AdjustYawPitchRoll(float yaw, float pitch, float roll)
        {
            RelativeOrientYPR.X += yaw;
            RelativeOrientYPR.Y += pitch;
            RelativeOrientYPR.Z += roll;
        }

        private void AddBone(RigBone rb)
        {
            ChildBones.Add(rb.Id, rb);
        }
        public void AddBone(ref RigBone rb)
        {
            ChildBones.Add(rb.Id, rb);
        }

        public static RigBone GetBone(int id, Model m, Vector3 scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            RigBone rb = new RigBone(id, m, scale, orientYPR, RelativeOrigin,modelOrientCorrection, modelOriginCorrection);
            return rb;
        }
        public static RigBone GetBone(int id, Model m, float scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            return GetBone(id, m, new Vector3(scale, scale, scale), orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
        }
        public static RigBone GetBone(int id, Model m, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            return GetBone(id, m, 1.0f, orientYPR, RelativeOrigin,modelOrientCorrection,  modelOriginCorrection);
        }
        /// <summary>
        /// Used for iterating through the meshes
        /// </summary>
        public Matrix[] ModelTranforms
        {
            get
            {
                Matrix[] tranforms = new Matrix[Mod.Bones.Count];
                Mod.CopyAbsoluteBoneTransformsTo(tranforms);
                return tranforms;
            }
        }
        /// <summary>
        /// Retrieves all transforms related to this bone's model
        /// Some Transformations are not used (because they are insignificant) for this bone's parents (scale / orientation correction)
        /// </summary>
        /// <param name="isLeaf"></param>
        /// <returns></returns>
        public Matrix GetTransformation(bool isLeaf)
        {
            Matrix transform = Matrix.Identity;

            if (isLeaf) // if this is the leaf model,
                transform*= Matrix.CreateScale(ModelScaleCorrection); // include the scale correction

            transform *= Matrix.CreateTranslation(ModelOriginCorrection); // shift the Model away from its native origin here

            Quaternion qRot = Orient; // represents orientation of the model based on animation / user input
            if (isLeaf) // if this is the leaf model,
                qRot *= OrientCorrection; // represent shifting the model away from native orientation 

            // the rot variable represents all manipulations in a single vector for Yaw, Pitch, Roll
            // Merging Yaw/Pitch/Roll vectors, then getting a Quaternion, and then a Matrix works
            // Multiplying two Quaternions and then getting a Matrix would work
            // Multiplying to Matrices does not work because the first changes the affect of the second.
            Matrix mRot = Matrix.CreateFromQuaternion(qRot); //

            transform *= mRot; // Rotate the model
            //X:-0.35 Y:0.1 Z:-0.26
            transform*=Matrix.CreateTranslation(RelativeOrigin); // Move the model
                
            if(ParentBone!=null)
                transform *= ParentBone.GetTransformation(false); // Apply parent's transforms

            return transform;
        }

        /// <summary>
        /// Returns just the transforms related to model the orientation caused by a series of bones
        /// </summary>
        /// <returns></returns>
        public Matrix GetOrientation()
        {
            Matrix orient = Matrix.CreateFromQuaternion(Orient); // include animation or user-input-based orientation transformation only
            if(ParentBone != null) // if there is a parent,
                orient *= ParentBone.GetOrientation(); // include their orientation transform
            return orient;
        }

        public void SetYawPitchRoll(float yaw, float pitch, float roll)
        {
            RelativeOrientYPR.X = yaw;
            RelativeOrientYPR.Y = pitch;
            RelativeOrientYPR.Z = roll;
        }
    }
}
