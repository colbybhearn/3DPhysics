using System;

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
