using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ClientDisconnectPacket : Packet       
    {
        public int id;
        public ClientDisconnectPacket(int i)
            :base(Types.ClientDisconnectPacket)
        {
            id = i;
        }
    }
}
