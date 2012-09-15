using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace Helper.Multiplayer
{
    public class TcpEventServer
    {
        #region Properties
        string sIPAddress;
        int iPort;
        bool bStopThread;
        IPAddress ipAd;
        TcpListener myListener;
        Thread ServerListener;

        #endregion


        #region Initialization

        public TcpEventServer( string IP, int port)
        {
            ipAd = IPAddress.Parse(IP);
            this.iPort = port;
            ServerListener = new Thread(new ThreadStart(ServerWorker));
            
        }
        public void StartListening()
        {
            ServerListener.Start();
            Debug.WriteLine("Lobby Listener: ServerListener.Start() finished: " + iPort.ToString());
        }
        public void StopListening()
        {
            this.bStopThread = true;
        }

        #endregion

        private void ServerWorker()
        {
            try
            {
                // use local m/c IP address, and 
                Debug.WriteLine("Lobby Listener: creating Listener");

                // Initializes the Listener 
                // myList = new TcpListener(ipAd, iPort);
                myListener = new TcpListener(IPAddress.Any, iPort);
                Debug.WriteLine("Lobby Listener: starting Listener");
                // Start Listeneting at the specified port         
                myListener.Start();
                Debug.WriteLine("Lobby Listener: Waiting to Accept on: " + iPort.ToString());

                // poll for pending connections
                while (!bStopThread)
                {
                    if (myListener.Pending())
                    {
                        Socket socket = myListener.AcceptSocket();
                        CallClientAccepted(socket);
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                string s = "LobbyListener Error: " + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(s);
            }
               myListener.Stop();
        }
        
        public delegate void ClientAcceptedEventHandler(Socket s);
        public event ClientAcceptedEventHandler ClientAccepted;
        public void CallClientAccepted(Socket s)
        {
            if (ClientAccepted != null)
                ClientAccepted(s);
        }

        

    }
}
