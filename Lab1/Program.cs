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

            Bank bank = new Bank();
            Buyer buyer = new Buyer();
            Shop shop = new Shop();

            buyer.BuyerEvent += bank.OnBuyerEvent;
            buyer.BuyerEvent += shop.OnBuyerEvent;

            shop.ShopEvent += bank.OnShopEvent;
            shop.ShopEvent += buyer.OnShopEvent;

            bank.BankEvent += shop.OnBankEvent;
            bank.BankEvent += buyer.OnBankEvent;

            bank.RunInThread();
            buyer.RunInThread();
            shop.RunInThread();

        }
    }
}
