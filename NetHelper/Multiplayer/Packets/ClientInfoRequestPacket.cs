using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ClientInfoRequestPacket : Packet
    {
        public ClientInfoRequestPacket()
            : base(Types.scClientInfoRequest)
        {
        }
    }
}
