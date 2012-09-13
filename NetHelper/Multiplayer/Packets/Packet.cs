using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace Helper.Multiplayer.Packets
{
    /* Adding a new packet class
     * 
     * Steps:
     *  - Add the specific packet class
     *  - Add the packet type to the Types enumeration
     *  - if convenient, copy this list to that new class as a header.
     * 
     *  - Make the class serializable with [Serializable]
     *  - Make the class public
     *  - Call the base constuctor with   :base(Types.____)
     *  - Add properties
     *  - Add constructor with initialization arguments (no empty constructors, please)
     *  - Add handling case in GameClient and GameServer's ProcessInputPacket
     *  - All done! Celebrate with some homework! =D
     * 
     * Notes:
     *  - sc means server to client onlu
     *  - cs means client to server only
     *  - binary serialization is used / automatically handled
     */

    [Serializable]
    public class Packet
    {
        public enum Types
        {
            KeepAlive, // not used yet (could be used to determine ping)
            Chat,
            scClientInfoRequest,
            csClientInfoResponse,
            csObjectUpdate,  // may not be needed, use scObjectUpdate
            scObjectUpdate
        }

        public Types Type;

        public Packet(Types type)
        {
            Type = type;
        }

        public virtual byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(ms, this);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            int datalen = (int)ms.Length;
            byte[] lenbytes = BitConverter.GetBytes(datalen);
            if (lenbytes.Length != 4)
            {

            }
            List<byte> dataList = new List<byte>();
            dataList.AddRange(lenbytes);
            dataList.AddRange(ms.ToArray());
            //ms.Write(lenbytes, 0, lenbytes.Length);
            
            //   | len of serialised data | Serialized Data |

            return dataList.ToArray();
        }

        public static Packet Deserialize(byte[] data)
        {            
            MemoryStream ms = new MemoryStream(data); 
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                object o = formatter.Deserialize(ms);
                if (o is Packets.Packet)
                    return o as Packets.Packet;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return null;
        }

    }
}
