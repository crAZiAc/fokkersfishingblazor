using MatBlazor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FokkersFishing.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            var urlState = new UrlState();
            builder.Configuration.Bind("UrlState", urlState);
            builder.Services.AddSingleton(urlState);
            builder.Services.AddHttpClient("server", client => client.BaseAddress = new Uri(urlState.BackendUrl));
            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            builder.Services.AddScoped<HttpClient>();

            builder.Services.AddMatBlazor();
            

            await builder
                .Build()
                .RunAsync();

        }
    } // end c
} // end ns

