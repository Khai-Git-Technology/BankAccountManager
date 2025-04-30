using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public class BankAccount : IBankAccount, IAccountOperations
    {
        private decimal balance;
        private decimal loanBalance;
        private List<string> transactionHistory = new();
        private bool isAuthenticated = false;
        private readonly INotification notificationService;

        public BankAccount(INotification notificationService)
        {
            this.notificationService = notificationService;
        }
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

        public void PerformTransaction(decimal amount, string type)
        {
            if (type == "deposit") Deposit(amount);
            else if (type == "withdraw") Withdraw(amount);
        }

        public void ManageOperations()
        {
            Console.WriteLine("Операції рахунку керуються через інші методи.");
        }


        public void AddTransaction(string details)
        {
            transactionHistory.Add(details);
            notificationService.SendTransactionNotification(details);
        }

    }
}
