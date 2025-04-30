using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public interface INotification
    {
        void SendTransactionNotification(string message);
        void SendBalanceChangeNotification(decimal balance);
    }
}
