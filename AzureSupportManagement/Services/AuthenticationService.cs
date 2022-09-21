
using AzureSupportManagement.Interface;
using AzureSupportManagement.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace AzureSupportManagement.Services
{
    public class AuthenticationService: IAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthenticationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private static string m_resource = "https://management.core.windows.net/";
        private static string m_clientId = "1950a258-227b-4e31-a9cf-717495945fc2"; // well-known client ID for Azure PowerShell
        private static string m_redirectURI = "urn:ietf:wg:oauth:2.0:oob"; // redirect URI for Azure PowerShell
        public string GetToken(bool forceLogin = false)
        {
            try
            {
                var sessionToken = _httpContextAccessor.HttpContext.Session.GetString("token");
                if (string.IsNullOrEmpty(sessionToken) || forceLogin)
                {
                    string resource = "https://management.core.windows.net/";
                    string clientId = "1950a258-227b-4e31-a9cf-717495945fc2";
                    string userName = "supportuser@riniehuijgenhotmail.onmicrosoft.com";
                    string password = "dfgbnjlk54y6HGFR@";

                    HttpClient client = new HttpClient();
                    string tokenEndpoint = "https://login.microsoftonline.com/common/oauth2/token";
                    var body = $"resource={resource}&client_id={clientId}&grant_type=password&username={userName}&password={password}";
                    var stringContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

                    var result = client.PostAsync(tokenEndpoint, stringContent).ContinueWith<string>((response) =>
                    {
                        return response.Result.Content.ReadAsStringAsync().Result;
                    }).Result;

                    JObject jobject = JObject.Parse(result);
                    var token = jobject["access_token"].Value<string>();
                    _httpContextAccessor.HttpContext.Session.SetString("token", token);
                    return token;
                }
                return sessionToken;



            }
            catch (Exception ex)
            {
                var e = ex;
                return null;
            }

        }
    }
}
