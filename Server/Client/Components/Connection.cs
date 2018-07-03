using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Client
{
    static class Connection
    {
        public static string Host;
        public static int Port;
        private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public static void Register(string username, string password)
        {
            socket.Connect(new IPEndPoint(IPAddress.Parse(Host), Port));
            SocketHandler TempSocketHandler = new SocketHandler(socket);

            Dictionary<string, string> RequestData = new Dictionary<string, string>
            {
                ["username"] = username,
                ["password"] = password
            };
            Response response = TempSocketHandler.Request(new Request(type: RequestType.Register, data: RequestData));
            socket.Close();
            if (response.Status == ResponseStatus.Error) throw Error.Make(response.Data);    
        }

        public static Client Authorize(string username, string password)
        {
            socket.Connect(new IPEndPoint(IPAddress.Parse(Host), Port));
            SocketHandler TempSocketHandler = new SocketHandler(socket);

            Dictionary<string, string> RequestData = new Dictionary<string, string>
            {
                ["username"] = username,
                ["password"] = password
            };
            Response response = TempSocketHandler.Request(new Request(type: RequestType.Authorize, data: RequestData));
            if (response.Status == ResponseStatus.Error) throw Error.Make(response.Data);
            return new Client(socketHandler: TempSocketHandler);
        }
    }

    static class Error
    {
        static Dictionary<string, Func<string,Exception>> Mapper = new Dictionary<string, Func<string, Exception>>
        {
            ["InvalidChat"] = (message) => new InvalidChatException(message),
            ["DatabaseAccessError"] = (message) => new DatabaseAccessException(message),
            ["AuthorizationError"] = (message) => new AuthorizationException(message),
            ["RegistrationError"] = (message) => new RegistraionException(message)
        };

        static public Exception Make(Dictionary<string, string> Data)
        {
            return Mapper[Data["Error Type"]](Data["Error Message"]);
        }
    }

    class Client
    {
        private Thread MessageReadThread;
        private CancellationTokenSource CancellationMessageReadTokenSource;
        private SocketHandler SocketHandler { get; }

        public Client(SocketHandler socketHandler)
        {
            SocketHandler = socketHandler;
        }
        
        public void Disconnect()
        {
            if(SocketHandler.Request(new Request(type: RequestType.Disconnect)).Status == ResponseStatus.Success)
                SocketHandler.Stop();
        }

        public void SendMessage(string Message,Chat Chat)
        {
            Response response = SocketHandler.Request(new Request(type: RequestType.SendMessage, data: new Dictionary<string, string>
            {
                ["Chat"] = Chat.Id,
                ["Message"] = Message
            }));
            if (response.Status == ResponseStatus.Error)
            {
                throw Error.Make(response.Data);
            }
        }

        public List<Chat> Chats()
        {
            Response response = SocketHandler.Request(new Request(type: RequestType.GetChats));
            if (response.Status == ResponseStatus.Success)
            {
                return JsonConvert.DeserializeObject<List<Chat>>(response.Data["Chats"]);
            }
            else
            {
                throw Error.Make(response.Data);
            }
        }

        public void OpenReadThread(Chat Chat, Action<Message> action)
        {

            if (MessageReadThread != null && MessageReadThread.IsAlive) throw new MultipleReadThreadsExecption("Multiple ReadTheads are not avialible");
            Response response = SocketHandler.Request(new Request(type: RequestType.SetChat,
                                                  data: new Dictionary<string, string> { ["NewChatId"] = Chat.Id }));
            if (response.Status == ResponseStatus.Success)
            {
                CancellationMessageReadTokenSource = new CancellationTokenSource();
                (MessageReadThread = new Thread(() =>
                {
                    ReceiveChatMessages(OnMessageReceive: action, CancellationToken: CancellationMessageReadTokenSource.Token);
               })).Start();
            }
            else
            {
                throw Error.Make(response.Data);
            }
        }
        
        public void CloseReadThread()
        {
            Response response = SocketHandler.Request(new Request(type: RequestType.UnsetChat));
            if (response.Status == ResponseStatus.Success)
            {
                CancellationMessageReadTokenSource.Cancel();
            }

        }

        private void ReceiveChatMessages(Action<Message> OnMessageReceive, CancellationToken CancellationToken)
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                Response response = ResponsesList.GetInstance().GetResponseTo(RequestType.SpreadMessage,CancellationToken);           
                if (response != null) OnMessageReceive(new Message { Author=response.Data["Author"], Content=response.Data["Message"] });
            }
        }

        public LinkedList<Message> ChatMessages(Chat Chat)
        {
            Response response = SocketHandler.Request(new Request(type: RequestType.GetMessages, data: new Dictionary<string, string>
            {
                ["ChatId"] = Chat.Id
            }));
            if (response.Status == ResponseStatus.Success)
                return JsonConvert.DeserializeObject<LinkedList<Message>>(response.Data["Messages"]);
            else
                throw Error.Make(response.Data);
        }

        public LinkedList<Message> ChatMessages(int ChatId)
        {
            return ChatMessages(new Chat() { Id = ChatId.ToString() });
        }

        public void CreateChat(string Title)
        {
            Response response = SocketHandler.Request(new Request(type: RequestType.CreateChat, data: new Dictionary<string, string> { ["Title"] = Title }));
        }

        public void AddToChat(Chat chat, User user)
        {
            Response response = SocketHandler.Request(new Request(type: RequestType.AddToChat, data: new Dictionary<string, string>
            {
                ["UserId"] = user.Id,
                ["ChatId"] = chat.Id
            }));
            if (response.Status == ResponseStatus.Error)
            {
                throw Error.Make(response.Data);
            }
        }

        public void RemoveFromChat(Chat chat, User user)
        {
            Response response = SocketHandler.Request(new Request(type: RequestType.RemoveFromChat, data: new Dictionary<string, string>
            {
                ["UserId"] = user.Id,
                ["ChatId"] = chat.Id
            }));
            if (response.Status == ResponseStatus.Error)
            {
                throw Error.Make(response.Data);
            }
        }
    }
}
