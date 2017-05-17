using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    public class Shop
    {
        public event EventHandler<ShopEventArgs> ShopEvent;
        int LBTS = 0;

        Queue<EventArgs> ownTasks;
        Queue<BuyerEventArgs> buyerTasks;
        Queue<BankEventArgs> bankTasks;

        public Shop() { }

        public void OnBuyerEvent(object sender, BuyerEventArgs e)
        {
            buyerTasks.Enqueue(e);
        }

        public void OnBankEvent(object sender, BankEventArgs e)
        {
            bankTasks.Enqueue(e);
        }


        public void Run()
        {

        }

        public void RunInThread()
        {
            ThreadStart shopStart = new ThreadStart(Run);
            Thread shopThread = new Thread(shopStart);
            shopThread.Start();
        }

    }

    public class ShopEventArgs : EventArgs
    {
        public ShopEventType EventType { get; private set; }

        public ShopEventArgs(ShopEventType type)
        {
            EventType = type;
        }
    }

    public enum ShopEventType
    {
        TransactionInCredit
    }
}
