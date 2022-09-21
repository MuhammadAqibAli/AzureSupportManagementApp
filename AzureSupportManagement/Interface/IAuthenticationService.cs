using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSupportManagement.Interface
{
    public interface IAuthenticationService
    {
        string GetToken(bool forceLogin = false);
    }
}
