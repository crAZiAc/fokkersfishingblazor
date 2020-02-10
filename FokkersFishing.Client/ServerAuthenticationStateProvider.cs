using System.Net.Http;
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

        public ServerAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfo = await _httpClient.GetJsonAsync<UserInfo>("user");

            ClaimsIdentity identity;
            if (userInfo.IsAuthenticated)
            {
                identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userInfo.Name), new Claim(ClaimTypes.NameIdentifier, userInfo.Id) }, userInfo.IdentityProvider);
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    } // end c
} // end ns