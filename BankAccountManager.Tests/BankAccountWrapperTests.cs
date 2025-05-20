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
        /// ����� ���������� �������.
        /// ����� ���: amount > 0 (���������, 1000).
        /// ����������: ������ ������� ����������.
        /// </summary>
        [TestMethod]
        public void Deposit_IncreasesBalance()
        {
            wrapper.Deposit(1000);
            Assert.AreEqual(1000, wrapper.GetBalance());
        }

        /// <summary>
        /// ����� ������ ����� � �������.
        /// ����� ���: �������� ���������� 1000, ���� ������ 300.
        /// ����������: ������ = 700.
        /// </summary>
        [TestMethod]
        public void Withdraw_DecreasesBalance()
        {
            wrapper.Deposit(1000);
            wrapper.Withdraw(300);
            Assert.AreEqual(700, wrapper.GetBalance());
        }

        /// <summary>
        /// ����� ������ ����� ����, �� � �� ������.
        /// ����� ���: ������� 500, ������ 1000.
        /// ����������: ������ �� ���������.
        /// </summary>
        [TestMethod]
        public void Withdraw_TooMuch_DoesNotChangeBalance()
        {
            wrapper.Deposit(500);
            wrapper.Withdraw(1000);
            Assert.AreEqual(500, wrapper.GetBalance());
        }

        /// <summary>
        /// ����� ���������� ���� "deposit".
        /// ����� ���: amount = 200, type = "deposit".
        /// ����������: ������ = 200.
        /// </summary>
        [TestMethod]
        public void PerformTransaction_Deposit_WorksCorrectly()
        {
            wrapper.PerformTransaction(200, "deposit");
            Assert.AreEqual(200, wrapper.GetBalance());
        }

        /// <summary>
        /// ����� ���������� ���� "withdraw".
        /// ����� ���: ������� 500, ���� ������ 100.
        /// ����������: ������ = 400.
        /// </summary>
        [TestMethod]
        public void PerformTransaction_Withdraw_WorksCorrectly()
        {
            wrapper.Deposit(500);
            wrapper.PerformTransaction(100, "withdraw");
            Assert.AreEqual(400, wrapper.GetBalance());
        }

        /// <summary>
        /// ��������, �� ����������� ��������� ���� ���������� �������.
        /// ����������: ������ SendBalanceChangeNotification � ���������� 1000.
        /// </summary>
        [TestMethod]
        public void Deposit_CallsBalanceChangeNotification()
        {
            wrapper.Deposit(1000);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1000), Times.Once);
        }

        /// <summary>
        /// ��������, �� ����������� ��������� ��� ���������� ��� �����.
        /// ����������: ����������� ������� ������ ����� "������".
        /// </summary>
        [TestMethod]
        public void Withdraw_CallsTransactionNotification()
        {
            wrapper.Deposit(500);
            wrapper.Withdraw(200);
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("������"))), Times.Once);
        }

        /// <summary>
        /// �������� ��������� ���������� � ������ �� ���������� ���������.
        /// ����������: ������ SendTransactionNotification � ��������� ������.
        /// </summary>
        [TestMethod]
        public void AddTransaction_AddsToHistory_AndSendsNotification()
        {
            wrapper.AddTransaction("������� ����������");
            mockNotification.Verify(n => n.SendTransactionNotification("������� ����������"), Times.Once);
        }

        /// <summary>
        /// ����� ��������� �������.
        /// ����� ���: ������ �� 1500.
        /// ����������: ������ � ���� ������� = 1500, ��� ���������.
        /// </summary>
        [TestMethod]
        public void TakeLoan_IncreasesBalanceAndLoanBalance()
        {
            wrapper.TakeLoan(1500);
            Assert.AreEqual(1500, wrapper.GetBalance());
            Assert.AreEqual(1500, wrapper.GetLoanBalance());
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1500), Times.Once);
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("�������� ������"))), Times.Once);
        }

        /// <summary>
        /// ����� �������� ��������� �������.
        /// ����� ���: ������ 2000, ��������� 500.
        /// ����������: ���� ������� = 1500, ������ = 1500.
        /// </summary>
        [TestMethod]
        public void RepayLoan_DecreasesLoanAndBalance_WhenSufficient()
        {
            wrapper.TakeLoan(2000);
            wrapper.RepayLoan(500);
            Assert.AreEqual(1500, wrapper.GetLoanBalance());
            Assert.AreEqual(1500, wrapper.GetBalance());
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("��������� �������"))), Times.Once);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(2000), Times.Once);
            mockNotification.Verify(n => n.SendBalanceChangeNotification(1500), Times.Once);
        }

        /// <summary>
        /// ����� ������ �������� �����, �� ������� �� �������.
        /// ����������: ����� �� ���������, ��������� �� �����������.
        /// </summary>
        [TestMethod]
        public void RepayLoan_TooMuch_DoesNotChangeLoanOrBalance()
        {
            wrapper.TakeLoan(1000);
            wrapper.RepayLoan(1500);
            Assert.AreEqual(1000, wrapper.GetLoanBalance());
            Assert.AreEqual(1000, wrapper.GetBalance());
            mockNotification.Verify(n => n.SendTransactionNotification(It.Is<string>(s => s.Contains("��������� �������"))), Times.Never);
        }

        /// <summary>
        /// ����� ���� � ����������� ��������� ������.
        /// ����������: ����������� true, isAuthenticated = true.
        /// </summary>
        [TestMethod]
        public void Login_WithCorrectCredentials_ReturnsTrueAndSetsAuthenticated()
        {
            var result = wrapper.Login("user", "password");
            Assert.IsTrue(result);
            Assert.IsTrue(wrapper.IsAuthenticated());
        }

        /// <summary>
        /// ����� ���� � ������������� ������.
        /// ����������: ����������� false, isAuthenticated = false.
        /// </summary>
        [TestMethod]
        public void Login_WithIncorrectCredentials_ReturnsFalse()
        {
            var result = wrapper.Login("admin", "wrongpass");
            Assert.IsFalse(result);
            Assert.IsFalse(wrapper.IsAuthenticated());
        }

        /// <summary>
        /// ����� ����� � ��������� ������.
        /// ����������: isAuthenticated = false ���� ������� Logout().
        /// </summary>
        [TestMethod]
        public void Logout_ResetsAuthenticatedToFalse()
        {
            wrapper.Login("user", "password");
            wrapper.Logout();
            Assert.IsFalse(wrapper.IsAuthenticated());
        }

        /// <summary>
        /// ����� ��������� ����� ����������.
        /// ����� ���: 2 ����������.
        /// ����������: ���� ������ ������ ����� ����������.
        /// </summary>
        [TestMethod]
        public void GetTransactionHistory_PrintsAllTransactions()
        {
            wrapper.AddTransaction("������� 1");
            wrapper.AddTransaction("������� 2");

            var output = new StringWriter();
            Console.SetOut(output);

            wrapper.GetTransactionHistory();

            var consoleOutput = output.ToString();
            StringAssert.Contains(consoleOutput, "������ ����������:");
            StringAssert.Contains(consoleOutput, "������� 1");
            StringAssert.Contains(consoleOutput, "������� 2");
        }
    }

}