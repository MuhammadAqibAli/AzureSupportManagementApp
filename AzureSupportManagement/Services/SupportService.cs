using AzureSupportManagement.Interface;
using AzureSupportManagement.Models;
using Microsoft.Azure.Management.Support;
using Microsoft.Azure.Management.Support.Models;
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

        private const string SUBID = "e5285bd8-3d19-4810-83af-7aa1527d14f9";
        private const string TICKETNAMEPREFIX = "ApiDemoConsoleApp_{0}_{1}";
        private const string OPTIONSSUFFIX = " for the subscription " + SUBID;
        private const string ERRORMSG = "\nSome error occured! Please file a github issue if you think there is an issue with the original code.";
        private static CustomLoginCredentials serviceClientCredentials;
        private static MicrosoftSupportClient supportClient;

        public SupportService(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            initialize();
        }
        private void initialize()
        {
            string token = _authenticationService.GetToken(false);

            // Setup support client and basic configuration like subscription ID
            serviceClientCredentials = new CustomLoginCredentials(token);
            supportClient = new MicrosoftSupportClient(serviceClientCredentials);
            supportClient.SubscriptionId = SUBID;

        }
        // Option 1: Get List of support tickets
        public List<SupportTicketDetails> GetSupportTicketList(string subscriptionId, int? top = null, string filter = null)
        {
            try
            {
                supportClient.SubscriptionId = subscriptionId;
                var response = supportClient.SupportTickets.List(top, filter).ToList();
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
                //return null;
            }
            return null;
        }

        public List<ProblemClassification> GetProblemClassifications(string serviceType)
        {
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
            BaseResponse response = new BaseResponse();
            try
            {
                string serviceName = GetServiceName(ticket.ServiceType);
                //string problemClassificationName = GetClassificationName(serviceName, ticket.ProblemClassification);

                //  3.Create random ticket name and call check name availability until unique name is not found
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

                //Create Ticket
                var inputPayload = new SupportTicketDetails()
                {
                    Severity = ticket.Severity,
                    Title = ticket.Summary,
                    Description = ticket.Description,
                    ContactDetails = new ContactProfile()
                    {
                        FirstName = "supportuser@riniehuijgenhotmail.onmicrosoft.com",
                        LastName = "B",
                        PrimaryEmailAddress = "supportuser@riniehuijgenhotmail.onmicrosoft.com",
                        PreferredContactMethod = ticket.PreferredContactMethod,
                        PreferredTimeZone = "Pacific Standard Time",
                        PreferredSupportLanguage = ticket.PreferredSupportLanguage,
                        Country = "usa"
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
                //response.Message = JsonConvert.SerializeObject(ex);
            }
            return response;
        }

        public BaseResponse CreateCommunication(Communication communication)
        {
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

                var rsp = supportClient.Communications.Create(communication.TicketName, randomTicketCommunicationName, new CommunicationDetails()
                {
                    Sender = "supportuser@riniehuijgenhotmail.onmicrosoft.com",
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

        private string GetServiceName(string serviceType)
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


        // Option 2: 
        //  1. Call services list api and find quota service name
        //  2. Call problem classification list api and find cores problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Create random ticket communication name and call check name availability 
        //  6. Add communication
        //  7. Close the support ticket
        private static void ExecuteOption2()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach (var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("service and subscription limits"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("compute-vm"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
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


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                inputPayload.QuotaTicketDetails = new QuotaTicketDetails()
                {
                    QuotaChangeRequestVersion = "1.0",
                    QuotaChangeRequests = new List<QuotaChangeRequest>()
                    {
                        new QuotaChangeRequest()
                        {
                            Region = "EastUS",
                            Payload = "{\"SKU\":\"DSv3 Series\",\"NewLimit\":104}"
                        }
                    }
                };
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5. Create random ticket communication name and call check name availability 
                var randomTicketCommunicationName = randomTicketName + "_communication";
                var rsp5 = CheckNameAvailability(randomTicketName, new CheckNameAvailabilityInput()
                {
                    Name = randomTicketCommunicationName,
                    Type = Type.MicrosoftSupportCommunications
                });


                //  6. Add communication
                CreateSupportTicketCommunication(randomTicketName, randomTicketCommunicationName, new CommunicationDetails()
                {
                    Sender = "abc@contoso.com",
                    Subject = "This is a test ticket",
                    Body = "This is a test ticket communication. Ticket can be closed without any work"
                });


                //  7. Close the support ticket
                UpdateSupportTicketStatus(randomTicketName, "closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }




        // Option 3: 
        //  1. Call services list api and find billing service name
        //  2. Call problem classification list api and find refund request problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Update Severity to moderate
        //  6. Update severity back to minimal and close the support ticket
        private static void ExecuteOption3()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach (var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("billing"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("refund request"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
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


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5. Update Severity to moderate
                UpdateSupportTicketSeverity(randomTicketName, "Moderate");


                //  6. Update severity back to minimal and close the support ticket
                UpdateSupportTicket(randomTicketName, new UpdateSupportTicket()
                {
                    Severity = "Minimal",
                    Status = "closed"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        // Option 4: 
        //  1. Call services list api and find subscription management service name
        //  2. Call problem classification list api and find cancel subscription problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Update additional email contact details
        //  6. Close the support ticket
        private static void ExecuteOption4()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach (var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("subscription management"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("cancel my subscription"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
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


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5.Update additional email contact details
                UpdateSupportTicketContact(randomTicketName, new UpdateContactProfile()
                {
                    AdditionalEmailAddresses = new List<string>() { "xyz@contoso.com" }
                });


                //  6. Close the support ticket
                UpdateSupportTicketStatus(randomTicketName, "closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        // Option 5: 
        //  1. Call services list api and find technical cosmos db service name
        //  2. Call problem classification list api and find throttling problem classification name
        //  3. Create random ticket name and call check name availability
        //  4. Create support ticket 
        //  5. Close the support ticket
        private static void ExecuteOption5()
        {
            try
            {


                //  1.Call services list api and find quota service name
                var rsp1 = GetServiceList();
                var serviceName = string.Empty;
                foreach (var service in rsp1)
                {
                    if (service.DisplayName.ToLower().Contains("cosmos db"))
                    {
                        serviceName = service.Name;
                        break;
                    }
                }


                //  2. Call problem classification list api and find cores problem classification name
                var rsp2 = GetProblemClassificationList(serviceName);
                var problemClassificationName = string.Empty;
                foreach (var problemClassification in rsp2)
                {
                    if (problemClassification.DisplayName.ToLower().Contains("throttling"))
                    {
                        problemClassificationName = problemClassification.Name;
                        break;
                    }
                }


                //  3.Create random ticket name and call check name availability until unique name is not found
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


                //  4. Create support ticket
                var inputPayload = GenerateCreateSupportTicketPayload();
                inputPayload.ServiceId = "/providers/Microsoft.Support/services/" + serviceName;
                inputPayload.ProblemClassificationId = "/providers/Microsoft.Support/services/" + serviceName + "/problemClassifications/" + problemClassificationName;
                CreateSupportTicket(randomTicketName, inputPayload);


                //  5. Close the support ticket
                UpdateSupportTicketStatus(randomTicketName, "closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        private static void UpdateSupportTicketSeverity(string supportTicketName, string severity)
        {
            UpdateSupportTicket(supportTicketName, new UpdateSupportTicket()
            {
                Severity = severity
            });
        }

        private static void UpdateSupportTicketStatus(string supportTicketName, string status)
        {
            UpdateSupportTicket(supportTicketName, new UpdateSupportTicket()
            {
                Status = status
            });
        }

        private static void UpdateSupportTicketContact(string supportTicketName, UpdateContactProfile contact)
        {
            UpdateSupportTicket(supportTicketName, new UpdateSupportTicket()
            {
                ContactDetails = contact
            });
        }

        private static void CreateSupportTicket(string supportTicketName, SupportTicketDetails createPayload)
        {
            try
            {
                var rsp = supportClient.SupportTickets.Create(supportTicketName, createPayload);
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        private static void UpdateSupportTicket(string supportTicketName, UpdateSupportTicket updatePayload)
        {
            try
            {
                var rsp = supportClient.SupportTickets.Update(supportTicketName, updatePayload);
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
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
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
                return false;
            }
        }

        private static void CreateSupportTicketCommunication(string supportTicketName, string communicationName, CommunicationDetails createCommunicationPayload)
        {
            try
            {
                var rsp = supportClient.Communications.Create(supportTicketName, communicationName, createCommunicationPayload);
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ERRORMSG);
                Console.WriteLine(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
        }

        private static List<Service> GetServiceList()
        {
            return supportClient.Services.List().ToList();
        }

        private static List<ProblemClassification> GetProblemClassificationList(string serviceName)
        {
            return supportClient.ProblemClassifications.List(serviceName).ToList();
        }

        private static SupportTicketDetails GenerateCreateSupportTicketPayload()
        {
            return new SupportTicketDetails()
            {
                Severity = "Minimal",
                Title = "Test ticket from Azure Support sample console app",
                Description = "Test ticket from Azure Support sample console app",
                ContactDetails = new ContactProfile()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    PrimaryEmailAddress = "abc@contoso.com",
                    PreferredContactMethod = "email",
                    PreferredTimeZone = "Pacific Standard Time",
                    PreferredSupportLanguage = "en-US",
                    Country = "usa"
                }
            };
        }
    }
}
