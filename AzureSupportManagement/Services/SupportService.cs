using AzureSupportManagement.Interface;
using AzureSupportManagement.Models;
using Microsoft.Azure.Management.Support;
using Microsoft.Azure.Management.Support.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Type = Microsoft.Azure.Management.Support.Models.Type;

namespace AzureSupportManagement.Services
{
    public class SupportService : ISupportService
    {
        private readonly IAuthenticationService _authenticationService;
        private string TICKETNAMEPREFIX = "";
        private static CustomLoginCredentials serviceClientCredentials;
        private static MicrosoftSupportClient supportClient;
        public IConfiguration _configuration;
        public SupportService(IAuthenticationService authenticationService, IConfiguration configuration)
        {
            _authenticationService = authenticationService;
            _configuration = configuration;
            initialize();
        }
        private void initialize()
        {
            TICKETNAMEPREFIX = _configuration.GetValue<string>("TICKETNAMEPREFIX");
            string token = _authenticationService.GetToken(false);
            serviceClientCredentials = new CustomLoginCredentials(token);
            supportClient = new MicrosoftSupportClient(serviceClientCredentials);
        }

        public List<SupportTicketDetails> GetSupportTicketList(string subscriptionId, int? top = null, string filter = null)
        {
            List<SupportTicketDetails> tickets = new List<SupportTicketDetails>();
            try
            {
                supportClient.SubscriptionId = subscriptionId;
                tickets = supportClient.SupportTickets.List(top, filter).ToList();
            }
            catch (Exception ex)
            {
            }
            return tickets;
        }

        public List<ProblemClassification> GetProblemClassifications(string serviceType, string subscriptionId)
        {
            supportClient.SubscriptionId = subscriptionId;
            //  1.Call services list api and find quota service name
            var rsp1 = GetServiceList();
            var serviceName = string.Empty;
            foreach (var service in rsp1)
            {
                if (service.DisplayName.ToLower().Contains(serviceType))
                {
                    serviceName = service.Name;
                    break;
                }
            }

            //  2. Call problem classification list api and find cores problem classification name
            var rsp2 = GetProblemClassificationList(serviceName);
            return rsp2;
        }

        public BaseResponse CreateTicket(Ticket ticket)
        {
            supportClient.SubscriptionId = ticket.SubscriptionId;
            BaseResponse response = new BaseResponse();
            try
            {
                string serviceName = GetServiceName(ticket.ServiceType, ticket.SubscriptionId);
                //string problemClassificationName = GetClassificationName(serviceName, ticket.ProblemClassification);

                //  Create random ticket name and call check name availability until unique name is not found
                var rsp3 = true;
                var randomTicketName = string.Empty;
                do
                {
                    randomTicketName = string.Format(TICKETNAMEPREFIX, DateTime.Today.ToString("%d_%M_%y"), new Random().Next(0, 10).ToString());
                    rsp3 = CheckNameAvailability("", new CheckNameAvailabilityInput()
                    {
                        Name = randomTicketName,
                        Type = Type.MicrosoftSupportSupportTickets
                    });
                } while (!rsp3);

                string userName = _configuration.GetValue<string>("UserEmail");
                string PreferredTimeZone = _configuration.GetValue<string>("PreferredTimeZone");
                string Country = _configuration.GetValue<string>("Country");
                //Create Ticket
                var inputPayload = new SupportTicketDetails()
                {
                    Severity = ticket.Severity,
                    Title = ticket.Summary,
                    Description = ticket.Description,
                    ContactDetails = new ContactProfile()
                    {
                        FirstName = userName,
                        LastName = "B",
                        PrimaryEmailAddress = userName,
                        PreferredContactMethod = ticket.PreferredContactMethod,
                        PreferredTimeZone = PreferredTimeZone,
                        PreferredSupportLanguage = ticket.PreferredSupportLanguage,
                        Country = Country
                    }
                };
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + ticket.ProblemClassification;
                var rsp = supportClient.SupportTickets.Create(randomTicketName, inputPayload);
                response.Success = true;
                response.Message = JsonConvert.SerializeObject(rsp);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public BaseResponse CreateCommunication(Communication communication)
        {
            supportClient.SubscriptionId = communication.SubscriptionId;
            BaseResponse response = new BaseResponse();
            try
            {
                //  Create random ticket communication name and call check name availability 
                var randomTicketCommunicationName = communication.TicketName + "_communication";
                var rsp5 = CheckNameAvailability(communication.TicketName, new CheckNameAvailabilityInput()
                {
                    Name = randomTicketCommunicationName,
                    Type = Type.MicrosoftSupportCommunications
                });
                string userName = _configuration.GetValue<string>("UserEmail");
                var rsp = supportClient.Communications.Create(communication.TicketName, randomTicketCommunicationName, new CommunicationDetails()
                {
                    Sender = userName,
                    Subject = communication.Subject,
                    Body = communication.Body
                });
                response.Message = JsonConvert.SerializeObject(rsp);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public BaseResponse UpdateTicketStatus(UpdateStatus ticket)
        {
            supportClient.SubscriptionId = ticket.SubscriptionId;
            BaseResponse response = new BaseResponse();
            try
            {
                var updatePayload = new UpdateSupportTicket()
                {
                    Status = ticket.Status
                };
                var rsp = supportClient.SupportTickets.Update(ticket.TicketName, updatePayload);
                response.Message = JsonConvert.SerializeObject(rsp);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private string GetServiceName(string serviceType, string subscriptionId)
        {
            var rsp1 = GetServiceList();
            var serviceName = string.Empty;
            foreach (var service in rsp1)
            {
                if (service.DisplayName.ToLower().Contains(serviceType))
                {
                    serviceName = service.Name;
                    break;
                }
            }
            return serviceName;
        }

        private static List<Service> GetServiceList()
        {
            return supportClient.Services.List().ToList();
        }

        private static List<ProblemClassification> GetProblemClassificationList(string serviceName)
        {
            return supportClient.ProblemClassifications.List(serviceName).ToList();
        }

        private static bool CheckNameAvailability(string supportTicketName, CheckNameAvailabilityInput inputPayload)
        {
            try
            {
                if (inputPayload.Type == Type.MicrosoftSupportSupportTickets)
                {
                    var rsp = supportClient.SupportTickets.CheckNameAvailability(inputPayload);
                    return rsp.NameAvailable ?? false;
                }
                else if (inputPayload.Type == Type.MicrosoftSupportCommunications)
                {
                    var rsp = supportClient.Communications.CheckNameAvailability(supportTicketName, inputPayload);
                    return rsp.NameAvailable ?? false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
