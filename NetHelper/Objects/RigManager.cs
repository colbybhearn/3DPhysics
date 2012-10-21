using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Helper.Objects
{
    public class RigManager
    {
        SortedList<int, RigBone> Bones = new SortedList<int,RigBone>();
        public RigManager()
        {
        }

        public void AddBone(int parentId, RigBone child)
        {
            if(!Bones.ContainsKey(child.Id))
                Bones.Add(child.Id, child);

            if (Bones.ContainsKey(parentId))            
            {
                RigBone parent = Bones[parentId];
                parent.AddBone(ref child);
                child.ParentBone = parent;
            }
        }

        public Matrix GetOrientationMatrix(int id)
        {
            if (!Bones.ContainsKey(id))
                return Matrix.Identity;
            return Bones[id].GetOrientation();
        }

        public Matrix GetTranformationMatrix(int id)
        {
            if (!Bones.ContainsKey(id))
                return Matrix.Identity;
            return Bones[id].GetTransformation(true);
        }

        public Matrix[] GetModelTransforms(int id)
        {
            if (!Bones.ContainsKey(id))
                return null;
            return Bones[id].ModelTranforms;
        }

        public void AdjustYawPitchRoll(int id, float yaw, float pitch, float roll)
        {
            if (!Bones.ContainsKey(id))
                return;
            Bones[id].AdjustYawPitchRoll(yaw, pitch, roll);
        }
        public void SetYawPitchRoll(int id, float yaw, float pitch, float roll)
        {
            if (!Bones.ContainsKey(id))
                return;
            Bones[id].SetYawPitchRoll(yaw, pitch, roll);

        }
        public void AddBone(int id, Model m, Vector3 scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            RigBone rb = new RigBone(id, m, scale, orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
            Bones.Add(id, rb);
        }
        public void AddBone(int id, Model m, float scale, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection, Vector3 modelOriginCorrection)
        {
            AddBone(id, m, new Vector3(scale, scale, scale), orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
        }
        public void AddBone(int id, Model m, Vector3 orientYPR, Vector3 RelativeOrigin, Vector3 modelOrientCorrection,Vector3 modelOriginCorrection)
        {
            AddBone(id, m, 1.0f, orientYPR, RelativeOrigin, modelOrientCorrection, modelOriginCorrection);
        }
        
    }
}
