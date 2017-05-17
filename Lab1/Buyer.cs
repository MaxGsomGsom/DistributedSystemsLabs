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
    public class Buyer
    {
        public event EventHandler<BuyerEventArgs> ToShopEvent;
        public event EventHandler<BuyerEventArgs> ToBankEvent;

        int threadTime = 0; //current thread time
        int LBTS = 0; //Lower Bound on the Time Stamp
        int shopLBTS = 0;
        int bankLBTS = 0;

        bool stop = false;
        StreamWriter log;
        Thread thread = null;

        Random gen = new Random((int)DateTime.Now.Ticks);

        Queue<BaseEventArgs> ownTasks = new Queue<BaseEventArgs>();
        ConcurrentQueue<BankEventArgs> bankTasks = new ConcurrentQueue<BankEventArgs>();
        ConcurrentQueue<ShopEventArgs> shopTasks = new ConcurrentQueue<ShopEventArgs>();

        DateTime NextMessageCreation = DateTime.Now;

        public Buyer() { }

        /// <summary>
        /// Bank -> buyer messages channel
        /// </summary>
        public void OnFromBankEvent(object sender, BankEventArgs e)
        {
            if (e.Timestamp < bankLBTS) throw new TimestampException();

            if (!HandleNullMessage(e))
            {
                bankTasks.Enqueue(e);
                bankLBTS = e.Timestamp;
                LBTS = Math.Min(bankLBTS, shopLBTS);
            }
        }

        /// <summary>
        /// Shop -> buyer messages channel
        /// </summary>
        public void OnFromShopEvent(object sender, ShopEventArgs e)
        {
            if (e.Timestamp < shopLBTS) throw new TimestampException();

            if (!HandleNullMessage(e))
            {
                shopTasks.Enqueue(e);
                shopLBTS = e.Timestamp;
                LBTS = Math.Min(bankLBTS, shopLBTS);
            }
        }

        /// <summary>
        /// Main method
        /// </summary>
        void Run()
        {
            log = new StreamWriter(File.OpenWrite("buyer.txt"));

            while (true)
            {
                //wait while all channels have at least 1 task
                while (bankTasks.Count == 0 /*|| shopTasks.Count == 0*/ && !stop) //shop don't send messages
                {
                    SendNewRandomMessage();
                }
                if (stop) break;

                //do task from channel with min timestamp
                //if (shopTasks.First().Timestamp < bankTasks.First().Timestamp)
                //{
                //    ShopEventArgs e;
                //    if (shopTasks.TryDequeue(out e))
                //        WorkOnTask(e);
                //}
                //else
                //{
                BankEventArgs e;
                if (bankTasks.TryDequeue(out e))
                    WorkOnTask(e);
                //}

                //do own task
                if (ownTasks.Count > 0)
                    WorkOnTask(ownTasks.Dequeue());

                SendNewRandomMessage();
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
            Console.WriteLine((threadTime / 1000.0).ToString() + " | task | buyer | " + task.EventType.ToString());
            log.WriteLine((threadTime / 1000.0).ToString() + " | task | buyer | " + task.EventType.ToString());

            int addTime = gen.Next(100, 2000);
            Thread.Sleep(addTime);
            threadTime += addTime;
            SendNullMessages();
        }

        /// <summary>
        /// Create and send random message
        /// </summary>
        void SendNewRandomMessage()
        {
            if (NextMessageCreation < DateTime.Now)
            {
                int addTime = gen.Next(100, 2000);
                NextMessageCreation = DateTime.Now.AddMilliseconds(addTime);
                threadTime += addTime;

                if (gen.Next(0, 2) == 0)
                {
                    //buyer buys goods
                    ToShopEvent?.Invoke(this, new BuyerEventArgs(EventType.BuyInCredit, threadTime));
                    log.WriteLine((threadTime / 1000.0).ToString() + " | message | buyer => shop | BuyInCredit");
                }
                else
                {
                    //buyer wants to withdraw money from bank
                    ToBankEvent?.Invoke(this, new BuyerEventArgs(EventType.WithdrawMoney, threadTime));
                    log.WriteLine((threadTime / 1000.0).ToString() + " | message | buyer => bank | WithdrawMoney");
                }
            }

        }

        /// <summary>
        /// Send null messages in all channels
        /// </summary>
        void SendNullMessages()
        {
            ToBankEvent?.Invoke(this, new BuyerEventArgs(EventType.Null, threadTime));
            log.WriteLine((threadTime / 1000.0).ToString() + " | message | buyer => bank | Null");

            ToShopEvent?.Invoke(this, new BuyerEventArgs(EventType.Null, threadTime));
            log.WriteLine((threadTime / 1000.0).ToString() + " | message | buyer => shop | Null");
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
    /// Message from buyer
    /// </summary>
    public class BuyerEventArgs : BaseEventArgs
    {
        public BuyerEventArgs(EventType type, int timestamp) : base(type, timestamp) { }
    }
}
