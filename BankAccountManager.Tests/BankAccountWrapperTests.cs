using Moq;
using BankAccountManager.Core;
using BankAccountManager.Wrappers;
using System.Reflection;

namespace BankAccountManager.Tests
{
    [TestClass]
    public class BankAccountWrapperTests
    {
        private BankAccountWrapper wrapper;
        private Mock<INotification> mockNotification;

        [TestInitialize]
        public void Setup()
        {
            mockNotification = new Mock<INotification>();
            var account = new BankAccount();
            wrapper = new BankAccountWrapper(account);
            wrapper.SetNotificationService(mockNotification.Object);
        }

        /// <summary>
        /// Тестує поповнення балансу.
        /// Вхідні дані: amount > 0 (наприклад, 1000).
        /// Очікування: баланс повинен збільшитись.
        /// </summary>
        [TestMethod]
        public void Deposit_IncreasesBalance()
        {
            wrapper.Deposit(1000);
            Assert.AreEqual(1000, wrapper.GetBalance());
        }

        /// <summary>
        /// Тестує зняття коштів з балансу.
        /// Вхідні дані: спочатку поповнення 1000, потім зняття 300.
        /// Очікування: баланс = 700.
        /// </summary>
        [TestMethod]
        public void Withdraw_DecreasesBalance()
        {
            wrapper.Deposit(1000);
            wrapper.Withdraw(300);
            Assert.AreEqual(700, wrapper.GetBalance());
        }

        /// <summary>
        /// Тестує зняття більшої суми, ніж є на балансі.
        /// Вхідні дані: депозит 500, зняття 1000.
        /// Очікування: баланс не змінюється.
        /// </summary>
        [TestMethod]
        public void Withdraw_TooMuch_DoesNotChangeBalance()
        {
            wrapper.Deposit(500);
            wrapper.Withdraw(1000);
            Assert.AreEqual(500, wrapper.GetBalance());
        }

        /// <summary>
        /// Тестує транзакцію типу "deposit".
        /// Вхідні дані: amount = 200, type = "deposit".
        /// Очікування: баланс = 200.
        /// </summary>
        [TestMethod]
        public void PerformTransaction_Deposit_WorksCorrectly()
        {
            wrapper.PerformTransaction(200, "deposit");
            Assert.AreEqual(200, wrapper.GetBalance());
        }

        /// <summary>
        /// Тестує транзакцію типу "withdraw".
        /// Вхідні дані: депозит 500, потім зняття 100.
        /// Очікування: баланс = 400.
        /// </summary>
        [TestMethod]
        public void PerformTransaction_Withdraw_WorksCorrectly()
        {
            wrapper.Deposit(500);
            wrapper.PerformTransaction(100, "withdraw");
            Assert.AreEqual(400, wrapper.GetBalance());
        }

