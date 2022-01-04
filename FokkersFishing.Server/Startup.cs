using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using FokkersFishing.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FokkersFishing.Interfaces;
using System.Threading.Tasks;
using FokkersFishing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using System.Net.Mime;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Azure.Data.Tables;

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

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddMvc()
                .AddNewtonsoftJson();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { MediaTypeNames.Application.Octet });
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));

            ConfigurationSection section = Configuration.GetSection("Storage") as ConfigurationSection;
            section["ConnectionString"] = Configuration["Storage:ConnectionString"];

            services.AddSingleton<IFokkersDbService>(InitializeTableClientInstanceAsync(section).GetAwaiter().GetResult());

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
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
                  });
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
            app.UseStaticFiles();
            app.UseBlazorFrameworkFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
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

            FokkersDbService tableDbService = new FokkersDbService(serviceClient);
            return tableDbService;
        }
    } // end c
} // end ns
