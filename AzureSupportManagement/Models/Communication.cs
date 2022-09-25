using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSupportManagement.Models
{
    public class Communication
    {
        public string TicketName { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public string SubscriptionId { get; set; }
    }
}
