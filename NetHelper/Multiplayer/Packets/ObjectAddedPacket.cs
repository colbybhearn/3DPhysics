using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectAddedPacket : Packet
    {
        public int Owner;
        public int ID;
        public int AssetName;

        public ObjectAddedPacket(int owner, int id, int asset)
            : base(Types.scObjectResponse)
        {
            Owner = owner;
            ID = id;
            AssetName = asset;
        }
    }
}
