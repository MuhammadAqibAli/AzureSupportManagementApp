
using AzureSupportManagement.Interface;
using AzureSupportManagement.Models;
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
        public SubscriptionService(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public List<Subscription> GetSubscriptions()
        {
            List<Subscription> subscriptions = new List<Subscription>();
            try
            {                
                string token = _authenticationService.GetToken(false);
                HttpClient client = new HttpClient();                
                client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
                var subcription = client.GetStringAsync("https://management.azure.com/subscriptions?api-version=2014-04-01-preview").Result;
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
