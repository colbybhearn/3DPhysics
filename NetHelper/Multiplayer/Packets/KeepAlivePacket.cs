using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class KeepAlivePacket : Packet
    {
        public DateTime time;

        public KeepAlivePacket() 
            : base(Types.KeepAlive)
        {
            time = DateTime.Now;
        }
    }
}
