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
        Vector3 position;
        Matrix orientation;
        Vector3 velocity;
        
        public ObjectUpdatePacket(int id, Vector3 pos, Matrix orient, Vector3 vel) 
            : base(Types.scObjectUpdate)
        {
            objectId = id;
            position = pos;
            orientation = orient;
            velocity = vel;
        }
    }
}
