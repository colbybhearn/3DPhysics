using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectRequestPacket : Packet
    {
        public int AssetName;
        
        public ObjectRequestPacket(int asset) 
            : base(Types.csObjectRequest)
        {
            AssetName = asset;
        }
    }
}
