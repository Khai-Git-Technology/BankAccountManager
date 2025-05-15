using Moq;
using BankAccountManager.Core;
using System.Reflection;

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
            account = new BankAccount();
            account.SetNotificationService(mockNotification.Object);
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
        [TestMethod]
        public void TakeLoan_IncreasesBalanceAndLoanBalance()
        {
            account.TakeLoan(1500);

            Assert.AreEqual(1500, account.GetBalance(), "Баланс має зрости на суму кредиту");
            Assert.AreEqual(1500, GetPrivateLoanBalance(account), "Кредит має зрости на суму кредиту");

            mockNotification.Verify(n => n.SendBalanceChangeNotification(1500), Times.Once);
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Отримано кредит"))), Times.Once);
        }

        [TestMethod]
        public void RepayLoan_DecreasesLoanAndBalance_WhenSufficient()
        {
            account.TakeLoan(2000);
            account.RepayLoan(500);

            Assert.AreEqual(1500, GetPrivateLoanBalance(account));
            Assert.AreEqual(1500, account.GetBalance());

            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Погашення кредиту"))), Times.Once);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(2000), Times.Once);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1500), Times.Once);
        }

        [TestMethod]
        public void RepayLoan_TooMuch_DoesNotChangeLoanOrBalance()
        {
            account.TakeLoan(1000);
            account.RepayLoan(1500); // більше ніж кредит

            Assert.AreEqual(1000, GetPrivateLoanBalance(account));
            Assert.AreEqual(1000, account.GetBalance());

            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Погашення кредиту"))), Times.Never);
        }
        [TestMethod]
        public void Login_WithCorrectCredentials_ReturnsTrueAndSetsAuthenticated()
        {
            var result = account.Login("user", "password");

            Assert.IsTrue(result);

            var field = typeof(BankAccount).GetField("isAuthenticated", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue((bool)field.GetValue(account));
        }
        [TestMethod]
        public void Login_WithIncorrectCredentials_ReturnsFalse()
        {
            var result = account.Login("admin", "wrongpass");

            Assert.IsFalse(result);

            var field = typeof(BankAccount).GetField("isAuthenticated", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsFalse((bool)field.GetValue(account));
        }

        [TestMethod]
        public void Logout_ResetsAuthenticatedToFalse()
        {
            account.Login("user", "password");
            account.Logout();

            var field = typeof(BankAccount).GetField("isAuthenticated", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsFalse((bool)field.GetValue(account));
        }


        [TestMethod]
        public void GetTransactionHistory_PrintsAllTransactions()
        {
            account.AddTransaction("Переказ 1");
            account.AddTransaction("Переказ 2");

            var output = new StringWriter();
            Console.SetOut(output);

            account.GetTransactionHistory();

            var consoleOutput = output.ToString();
            StringAssert.Contains(consoleOutput, "Історія транзакцій:");
            StringAssert.Contains(consoleOutput, "Переказ 1");
            StringAssert.Contains(consoleOutput, "Переказ 2");
        }
    }
}