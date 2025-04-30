using Moq;
using BankAccountManager.Core;

namespace BankAccountManager.Tests
{
    [TestClass]
    public class BankAccountTests
    {
        private BankAccount account;
        private Mock<INotification> mockNotification;

        [TestInitialize]
        public void Setup()
        {
            mockNotification = new Mock<INotification>();
            account = new BankAccount(mockNotification.Object);
        }
        private decimal GetPrivateLoanBalance(BankAccount account)
        {
            var field = typeof(BankAccount).GetField("loanBalance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (decimal)field.GetValue(account);
        }


        [TestMethod]
        public void Deposit_IncreasesBalance()
        {
            account.Deposit(1000);
            Assert.AreEqual(1000, account.GetBalance());
        }

        [TestMethod]
        public void Withdraw_DecreasesBalance()
        {
            account.Deposit(1000);
            account.Withdraw(300);
            Assert.AreEqual(700, account.GetBalance());
        }

        [TestMethod]
        public void Withdraw_TooMuch_DoesNotChangeBalance()
        {
            account.Deposit(500);
            account.Withdraw(1000);
            Assert.AreEqual(500, account.GetBalance());
        }

        [TestMethod]
        public void PerformTransaction_Deposit_WorksCorrectly()
        {
            account.PerformTransaction(200, "deposit");
            Assert.AreEqual(200, account.GetBalance());
        }

        [TestMethod]
        public void PerformTransaction_Withdraw_WorksCorrectly()
        {
            account.Deposit(500);
            account.PerformTransaction(100, "withdraw");
            Assert.AreEqual(400, account.GetBalance());
        }

        [TestMethod]
        public void Deposit_CallsBalanceChangeNotification()
        {
            account.Deposit(1000);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1000), Times.Once);
        }

        [TestMethod]
        public void Withdraw_CallsTransactionNotification()
        {
            account.Deposit(500);
            account.Withdraw(200);
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Зняття"))), Times.Once);
        }

        [TestMethod]
        public void AddTransaction_AddsToHistory_AndSendsNotification()
        {
            account.AddTransaction("Тестова транзакція");

            mockNotification.Verify(n => n.SendTransactionNotification("Тестова транзакція"), Times.Once);
        }
    }
}