using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    public enum EventType
    {
        Null,
        Any,
        CreditTransaction,
        BuyInCredit,
        WithdrawMoney,
        ConfirmMoneyWithdrawal,
        RefuseMoneyWithdrawal
    }

    /// <summary>
    /// Message between classes
    /// </summary>
    public class BaseEventArgs
    {
        public EventType EventType { get; private set; }

        public int Timestamp { get; private set; }

        public BaseEventArgs(EventType type, int timestamp)
        {
            EventType = type;
            Timestamp = timestamp;
        }
    }
}
