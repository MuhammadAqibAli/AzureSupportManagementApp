using AzureSupportManagement.Models;
using Microsoft.Azure.Management.Support.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSupportManagement.Interface
{
    public interface ISupportService
    {
        List<SupportTicketDetails> GetSupportTicketList(string subscriptionId, int? top = null, string filter = null);

        List<ProblemClassification> GetProblemClassifications(string serviceType, string subscriptionId);

        BaseResponse CreateTicket(Ticket ticket);

        BaseResponse CreateCommunication(Communication communication);
        BaseResponse UpdateTicketStatus(UpdateStatus ticket);
    }
}
