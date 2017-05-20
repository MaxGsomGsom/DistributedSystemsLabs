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
            Console.WriteLine("[[[ Press Q to quit or press BACKSPACE to add random task ]]]");

            //generate some tasks
            var r = new Random();
            List<ComputingTask> list = new List<ComputingTask>();
            int count = r.Next(10, 20);
            for (int i = 0; i < count; i++)
            {
                list.Add(new ComputingTask(r.Next(1000, 10000)));
            }

            //create client
            Client cli = new Client("localhost:12345", list);
            cli.Connect();

            cli.StartComputingTasks();


            while (true)
            {
                var key = Console.ReadKey().Key;

                if (key == ConsoleKey.Q) break;

                else if (key == ConsoleKey.Backspace)
                {
                    var task = new ComputingTask(r.Next(1000, 10000));
                    cli.AddTaskToCompute(task);

                }
            }

            cli.Disconnect();
        }
    }
}
