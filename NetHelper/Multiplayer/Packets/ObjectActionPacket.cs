using System;

namespace Helper.Multiplayer.Packets
{
    [Serializable]
    public class ObjectActionPacket : Packet
    {
        /* TODO If needed - by Jeffrey
         * Possible to get this packet to be a "non-generic" packet if we know the following
         *  What object type it belongs to (known)
         *  Thus which functions it calls (known)
         *  And finally what parameters those functions take (not known generically)
         * So if when defining the functions, instead of saying how many parameters it takes
         *      if it either passes in what parameters it takes, or if they are defined and
         *      tied to the Enum of the type of function, we can then have this packet be
         *      compact and not a generic serialized packet.
         *      
         * This is probably not needed until this packet type gets used heavily.
         * Defining the functions in such a way that the parameters are known elseway (in the
         *      network code) also allows us to not have to have a "generic" and "specific"
         *      type of each.
         */

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
