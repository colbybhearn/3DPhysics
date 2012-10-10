using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectAttributePacket : Packet
    {
        public int objectId;
        public bool[] booleans;
        public int[] ints;
        public float[] floats;

        public ObjectAttributePacket(int id, bool[] bv, int[] iv, float[] fv)
             :base(Types.Attributes)
        {
            objectId = id;
            AddBools(bv);
            AddInts(iv);
            AddFloats(fv);
        }

        public void AddBools(bool[] bv)
        {
            booleans = bv;
        }

        public void AddInts(int[] iv)
        {
            ints = iv;
        }

        public void AddFloats(float[] fv)
        {
            floats = fv;
        }
    }
}
