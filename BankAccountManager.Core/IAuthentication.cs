using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountManager.Core
{
    public interface IAuthentication
    {
        bool Login(string username, string password);
        void Logout();
        bool CheckAccessRights(string role);

    }
}
