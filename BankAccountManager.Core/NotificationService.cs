using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public class NotificationService : INotification
    {
        public void SendTransactionNotification(string message)
        {
            Console.WriteLine($"[Транзакція] {message}");
        }

        public void SendBalanceChangeNotification(decimal balance)
        {
            Console.WriteLine($"[Баланс] Новий баланс: {balance} грн");
        }
    }
}
