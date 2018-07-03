using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Data.OleDb;
using MySql.Data.MySqlClient;
using System.Threading;


namespace Server
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter server IP - ");
            String Host = Console.ReadLine();
            Console.Write("Enter server Port - ");
            Int32 Port = int.Parse(Console.ReadLine());

            Server server = new Server(Host, Port);
            server.Run();

            Console.ReadKey();

        }
    }
}
