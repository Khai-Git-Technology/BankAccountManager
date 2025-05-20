using BankAccountManager.Core;

namespace BankAccountManager.Wrappers
{
    public class BankAccountWrapper
    {
        private readonly BankAccount _account;

        public BankAccountWrapper(BankAccount account)
        {
            _account = account;
        }

        public void SetNotificationService(INotification notification)
        {
            _account.SetNotificationService(notification);
        }

        public void Deposit(decimal amount)
        {
            _account.Deposit(amount);
        }

        public void Withdraw(decimal amount)
        {
            _account.Withdraw(amount);
        }

        public decimal GetBalance()
        {
            return _account.GetBalance();
        }

        public void PerformTransaction(decimal amount, string type)
        {
            _account.PerformTransaction(amount, type);
        }

        public void AddTransaction(string description)
        {
            _account.AddTransaction(description);
        }

        public void TakeLoan(decimal amount)
        {
            _account.TakeLoan(amount);
        }

        public void RepayLoan(decimal amount)
        {
            _account.RepayLoan(amount);
        }

        public bool Login(string username, string password)
        {
            return _account.Login(username, password);
        }

        public void Logout()
        {
            _account.Logout();
        }

        public void GetTransactionHistory()
        {
            _account.GetTransactionHistory();
        }

        // Додатковий метод для доступу до приватного loanBalance у тестах
        public decimal GetLoanBalance()
        {
            var field = typeof(BankAccount).GetField("loanBalance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (decimal)field.GetValue(_account);
        }

        public bool IsAuthenticated()
        {
            var field = typeof(BankAccount).GetField("isAuthenticated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (bool)field.GetValue(_account);
        }
    }
}
