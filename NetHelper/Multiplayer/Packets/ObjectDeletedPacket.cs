using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectDeletedPacket : Packet
    {
        public int objectId;

        public ObjectDeletedPacket(int id)
            : base(Types.scObjectDeleted)
        {
            objectId = id;
        }

    }
}
