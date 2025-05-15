using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public interface ITransactionHistory
    {
        void AddTransaction(string details);
        void GetTransactionHistory();
    }
}
