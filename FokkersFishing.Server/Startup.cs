using Azure.Data.Tables;
using FokkersFishing.Data;
using FokkersFishing.Helpers;
using FokkersFishing.Interfaces;
using FokkersFishing.Models;
using FokkersFishing.Server.Helpers;
using FokkersFishing.Server.Interfaces;
using FokkersFishing.Services;
using FokkersFishing.Shared.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace FokkersFishing
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { MediaTypeNames.Application.Octet });
            });

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlite(
                   Configuration["ConnectionStrings:DefaultConnection"]));

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
            })
                .AddRoles<IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            ConfigurationSection section = Configuration.GetSection("Storage") as ConfigurationSection;
            section["ConnectionString"] = Configuration["Storage:ConnectionString"];

            services.AddSingleton<IFokkersDbService>(InitializeTableClientInstanceAsync(section).GetAwaiter().GetResult());

            services.AddIdentityServer(options =>
            {
                options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
            })
           .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(opt =>
           {
               opt.IdentityResources["openid"].UserClaims.Add("role");
               opt.ApiResources.Single().UserClaims.Add("role");
               opt.Clients.First().AccessTokenLifetime = 28800;
           });
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddIdentityServerJwt()
                .AddCookie()
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    facebookOptions.Fields.Add("name");
                    facebookOptions.Fields.Add("email");
                    facebookOptions.SaveTokens = true;
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                    microsoftOptions.SaveTokens = true;
                })
                  .AddGoogle(googleOptions =>
                  {
                      googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                      googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                      googleOptions.SaveTokens = true;
                  })
                  .AddBasicAuthenticationSchema();

            if (!string.IsNullOrEmpty(Configuration["IdentityServer:IssuerUri"]))
            {
                services.Configure<JwtBearerOptions>(IdentityServerJwtConstants.IdentityServerJwtBearerScheme, o => o.Authority = Configuration["IdentityServer:IssuerUri"]);
            }

            // configure DI for application services
            services.AddScoped<IUserService>(us => new UserService(Configuration["Api:User"], Configuration["Api:Key"]));

            services.AddControllersWithViews();
            services.AddRazorPages();



        } //end f

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        /// <summary>
        /// Creates a an Azure Table 
        /// </summary>
        /// <returns></returns>
        private static async Task<FokkersDbService> InitializeTableClientInstanceAsync(IConfigurationSection configurationSection)
        {
            string connectionString = configurationSection.GetSection("ConnectionString").Value;
            string accountName = configurationSection.GetSection("AccountName").Value;
            var serviceClient = new TableServiceClient(connectionString);

            FokkersDbService tableDbService = new FokkersDbService(serviceClient, connectionString);
            return tableDbService;
        }
    } // end c
} // end ns
