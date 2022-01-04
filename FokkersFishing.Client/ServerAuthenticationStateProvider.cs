using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using FokkersFishing.Shared.Models;
using System;

namespace FokkersFishing.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public ServerAuthenticationStateProvider(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("server");
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            var userInfo = await _httpClient.GetFromJsonAsync<UserInfo>("user");

            ClaimsIdentity identity;
            if (userInfo.IsAuthenticated)
            {
                identity = new ClaimsIdentity(new[]
                {   new Claim(ClaimTypes.Name, userInfo.Name),
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
                    new Claim(ClaimTypes.Email, userInfo.Email)},
                    userInfo.IdentityProvider);
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }


    } // end c
} // end ns