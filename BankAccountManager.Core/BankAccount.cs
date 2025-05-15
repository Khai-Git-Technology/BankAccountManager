using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public class BankAccount : IBankAccount, IAccountOperations, IAuthentication, ICreditAccount, ITransactionHistory
    {
        private decimal balance;
        private decimal loanBalance;
        private List<string> transactionHistory = new();
        private bool isAuthenticated = false;
        private INotification notificationService;

        public void SetNotificationService(INotification notificationService)
        {
            this.notificationService = notificationService;
        }
        public void Deposit(decimal amount)
        {
            balance += amount;
            AddTransaction($"Поповнення: {amount} грн");
            notificationService?.SendBalanceChangeNotification(balance);
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
            notificationService?.SendBalanceChangeNotification(balance);
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

        public void TakeLoan(decimal amount)
        {
            loanBalance += amount;
            balance += amount;
            AddTransaction($"Отримано кредит: {amount} грн");
            notificationService?.SendBalanceChangeNotification(balance);
        }
        public void AddTransaction(string details)
        {
            transactionHistory.Add(details);
            notificationService.SendTransactionNotification(details);
        }
        public bool Login(string username, string password)
        {
            if (username == "user" && password == "password")
            {
                isAuthenticated = true;
                Console.WriteLine("Вхід виконано успішно!");
                return true;
            }
            Console.WriteLine("Невірні облікові дані.");
            return false;
        }
        public bool CheckAccessRights(string role) => isAuthenticated;
        public void Logout()
        {
            isAuthenticated = false;
            Console.WriteLine("Вихід із системи виконано.");
        }

        public void RepayLoan(decimal amount)
        {
            if (amount > loanBalance)
            {
                Console.WriteLine("Сума перевищує залишок кредиту!");
                return;
            }
            loanBalance -= amount;
            balance -= amount;
            AddTransaction($"Погашення кредиту: {amount} грн");
            notificationService?.SendBalanceChangeNotification(balance);
        }

        public decimal CheckLoanBalance() => loanBalance;

        public void GetTransactionHistory()
        {
            Console.WriteLine("Історія транзакцій:");
            foreach (var transaction in transactionHistory)
            {
                Console.WriteLine(transaction);
            }
        }
    }
}
