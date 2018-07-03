using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    class SocketHandler : IDisposable
    {
        private Socket socket;

        public SocketHandler(Socket _socket)
        {
            socket = _socket;
        }

        public IEnumerable<Request> Receive()
        {
            NetworkStream networkStream = new NetworkStream(socket);
            String message = String.Empty;
            while (true)
            {
                byte[] data = new byte[256];
                networkStream.Read(data, 0, data.Length);
                message += System.Text.Encoding.UTF8.GetString(data);

                if (!networkStream.DataAvailable)
                {
                    yield return JsonConvert.DeserializeObject<Request>(message);
                    message = String.Empty;
                }
            }
        }

        public void Send(Response response)
        {
            String jsonRespond = JsonConvert.SerializeObject(response);
            socket.Send(Encoding.UTF8.GetBytes(jsonRespond));
        }

        public void Close()
        {
            socket.Close();
        }

        public void Dispose()
        {
            ClientStatus.RemoveInstance(Thread.CurrentThread.ManagedThreadId.ToString());
            Server.Clients.Remove(this);
            socket.Close();
        }
    }
}
