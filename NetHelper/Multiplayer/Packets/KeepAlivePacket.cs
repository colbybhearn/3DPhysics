using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
