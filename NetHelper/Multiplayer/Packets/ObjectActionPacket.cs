using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectActionPacket : Packet
    {
        public int objectId;
        public object[] actionParameters;

        public ObjectActionPacket(int id, object[] parameters) 
            : base(Types.scObjectAction)
        {
            objectId = id;
            actionParameters = parameters;
        }
    }
}
