using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ClientInfoRequestPacket : Packet
    {
        public int ID;
        public ClientInfoRequestPacket(int clientID)
            : base(Types.scClientInfoRequest)
        {
            ID = clientID;
        }
    }
}
