using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectAddedPacket : Packet
    {
        public int Owner;
        public int ID;
        public string AssetName;

        public ObjectAddedPacket(int owner, int id, string asset)
            : base(Types.scObjectResponse)
        {
            Owner = owner;
            ID = id;
            AssetName = asset;
        }
    }
}
