using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectUpdatePacket : Packet
    {
        int objectId;
        string assetName;
        Vector3 position;
        Matrix orientation;
        Vector3 velocity;
        
        public ObjectUpdatePacket(int id, string asset, Vector3 pos, Matrix orient, Vector3 vel) 
            : base(Types.scObjectUpdate)
        {
            objectId = id;
            assetName = asset;
            position = pos;
            orientation = orient;
            velocity = vel;
        }
    }
}
