using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ClientStatus
    {
        public User User { get; set; }
        public Chat Chat { get; set; }

        private static Dictionary<string, ClientStatus> instances = new Dictionary<string, ClientStatus>();

        private ClientStatus() { }

        public static ClientStatus GetInstance(string ThreadId)
        {
            if (!instances.ContainsKey(ThreadId))
            {
                instances.Add(ThreadId, new ClientStatus());
            }
            return instances[ThreadId];
        }

        public static void RemoveInstance(string ThreadId)
        {
            if (instances.ContainsKey(ThreadId))
            {
                instances.Remove(ThreadId);
            }
        }
    }
}
