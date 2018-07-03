using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace Client
{
    class SocketHandler
    {
        private Thread ResponseReadThread;
        private CancellationTokenSource CancellationResponseReadTokenSource = new CancellationTokenSource();
        private Socket socket;

        public SocketHandler(Socket _socket)
        {
            socket = _socket;
            (ResponseReadThread = new Thread(() => Receive())).Start();
        }

        public async void Receive()
        {
            NetworkStream networkStream = new NetworkStream(socket);
            String message = String.Empty;
            while (!CancellationResponseReadTokenSource.Token.IsCancellationRequested)
            {
                byte[] data = new byte[256];
                await networkStream.ReadAsync(data, 0, data.Length, CancellationResponseReadTokenSource.Token);
                message += Encoding.UTF8.GetString(data);

                if (!networkStream.DataAvailable)
                {
                    ResponsesList.GetInstance().Items.Add(JsonConvert.DeserializeObject<Response>(message));
                    message = String.Empty;
                }
            }
        }

        public void Send(Request response)
        {
            String jsonRespond = JsonConvert.SerializeObject(response);
            socket.Send(Encoding.UTF8.GetBytes(jsonRespond));
        }

        public Response Request(Request request)
        {
            Send(request);
            return ResponsesList.GetInstance().GetResponseTo(request.Type, CancellationResponseReadTokenSource.Token);
        }

        public void Stop()
        {
            CancellationResponseReadTokenSource.Cancel();
        }

    }
}
