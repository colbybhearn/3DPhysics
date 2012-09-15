using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ClientDisconnectPacket : Packet       
    {
        public string Alias;
        public ClientDisconnectPacket(string alias)
            :base(Types.ClientDisconnectPacket)
        {
            Alias = alias;
        }
    }
}
