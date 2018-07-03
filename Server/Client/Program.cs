using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter server IP - ");
            string Host = Console.ReadLine();
            Console.Write("Enter server Port - ");
            int Port = int.Parse(Console.ReadLine());

            Connection.Host = Host;
            Connection.Port = Port;

            Console.Write("Enter username - ");
            string username = Console.ReadLine();
            Console.Write("Enter password - ");
            string password = Console.ReadLine();

            try
            {
                Client client = Connection.Authorize(username, password);

                while (true)
                {
                    Console.WriteLine("----------------");
                    foreach (Chat chat in client.Chats())
                    {
                        Console.WriteLine("{0} - {1}", chat.Id, chat.Title);
                    }

                    try
                    {
                        Console.Write("Enter chat id - ");
                        string chatId = Console.ReadLine();
                        Chat Chat = new Chat() { Id = chatId };

                        try
                        {
                            client.OpenReadThread(Chat, (message) => Console.WriteLine("{0}: {1}", message.Author, message.Content));
                        }
                        catch (MultipleReadThreadsExecption exception)
                        {
                            Console.WriteLine(exception.Message);
                        }

                        foreach (Message message in client.ChatMessages(Chat))
                        {
                            Console.WriteLine("{0}: {1}", message.Author, message.Content);
                        }

                        while (true)
                        {
                            Console.Write("Enter new message - ");
                            string message = Console.ReadLine();
                            if (message == "?close?") break;
                            client.SendMessage(message, Chat);
                        }

                        client.CloseReadThread();
                    }
                    catch (InvalidChatException exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                    
                }

                client.Disconnect();
            }
            catch(AuthorizationException exception)
            {
                Console.WriteLine(exception.Message);
            }
            Console.ReadKey();
        }
    }
}
