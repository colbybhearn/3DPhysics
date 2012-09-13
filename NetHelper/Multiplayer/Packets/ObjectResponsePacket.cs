using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectResponsePacket : Packet
    {
        public int ID;

        public ObjectResponsePacket(int id)
            : base(Types.scObjectResponse)
        {
            ID = id;
        }
    }
}
