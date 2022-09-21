using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSupportManagement.Models
{
    public class Ticket
    {
        public string Description { get; set; }
        public string Severity { get; set; }
        public string SubscriptionId { get; set; }
        public string PreferredSupportLanguage { get; set; }
        public string ServiceType { get; set; }
        public string PreferredContactMethod { get; set; }
        public string Summary { get; set; }
        public string ProblemClassification { get; set; }
        public DateTime ProblemStartDate { get; set; }
        public string ProblemStartTime { get; set; }
    }
}
