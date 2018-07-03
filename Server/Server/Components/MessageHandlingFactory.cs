using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Server
{
    delegate IMessageFactory FactoryGetter();

    static class TypeFactoryMapper
    {
        static private Dictionary<string, FactoryGetter> Mapper = new Dictionary<string, FactoryGetter>
        {
            ["Text"] = GetTextMessageFactory,
            ["Image"] = GetImageMessageFactory,
            ["Document"] = GetDocumentMessageFactory
        };  

        static public IMessageFactory GetFactory(string _type)
        {
            return Mapper[_type].Invoke();
        }

        static IMessageFactory GetTextMessageFactory()
        {
            return new TextMessageFactory();
        }

        static IMessageFactory GetImageMessageFactory()
        {
            return new ImageMessageFactory();
        }

        static IMessageFactory GetDocumentMessageFactory()
        {
            return new DocumentMessageFactory();
        }
    }



    interface IMessageFactory
    {
        IMessageHandler DefineHandler(User _user, Chat _chat, LinkedList<Socket> _receivers, Dictionary<string, string> _data);
    }

    public class TextMessageFactory : IMessageFactory
    {
        IMessageHandler IMessageFactory.DefineHandler(User _user, Chat _chat, LinkedList<Socket> _receivers, Dictionary<string, string> _data)
        {
            return new TextMessageHandler(_user, _chat, _receivers, _data);
        } 
    }

    public class ImageMessageFactory : IMessageFactory
    {
        IMessageHandler IMessageFactory.DefineHandler(User _user, Chat _chat, LinkedList<Socket> _receivers, Dictionary<string, string> _data)
        {
            return new ImageMessageHandler(_user, _chat, _receivers, _data);
        }
    }

    public class DocumentMessageFactory : IMessageFactory
    {
        IMessageHandler IMessageFactory.DefineHandler(User _user, Chat _chat, LinkedList<Socket> _receivers, Dictionary<string, string> _data)
        {
            return new DocumentMessageHandler(_user, _chat, _receivers,_data);
        }
    }



    interface IMessageHandler
    {
        void Handle();
    } 

    public class MessageHandler
    {
        protected User User { get; }
        protected Chat Chat { get; }
        protected LinkedList<Socket> Receivers { get; }
        protected Dictionary<string,string> RequestData { get; }

        public MessageHandler(User _user, Chat _chat, LinkedList<Socket> _receivers, Dictionary<string, string> _data)
        {
            User = _user;
            Chat = _chat;
            Receivers = _receivers;
            RequestData = _data;
        }

        public void SpreadBack()
        {
            foreach(Socket client in Receivers)
            {
                Dictionary<string, string> ResponseData = new Dictionary<string, string>
                {
                    ["Author"] = User.Username,
                    ["Type"] = RequestData["Type"],
                    ["Message"] = RequestData["Message"]
                };
                //String jsonResponse = JsonConvert.SerializeObject(new Response(status: "New message",
                //                                                               data: ResponseData));
                //client.Send(Encoding.UTF8.GetBytes(jsonResponse));
            }
        }

        public void Save()
        {
            Chat.AddMessage(User, RequestData["Message"]);
        }
    }

    public class TextMessageHandler : MessageHandler, IMessageHandler
    {
        public TextMessageHandler(User user, Chat chat, LinkedList<Socket> receivers, Dictionary<string, string> data) : base(_user: user, _chat: chat, _receivers: receivers, _data: data) { }

        public void Handle()
        {
            Save();
            SpreadBack();
        }
    }

    public class ImageMessageHandler : MessageHandler, IMessageHandler
    {
        public ImageMessageHandler(User user, Chat chat,LinkedList<Socket> receivers, Dictionary<string, string> data) : base(_user: user, _chat: chat, _receivers: receivers, _data: data) { }

        public void Handle()
        {

        }
    }

    public class DocumentMessageHandler : MessageHandler, IMessageHandler
    {
        public DocumentMessageHandler(User user, Chat chat, LinkedList<Socket> receivers, Dictionary<string, string> data) : base(_user: user, _chat: chat, _receivers: receivers, _data: data) { }

        public void Handle()
        {

        }
    }
}
