using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    enum RequestType
    {
        Authorize,
        Register,
        SendMessage,
        GetChats,
        SetChat,
        UnsetChat,
        GetMessages,
        SpreadMessage,
        CreateChat,
        AddToChat,
        RemoveFromChat,
        Disconnect
    }

    class Request
    {
        public RequestType Type { get; set; }
        public Dictionary<string,string> Data { get; set; }
    }
}