        /// <summary>
        /// Перевіряє, чи надсилається сповіщення після поповнення балансу.
        /// Очікування: виклик SendBalanceChangeNotification з параметром 1000.
        /// </summary>
        [TestMethod]
        public void Deposit_CallsBalanceChangeNotification()
        {
            wrapper.Deposit(1000);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1000), Times.Once);
        }

        /// <summary>
        /// Перевіряє, чи надсилається сповіщення про транзакцію при знятті.
        /// Очікування: повідомлення повинно містити слово "Зняття".
        /// </summary>
        [TestMethod]
        public void Withdraw_CallsTransactionNotification()
        {
            wrapper.Deposit(500);
            wrapper.Withdraw(200);
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Зняття"))), Times.Once);
        }

        /// <summary>
        /// Перевіряє додавання транзакції в історію та надсилання сповіщення.
        /// Очікування: виклик SendTransactionNotification з переданим описом.
        /// </summary>
        [TestMethod]
        public void AddTransaction_AddsToHistory_AndSendsNotification()
        {
            wrapper.AddTransaction("Тестова транзакція");
            mockNotification.Verify(n => n.SendTransactionNotification("Тестова транзакція"), Times.Once);
        }

        /// <summary>
        /// Тестує отримання кредиту.
        /// Вхідні дані: кредит на 1500.
        /// Очікування: баланс і сума кредиту = 1500, два сповіщення.
        /// </summary>
        [TestMethod]
        public void TakeLoan_IncreasesBalanceAndLoanBalance()
        {
            wrapper.TakeLoan(1500);
            Assert.AreEqual(1500, wrapper.GetBalance());
            Assert.AreEqual(1500, wrapper.GetLoanBalance());
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1500), Times.Once);
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Отримано кредит"))), Times.Once);
        }

        /// <summary>
        /// Тестує часткове погашення кредиту.
        /// Вхідні дані: кредит 2000, погашення 500.
        /// Очікування: сума кредиту = 1500, баланс = 1500.
        /// </summary>
        [TestMethod]
        public void RepayLoan_DecreasesLoanAndBalance_WhenSufficient()
        {
            wrapper.TakeLoan(2000);
            wrapper.RepayLoan(500);
            Assert.AreEqual(1500, wrapper.GetLoanBalance());
            Assert.AreEqual(1500, wrapper.GetBalance());
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Погашення кредиту"))), Times.Once);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(2000), Times.Once);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1500), Times.Once);
        }

        /// <summary>
        /// Тестує спробу погасити більше, ніж залишок по кредиту.
        /// Очікування: нічого не змінюється, сповіщення не надсилається.
        /// </summary>
        [TestMethod]
        public void RepayLoan_TooMuch_DoesNotChangeLoanOrBalance()
        {
            wrapper.TakeLoan(1000);
            wrapper.RepayLoan(1500);
            Assert.AreEqual(1000, wrapper.GetLoanBalance());
            Assert.AreEqual(1000, wrapper.GetBalance());
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("Погашення кредиту"))), Times.Never);
        }

        /// <summary>
        /// Тестує вхід з правильними обліковими даними.
        /// Очікування: повертається true, isAuthenticated = true.
        /// </summary>
        [TestMethod]
        public void Login_WithCorrectCredentials_ReturnsTrueAndSetsAuthenticated()
        {
            var result = wrapper.Login("user", "password");
            Assert.IsTrue(result);
            Assert.IsTrue(wrapper.IsAuthenticated());
        }

        /// <summary>
        /// Тестує вхід з неправильними даними.
        /// Очікування: повертається false, isAuthenticated = false.
        /// </summary>
        [TestMethod]
        public void Login_WithIncorrectCredentials_ReturnsFalse()
        {
            var result = wrapper.Login("admin", "wrongpass");
            Assert.IsFalse(result);
            Assert.IsFalse(wrapper.IsAuthenticated());
        }

        /// <summary>
        /// Тестує вихід з облікового запису.
        /// Очікування: isAuthenticated = false після виклику Logout().
        /// </summary>
        [TestMethod]
        public void Logout_ResetsAuthenticatedToFalse()
        {
            wrapper.Login("user", "password");
            wrapper.Logout();
            Assert.IsFalse(wrapper.IsAuthenticated());
        }

        /// <summary>
        /// Тестує виведення історії транзакцій.
        /// Вхідні дані: 2 транзакції.
        /// Очікування: вивід містить обидва описи транзакцій.
        /// </summary>
        [TestMethod]
        public void GetTransactionHistory_PrintsAllTransactions()
        {
            wrapper.AddTransaction("Переказ 1");
            wrapper.AddTransaction("Переказ 2");

            var output = new StringWriter();
            Console.SetOut(output);

            wrapper.GetTransactionHistory();

            var consoleOutput = output.ToString();
            StringAssert.Contains(consoleOutput, "Історія транзакцій:");
            StringAssert.Contains(consoleOutput, "Переказ 1");
            StringAssert.Contains(consoleOutput, "Переказ 2");
        }
    }

}