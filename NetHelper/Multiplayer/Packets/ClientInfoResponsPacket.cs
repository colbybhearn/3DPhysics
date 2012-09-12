using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ClientInfoResponsePacket : Packet
    {
        public string Alias;
        public ClientInfoResponsePacket(string alias) 
            :base(Types.csClientInfoResponse)
        {
            Alias = alias;
        }
    }
}
