using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab3_interface;

namespace Lab3_client
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Press Q to quit or press BACKSPACE to add random task");

            Client cli = new Client("localhost:12345", new ComputingTask[] { });
            cli.Connect();

            var r = new Random();
            while (true)
            {
                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.Q) break;

                else if (key == ConsoleKey.Backspace)
                {
                    var task = new ComputingTask(r.Next(100, 5000));
                    cli.AddTaskToCompute(task);

                }
            }

            cli.Disconnect();
        }
    }
}
