using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public class BankAccount : IBankAccount
    {
        private decimal balance;
        private decimal loanBalance;
        private List<string> transactionHistory = new();
        private bool isAuthenticated = false;
        private readonly INotification notificationService;

        public void Deposit(decimal amount)
        {
            balance += amount;
            AddTransaction($"Поповнення: {amount} грн");
            notificationService.SendBalanceChangeNotification(balance);
        }

        public void Withdraw(decimal amount)
        {
            if (amount > balance)
            {
                Console.WriteLine("Недостатньо коштів!");
                return;
            }
            balance -= amount;
            AddTransaction($"Зняття: {amount} грн");
            notificationService.SendBalanceChangeNotification(balance);
        }

        public decimal GetBalance() => balance;


        public void AddTransaction(string details)
        {
            transactionHistory.Add(details);
            notificationService.SendTransactionNotification(details);
        }

    }
}
