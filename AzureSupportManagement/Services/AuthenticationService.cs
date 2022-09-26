
using AzureSupportManagement.Interface;
using AzureSupportManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        public IConfiguration _configuration;
        public AuthenticationService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public string GetToken(bool forceLogin = false)
        {
            try
            {
                var sessionToken = _httpContextAccessor.HttpContext.Session.GetString("token");
                if (string.IsNullOrEmpty(sessionToken) || forceLogin)
                {                    
                    string resource = _configuration.GetValue<string>("Resource");
                    string clientId = _configuration.GetValue<string>("ClientId");
                    string userName = _configuration.GetValue<string>("UserEmail");
                    string password = _configuration.GetValue<string>("Password");

                    HttpClient client = new HttpClient();
                    string tokenEndpoint = _configuration.GetValue<string>("TokenEndpoint");
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
                return null;
            }

        }
    }
}
