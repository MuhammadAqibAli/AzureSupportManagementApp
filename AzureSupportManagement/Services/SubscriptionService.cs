
using AzureSupportManagement.Interface;
using AzureSupportManagement.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AzureSupportManagement.Services
{
    public class SubscriptionService: ISubscriptionService
    {
        private readonly IAuthenticationService _authenticationService;
        public IConfiguration _configuration;

        public SubscriptionService(IAuthenticationService authenticationService, IConfiguration configuration)
        {
            _authenticationService = authenticationService;
            _configuration = configuration;
        }

        public List<Subscription> GetSubscriptions()
        {
            List<Subscription> subscriptions = new List<Subscription>();
            try
            {                
                string token = _authenticationService.GetToken(false);
                HttpClient client = new HttpClient();                
                client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
                string subscriptionEndpoint=_configuration.GetValue<string>("SubscriptionEndpoint");
                var subcription = client.GetStringAsync(subscriptionEndpoint).Result;
                var v=JObject.Parse(subcription)["value"];
                subscriptions = JsonConvert.DeserializeObject<List<Subscription>>(v.ToString());                
            }
            catch (Exception ex)
            {                             
            }
            return subscriptions;
        }       
    }
}
