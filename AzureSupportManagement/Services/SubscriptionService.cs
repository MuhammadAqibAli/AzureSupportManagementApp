
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

        private static string m_resource = "https://management.core.windows.net/";
        private static string m_clientId = "1950a258-227b-4e31-a9cf-717495945fc2"; // well-known client ID for Azure PowerShell
        private static string m_redirectURI = "urn:ietf:wg:oauth:2.0:oob"; // redirect URI for Azure PowerShell
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
                var e = ex;                
            }
            return subscriptions;

        }
        //    try
        //{
        //        var ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
        //        // This will show the login window
        //        var mainAuthRes = ctx.AcquireToken(m_resource, m_clientId, new Uri(m_redirectURI), PromptBehavior.Always);
        //        var subscriptionCredentials = new TokenCloudCredentials(mainAuthRes.AccessToken);
        //        var cancelToken = new CancellationToken();
        //        using (var subscriptionClient = new SubscriptionClient(subscriptionCredentials))
        //        {
        //            var tenants = subscriptionClient.Tenants.ListAsync(cancelToken).Result;
        //            foreach (var tenantDescription in tenants.TenantIds)
        //            {
        //                var tenantCtx = new AuthenticationContext("https://login.microsoftonline.com/" + tenantDescription.TenantId);
        //                // This will NOT show the login window
        //                var tenantAuthRes = tenantCtx.AcquireToken(
        //                    m_resource,
        //                    m_clientId,
        //                    new Uri(m_redirectURI),
        //                    PromptBehavior.Never,
        //                    new UserIdentifier(mainAuthRes.UserInfo.DisplayableId, UserIdentifierType.RequiredDisplayableId));
        //                var tenantTokenCreds = new TokenCloudCredentials(tenantAuthRes.AccessToken);
        //                using (var tenantSubscriptionClient = new SubscriptionClient(tenantTokenCreds))
        //                {
        //                    var tenantSubscriptioins = tenantSubscriptionClient.Subscriptions.ListAsync(cancelToken).Result;
        //                    foreach (var sub in tenantSubscriptioins.Subscriptions)
        //                    {
        //                        Console.WriteLine($"SubscriptionName : {sub.DisplayName}");
        //                        Console.WriteLine($"SubscriptionId   : {sub.SubscriptionId}");
        //                        Console.WriteLine($"TenantId         : {tenantDescription.TenantId}");
        //                        Console.WriteLine($"State            : {sub.State}");
        //                        Console.WriteLine();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.ToString());
        //}
        //finally
        //{
        //    Console.WriteLine("press something");
        //    Console.ReadLine();
        //}
        //}


    }
}
