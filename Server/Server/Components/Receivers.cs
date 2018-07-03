using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Server
{
    class RequestReceiver
    {
        private SocketHandler SocketHandler;

        public RequestReceiver(SocketHandler _socketHandler)
        {
            SocketHandler = _socketHandler;
        }

        public void StartReceiving()
        {
            try
            {
                foreach (Request request in SocketHandler.Receive())
                {
                    Console.WriteLine("|================================================|");
                    Console.WriteLine("| REQUEST FROM THREAD - {0} ", Thread.CurrentThread.ManagedThreadId.ToString());
                    Console.WriteLine("|------------------------------------------------|");
                    Console.WriteLine("|                REQUEST DATA                    |");
                    Console.WriteLine("|------------------------------------------------|");
                    Console.WriteLine("| REQUEST TYPE - {0} ", request.Type);
                    if (request.Data != null)
                    {
                        foreach (KeyValuePair<string, string> pair in request.Data)
                        {
                            Console.WriteLine("| REQUEST DATA - {0}: {1}", pair.Key, pair.Value);
                        }
                    }


                    IRequestHandler requestHandler = RequestHandlerMapper.DefineHandler(request.Type);
                    Response response = requestHandler.Handle(request);
                    SocketHandler.Send(response);


                    Console.WriteLine("|================================================|");
                    Console.WriteLine("|                RESPONSE DATA                   |");
                    Console.WriteLine("|------------------------------------------------|");
                    Console.WriteLine("| RESPONSE STATUS - {0} ", response.Status);
                    if (response.Data != null)
                    {
                        foreach (KeyValuePair<string, string> pair in response.Data)
                        {
                            Console.WriteLine("| RESPONSE DATA - {0}: {1}", pair.Key, pair.Value);
                        }
                    }
                    Console.WriteLine("|================================================|\n\n\n");  
                    if (response.OnRequest == RequestType.Disconnect)
                    {
                        Disconnect();
                        return;
                    }
                }
            }
            catch (IOException)
            {
                Disconnect();
                return;
            }
        }

        private void Disconnect()
        {
            ClientStatus.RemoveInstance(Thread.CurrentThread.ManagedThreadId.ToString());
            Server.Clients.Remove(SocketHandler);
            SocketHandler.Close();
        }
    }
}
