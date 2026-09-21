using System;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using FokkersFishing.Api.Controllers;
using FokkersFishing.Api.Data;
using FokkersFishing.Api.Helpers;
using FokkersFishing.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Azure Key Vault (production only) - mirrors the original app's secret loading.
var keyVaultName = config["KeyVaultName"];
if (builder.Environment.IsProduction() && !string.IsNullOrEmpty(keyVaultName))
{
    config.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

// ---- Identity (SQLite) : reuses the original app.db schema so accounts keep working ----
var sqliteConnection = config["ConnectionStrings:DefaultConnection"] ?? "DataSource=app.db";
EnsureSqliteDirectoryExists(sqliteConnection);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(sqliteConnection));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Fresh SPA sign-in does not gate on email confirmation; existing users are already confirmed.
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---- Azure Table Storage ----
var storageConnectionString = config["Storage:ConnectionString"];
if (string.IsNullOrEmpty(storageConnectionString))
{
    throw new InvalidOperationException(
        "Storage:ConnectionString is not configured. Set it in user-secrets, appsettings, Key Vault, or as an App Service setting 'Storage__ConnectionString'.");
}
var tableServiceClient = new TableServiceClient(storageConnectionString);
builder.Services.AddSingleton<IFokkersDbService>(new FokkersDbService(tableServiceClient, storageConnectionString));

builder.Services.AddScoped<UserHelper>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddHttpContextAccessor();

// ---- Authentication : JWT bearer for the API, cookie for the external round-trip ----
var jwtKey = config["Jwt:Key"];
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrEmpty(config["Jwt:Issuer"]),
            ValidateAudience = !string.IsNullOrEmpty(config["Jwt:Audience"]),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(string.IsNullOrEmpty(jwtKey) ? Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N") : jwtKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
    })
    .AddCookie(AuthController.ExternalCookieScheme, options =>
    {
        options.Cookie.Name = "Fokkers.External";
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, BasicAuthenticationHandler>(
        AuthenticationSchemaNames.BasicAuthentication, _ => { });

// External providers (Facebook intentionally omitted).
if (!string.IsNullOrEmpty(config["Authentication:Google:ClientId"]))
{
    builder.Services.AddAuthentication().AddGoogle(o =>
    {
        o.ClientId = config["Authentication:Google:ClientId"];
        o.ClientSecret = config["Authentication:Google:ClientSecret"];
        o.SignInScheme = AuthController.ExternalCookieScheme;
        o.SaveTokens = true;
    });
}
if (!string.IsNullOrEmpty(config["Authentication:Microsoft:ClientId"]))
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(o =>
    {
        o.ClientId = config["Authentication:Microsoft:ClientId"];
        o.ClientSecret = config["Authentication:Microsoft:ClientSecret"];
        o.SignInScheme = AuthController.ExternalCookieScheme;
        o.SaveTokens = true;
    });
}

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ---- Seed roles (and ensure DB exists for a fresh install) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var roleName in new[] { "User", "Administrator" })
    {
        if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
        }
    }

    // Bootstrap admins: any email listed in Admin:SeedAdministratorEmails is granted the
    // Administrator role on startup (once the account exists). Safe to run every boot.
    var seedAdmins = config.GetSection("Admin:SeedAdministratorEmails").Get<string[]>() ?? System.Array.Empty<string>();
    if (seedAdmins.Length > 0)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var email in seedAdmins)
        {
            if (string.IsNullOrWhiteSpace(email)) continue;
            var adminUser = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
            if (adminUser != null && !userManager.IsInRoleAsync(adminUser, "Administrator").GetAwaiter().GetResult())
            {
                userManager.AddToRoleAsync(adminUser, "Administrator").GetAwaiter().GetResult();
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevCorsPolicy);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA fallback: any non-API, non-file route serves the React shell.
app.MapFallbackToFile("index.html");

app.Run();

// Ensures the folder for a SQLite DataSource exists (e.g. /home/data on App Service),
// since SQLite creates the file but not its parent directory.
static void EnsureSqliteDirectoryExists(string connectionString)
{
    try
    {
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:") return;
        var dir = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
    }
    catch
    {
        // Non-fatal: fall through and let EF surface a clear error if the path is truly unusable.
    }
}
