using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectResponsePacket : Packet
    {
        public int ID;
        public string AssetName;

        public ObjectResponsePacket(int id, string asset)
            : base(Types.scObjectResponse)
        {
            ID = id;
            AssetName = asset;
        }
    }
}
