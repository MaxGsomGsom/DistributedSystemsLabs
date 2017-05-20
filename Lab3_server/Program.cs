using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3_server
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Press Q to quit");

            Server serv = new Server(12345);
            serv.StartServer();

            while (Console.ReadKey().Key != ConsoleKey.Q) { }

            serv.StopServer();
        }
    }

}
