using FokkersFishing.Helpers;
using FokkersFishing.Shared.Models;
using Microsoft.AspNetCore.Authentication;

namespace FokkersFishing.Server.Helpers
{
    public static class DependencyInjectionExtensions
    {
        public static AuthenticationBuilder AddBasicAuthenticationSchema(this AuthenticationBuilder authentication)
        {
            authentication.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationSchemaNames.BasicAuthentication, o => { });
            return authentication;
        }
    } // end c
} // end ns
