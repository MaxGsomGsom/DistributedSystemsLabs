using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

namespace Lab1
{
    public class Bank
    {
        public event EventHandler<BankEventArgs> ToShopEvent;
        public event EventHandler<BankEventArgs> ToBuyerEvent;

        int threadTime = 0; //current thread time
        int LBTS = 0; //Lower Bound on the Time Stamp
        int buyerLBTS = 0;
        int shopLBTS = 0;

        StreamWriter log;
        bool stop = false;
        Thread thread = null;

        Random gen = new Random((int)DateTime.Now.Ticks);

        Queue<BaseEventArgs> ownTasks = new Queue<BaseEventArgs>();
        ConcurrentQueue<BuyerEventArgs> buyerTasks = new ConcurrentQueue<BuyerEventArgs>();
        ConcurrentQueue<ShopEventArgs> shopTasks = new ConcurrentQueue<ShopEventArgs>();

        DateTime NextMessageVreation = DateTime.Now;

        public Bank() { }

        /// <summary>
        /// Buyer -> bank messages channel
        /// </summary>
        public void OnFromBuyerEvent(object sender, BuyerEventArgs e)
        {
            if (e.Timestamp < buyerLBTS) throw new TimestampException();

            if (!HandleNullMessage(e))
            {
                buyerTasks.Enqueue(e);
                buyerLBTS = e.Timestamp;
                LBTS = Math.Min(buyerLBTS, shopLBTS);
            }
        }

        /// <summary>
        /// Shop -> bank messages channel
        /// </summary>
        public void OnFromShopEvent(object sender, ShopEventArgs e)
        {
            if (e.Timestamp < shopLBTS) throw new TimestampException();

            if (!HandleNullMessage(e))
            {
                shopTasks.Enqueue(e);
                shopLBTS = e.Timestamp;
                LBTS = Math.Min(buyerLBTS, shopLBTS);
            }
        }

        /// <summary>
        /// Main method
        /// </summary>
        void Run()
        {
            log = new StreamWriter(File.OpenWrite("bank.txt"));

            while (true)
            {
                //wait while all channels have at least 1 task
                while ((shopTasks.Count == 0 || buyerTasks.Count == 0) && !stop) { }
                if (stop) break;

                //do task from channel with min timestamp
                if (shopTasks.First().Timestamp < buyerTasks.First().Timestamp)
                {
                    ShopEventArgs e;
                    if (shopTasks.TryDequeue(out e))
                        WorkOnTask(e);
                }
                else
                {
                    BuyerEventArgs e;
                    if (buyerTasks.TryDequeue(out e))
                        WorkOnTask(e);
                }

                //do own task
                if (ownTasks.Count > 0)
                    WorkOnTask(ownTasks.Dequeue());
            }
        }

        /// <summary>
        /// Runs main method in separated thread
        /// </summary>
        public void RunInThread()
        {
            ThreadStart threadStart = new ThreadStart(Run);
            thread = new Thread(threadStart);
            thread.Start();
        }

        /// <summary>
        /// Works on task for some time
        /// </summary>
        /// <param name="task"></param>
        void WorkOnTask(BaseEventArgs task)
        {
            Console.WriteLine((threadTime / 1000.0).ToString() + " | task | bank | " + task.EventType.ToString());
            log.WriteLine((threadTime / 1000.0).ToString() + " | task | bank | " + task.EventType.ToString());

            int addTime = gen.Next(100, 2000);
            Thread.Sleep(addTime);
            threadTime += addTime;
            SendNullMessages();

            if (task.EventType == EventType.WithdrawMoney)
            {
                if (gen.Next(0, 2) == 0)
                {
                    //bank confirms withdrawal
                    ToBuyerEvent?.Invoke(this, new BankEventArgs(EventType.ConfirmMoneyWithdrawal, threadTime));
                    log.WriteLine((threadTime / 1000.0).ToString() + " | message | bank => buyer | ConfirmMoneyWithdrawal");
                }
                else
                {
                    //bank refuses withdrawal
                    ToBuyerEvent?.Invoke(this, new BankEventArgs(EventType.RefuseMoneyWithdrawal, threadTime));
                    log.WriteLine((threadTime / 1000.0).ToString() + " | message | bank => buyer | RefuseMoneyWithdrawal");
                }
            }
        }

        /// <summary>
        /// Send null messages in all channels
        /// </summary>
        void SendNullMessages()
        {
            ToShopEvent?.Invoke(this, new BankEventArgs(EventType.Null, threadTime));
            log.WriteLine((threadTime / 1000.0).ToString() + " | message | bank => shop | Null");

            ToBuyerEvent?.Invoke(this, new BankEventArgs(EventType.Null, threadTime));
            log.WriteLine((threadTime / 1000.0).ToString() + " | message | bank => buyer | Null");
        }

        /// <summary>
        /// Stops execution of main function
        /// </summary>
        public void Stop()
        {
            stop = true;
        }


        /// <summary>
        /// Handle Null message
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool HandleNullMessage(BaseEventArgs e)
        {
            if (e.EventType != EventType.Null) return false;
            else if (threadTime < e.Timestamp)
            {
                threadTime = e.Timestamp;
                SendNullMessages();
            }
            return true;
        }
    }

    /// <summary>
    /// Message from Bank
    /// </summary>
    public class BankEventArgs : BaseEventArgs
    {
        public BankEventArgs(EventType type, int timestamp) : base(type, timestamp) { }
    }

}
