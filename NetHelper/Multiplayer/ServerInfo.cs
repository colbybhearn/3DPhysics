using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Helper.Multiplayer
{
    public class ServerInfo
    {
        public IPEndPoint endPoint;
        private System.IO.Stream stream;

        public ServerInfo(IPEndPoint ep)
        {
            endPoint = ep;
        }

        public void SetStream(Stream s)
        {
            this.stream = s;
        }
    }
}
