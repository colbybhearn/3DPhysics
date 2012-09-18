using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    // Sent to connected clients when a new client connects to the server
    [Serializable]
    public class ClientConnectedPacket : Packet
    {
        public int ID;
        public string Alias;
        public ClientConnectedPacket(int id, string alias)
            : base(Types.ClientConnectedPacket)
        {
            ID = id;
            Alias = alias;
        }
    }
}
