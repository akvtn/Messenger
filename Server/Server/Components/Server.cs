using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace Server
{
    class Server
    {
        public static Dictionary<SocketHandler, ClientStatus> Clients = new Dictionary<SocketHandler, ClientStatus> { };

        private String host;
        private int port;
        private Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private int oneTimeLinstened = 100;

        public Server(String _host, int _port)
        {
            port = _port;
            host = _host;
            listenSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
        }

        public void Run()
        {
            listenSocket.Listen(oneTimeLinstened);
            AcceptIncomingConnections();
            listenSocket.Close();
        }

        private void AcceptIncomingConnections()
        {
            while (true)
            {
                SocketHandler SocketHandler = new SocketHandler(listenSocket.Accept());
                RequestReceiver client = new RequestReceiver(SocketHandler);
                Thread currentUserThread = new Thread(client.StartReceiving);
                currentUserThread.Start();
                Clients.Add(SocketHandler, ClientStatus.GetInstance(currentUserThread.ManagedThreadId.ToString()));
            }
        }
    }
}
