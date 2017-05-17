using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab1
{
    public class Shop
    {
        public event EventHandler<ShopEventArgs> ToBankEvent;
        public event EventHandler<ShopEventArgs> ToBuyerEvent;

        int threadTime = 0; //current thread time
        int LBTS = 0; //Lower Bound on the Time Stamp
        int buyerLBTS = 0;
        int bankLBTS = 0;

        bool stop = false;
        StreamWriter log;
        Thread thread = null;

        Random gen = new Random((int)DateTime.Now.Ticks);

        Queue<BaseEventArgs> ownTasks = new Queue<BaseEventArgs>();
        ConcurrentQueue<BuyerEventArgs> buyerTasks = new ConcurrentQueue<BuyerEventArgs>();
        ConcurrentQueue<BankEventArgs> bankTasks = new ConcurrentQueue<BankEventArgs>();

        DateTime NextMessageCreation = DateTime.Now;

        public Shop() { }

        /// <summary>
        /// Buyer -> shop messages channel
        /// </summary>
        public void OnFromBuyerEvent(object sender, BuyerEventArgs e)
        {
            if (e.Timestamp < buyerLBTS) throw new TimestampException();

            if (!HandleNullMessage(e))
            {
                buyerTasks.Enqueue(e);
                buyerLBTS = e.Timestamp;
                LBTS = Math.Min(bankLBTS, buyerLBTS);
            }
        }

        /// <summary>
        /// Bank -> shop messages channel
        /// </summary>
        public void OnFromBankEvent(object sender, BankEventArgs e)
        {
            if (e.Timestamp < bankLBTS) throw new TimestampException();

            if (!HandleNullMessage(e))
            {
                bankTasks.Enqueue(e);
                bankLBTS = e.Timestamp;
                LBTS = Math.Min(bankLBTS, buyerLBTS);
            }
        }

        /// <summary>
        /// Main method
        /// </summary>
        void Run()
        {
            log = new StreamWriter(File.OpenWrite("shop.txt"));

            while (true)
            {
                //wait while all channels have at least 1 task
                while (buyerTasks.Count == 0 /*|| bankTasks.Count == 0*/ && !stop) { } //bank don't send messages
                if (stop) break;

                //do task from channel with min timestamp
                //if (buyerTasks.First().Timestamp < bankTasks.First().Timestamp)
                //{
                BuyerEventArgs e;
                if (buyerTasks.TryDequeue(out e))
                    WorkOnTask(e);
                //}
                //else
                //{
                //    BankEventArgs e;
                //    if (bankTasks.TryDequeue(out e))
                //        WorkOnTask(e);
                //}

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
            Console.WriteLine((threadTime / 1000.0).ToString() + " | task | shop | " + task.EventType.ToString());
            log.WriteLine((threadTime / 1000.0).ToString() + " | task | shop | " + task.EventType.ToString());

            int addTime = gen.Next(100, 2000);
            Thread.Sleep(addTime);
            threadTime += addTime;
            SendNullMessages();

            if (task.EventType == EventType.BuyInCredit)
            {
                //shop informs bank about transaction
                ToBankEvent?.Invoke(this, new ShopEventArgs(EventType.CreditTransaction, threadTime));
                log.WriteLine((threadTime / 1000.0).ToString() + " | message | shop => bank | CreditTransaction");
            }
        }

        /// <summary>
        /// Send null messages in all channels
        /// </summary>
        void SendNullMessages()
        {
            ToBankEvent?.Invoke(this, new ShopEventArgs(EventType.Null, threadTime));
            log.WriteLine((threadTime / 1000.0).ToString() + " | message | shop => bank | Null");

            ToBuyerEvent?.Invoke(this, new ShopEventArgs(EventType.Null, threadTime));
            log.WriteLine((threadTime / 1000.0).ToString() + " | message | shop => buyer | Null");
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
    /// Message from shop
    /// </summary>
    public class ShopEventArgs : BaseEventArgs
    {
        public ShopEventArgs(EventType type, int timestamp) : base(type, timestamp) { }
    }
}
