using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace Lab1
{
    public class Bank
    {
        public event EventHandler<BankEventArgs> BankEvent;
        int LBTS = 0;

        Queue<EventArgs> ownTasks;
        Queue<BuyerEventArgs> buyerTasks;
        Queue<ShopEventArgs> shopTasks;

        public Bank() { }

        public void OnBuyerEvent(object sender, BuyerEventArgs e)
        {
            buyerTasks.Enqueue(e);
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
            ThreadStart bankStart = new ThreadStart(Run);
            Thread bankThread = new Thread(bankStart);
            bankThread.Start();
        }

    }


    public class BankEventArgs : EventArgs
    {
        public BankEventType EventType { get; private set; }

        public BankEventArgs(BankEventType type)
        {
            EventType = type;
        }
    }

    public enum BankEventType
    {
        ConfirmMoneyWithdrawal,
        RefuseMoneyWithdrawal

    }
}
