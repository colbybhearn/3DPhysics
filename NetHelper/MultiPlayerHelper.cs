using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace MultiplayerHelper
{

    public class Game
    {
        string sName;
        public string m_Name
        {
            get { return sName; }
            set { sName = value; }
        }

        string sKey;
        public string m_Key
        {
            get { return sKey; }
            set { sKey = value; }
        }

        public Game(string name)
        {
            sName = name;
            sKey = System.Guid.NewGuid().ToString();
        }

        public Game(string key, string name)
        {
            sKey = key;
            sName = name;            
        }
    }


    /// <summary>
    /// Packet Structer
    /// <TYPE|INFO|DELIVERY|SOURCE|TARGET|DATA>
    /// </summary>
    
    public class Packet
    {
        private string sParse = "%";
        
        public enum pType
        {
            TO_CLIENT,
            TO_CLIENT_GUI,
            TO_SERVER,
            TO_SERVER_GUI,
        }

        public enum pInfo
        {
            ALIAS_CHANGE, 
            CONNECTION_INFO,
            CHAT_MESSAGE,
            CLIENT_DISCONNECT,
            CLIENT_LIST_ADD,
            CLIENT_LIST_REFRESH,            
            GAME_DATA,
            GENERIC_DATA,
            NEW_CLIENT,
            NEW_GAME,
            SETUP,
            STATUS_MESSAGE
        }

        public enum pDelivery
        {
            BROADCAST_ALL,
            BROADCAST_OTHERS,
            TARGETED
        }

        public pType type;    
        public pDelivery delivery;
        public pInfo info;
        public string sClientTarget;
        public string sClientSource;
        private SortedList<string, string> m_Fields = new SortedList<string, string>();
        public SortedList<string, string> Fields
        {
            get { return m_Fields; }
        }

        public Packet()
        {
        }

        public Packet(Packet.pType t, Packet.pInfo i, Packet.pDelivery d, string source, string target)
        {
            type = t;
            info = i;
            delivery = d;
            sClientSource = source;
            sClientTarget = target;
            Fields.Clear();
        }

        public override string  ToString()
        {
            //÷key‹string
            StringBuilder sb = new StringBuilder();
            sb.Append(((int)type).ToString() + sParse);
            sb.Append(((int)info).ToString() + sParse);
            sb.Append(((int)delivery).ToString() + sParse);
            sb.Append(this.sClientSource + sParse);
            sb.Append(this.sClientTarget + sParse);


            for (int i = 0; i < m_Fields.Count; i++)
                sb.Append(m_Fields.Keys[i] + sParse + m_Fields.Values[i] + sParse);
                
            return sb.ToString();
        }

        public Packet FromString(string s)
        {
            //÷key‹string
            string[] word = s.Split(sParse.ToCharArray());
            this.type = (pType)Convert.ToInt32(word[0]);
            this.info = (pInfo)Convert.ToInt32(word[1]);
            this.delivery = (pDelivery)Convert.ToInt32(word[2]);
            this.sClientSource = word[3];
            this.sClientTarget = word[4];


            m_Fields.Clear();

            for(int i=5;i+1<word.Length;i+=2)
                m_Fields.Add(word[i], word[i+1]);

            return this;
        }

        public bool AddFieldValue(string name, string value)
        {
            if(m_Fields.ContainsKey(name))
                return false;

            m_Fields.Add(name, value);
            return true;
        }

        public string GetFieldValue(string name)
        {
            return m_Fields[name];
        }

        public void setTarget(string s)
        {
            this.sClientTarget = s;
        }

        public string getTarget()
        {
            return sClientTarget;
        }

        public Packet Clone()
        {
            Packet p = new Packet();
            p.FromString(this.ToString());
            return p;
        }
    }
    
    /*

    public class ServerPacket
    {
        public string sMsg;

        public ServerPacket(string s)
        {
            sMsg = s;
        }
    }

    public class ClientPacket
    {        
        public string sClientKey;
        public string sMsg;

        public ClientPacket(string k, string s)
        {
            this.sClientKey = k;
            this.sMsg = s;
        }

    }
     * */
}
