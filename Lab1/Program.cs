using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Q to stop");

            Bank bank = new Bank();
            Buyer buyer = new Buyer();
            Shop shop = new Shop();

            //create channels between threads
            buyer.ToBankEvent += bank.OnFromBuyerEvent;
            buyer.ToShopEvent += shop.OnFromBuyerEvent;

            shop.ToBankEvent += bank.OnFromShopEvent;
            shop.ToBuyerEvent += buyer.OnFromShopEvent;

            bank.ToShopEvent += shop.OnFromBankEvent;
            bank.ToBuyerEvent += buyer.OnFromBankEvent;

            //start threads
            bank.RunInThread();
            buyer.RunInThread();
            shop.RunInThread();

            while (Console.ReadKey().Key != ConsoleKey.Q) { }

            bank.Stop();
            buyer.Stop();
            shop.Stop();

            Console.WriteLine("Press Q to quit");
            while (Console.ReadKey().Key != ConsoleKey.Q) { }
        }
    }
}
