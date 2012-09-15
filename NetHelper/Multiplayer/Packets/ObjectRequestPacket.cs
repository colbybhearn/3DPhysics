using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectRequestPacket : Packet
    {
        public string AssetName;
        
        public ObjectRequestPacket(string asset) 
            : base(Types.csObjectRequest)
        {
            AssetName = asset;
        }
    }
}
