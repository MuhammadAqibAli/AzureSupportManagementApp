using Microsoft.Rest;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AzureSupportManagement.Services
{
    public class CustomLoginCredentials : ServiceClientCredentials
    {
        // Generate auth token using armclient (https://github.com/projectkudu/ARMClient)
        // Or using your own custom authentication/authorization implementation.
        // You can use token acquired for a user or a service principal for the arm audience https://management.azure.com/ 
        private string AUTHTOKEN = "";

        public CustomLoginCredentials(string token)
        {
            AUTHTOKEN = token;
        }

        private string AuthenticationToken { get; set; }

        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            AuthenticationToken = AUTHTOKEN.Replace("\n", "").Replace("\r", "").Replace(" ", "");
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (AuthenticationToken == null)
            {
                throw new InvalidOperationException("Token Provider Cannot Be Null");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}
