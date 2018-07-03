using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Message
    {
        public string Id { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }

    }

    class Chat
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
    
    class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }
}
