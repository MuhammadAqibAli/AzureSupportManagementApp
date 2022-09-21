using AzureSupportManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSupportManagement.Interface
{
    public interface ISubscriptionService
    {
        List<Subscription> GetSubscriptions();
    }
}
