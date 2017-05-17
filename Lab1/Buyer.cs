using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    public class Buyer
    {
        public event EventHandler<BuyerEventArgs> BuyerEvent;
        int LBTS = 0;

        Queue<EventArgs> ownTasks;
        Queue<BankEventArgs> bankTasks;
        Queue<ShopEventArgs> shopTasks;

        public Buyer() { }

        public void OnBankEvent(object sender, BankEventArgs e)
        {
            bankTasks.Enqueue(e);
        }

        public void OnShopEvent(object sender, ShopEventArgs e)
        {
            shopTasks.Enqueue(e);
        }


        public void Run()
        {

        }

        public void RunInThread()
        {
            ThreadStart buyerStart = new ThreadStart(Run);
            Thread buyerThread = new Thread(buyerStart);
            buyerThread.Start();
        }
    }

    public class BuyerEventArgs : EventArgs
    {
        public BuyerEventType EventType { get; private set; }

        public BuyerEventArgs(BuyerEventType type)
        {
            EventType = type;
        }
    }

    public enum BuyerEventType
    {
        BuyInCredit,
        WithdrawMoney
    }
}
