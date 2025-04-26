using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    internal interface IAccountOperations
    {
        void PerformTransaction(decimal amount, string type);
        void ManageOperations();
    }
}
