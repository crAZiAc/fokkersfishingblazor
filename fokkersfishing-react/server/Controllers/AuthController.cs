using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FokkersFishing.Api.Data;
using FokkersFishing.Api.Helpers;
using FokkersFishing.Api.Models.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FokkersFishing.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        public const string ExternalCookieScheme = "External";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenService _jwt;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenService jwt,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwt = jwt;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
            {
                return BadRequest(new { message = "A user with that email already exists." });
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.UserName,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });
            }

            await _userManager.AddToRoleAsync(user, "User");
            return await BuildAuthResponse(user, "Local");
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var check = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!check.Succeeded)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            return await BuildAuthResponse(user, "Local");
        }

        [Authorize]
        [HttpGet("user")]
        public ActionResult<UserInfo> GetCurrentUser()
        {
            return new UserInfo
            {
                IsAuthenticated = true,
                Name = User.Identity?.Name,
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                IdentityProvider = User.Identity?.AuthenticationType,
                Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToArray()
            };
        }

        [HttpGet("providers")]
        public ActionResult<IEnumerable<ExternalProviderInfo>> GetProviders()
        {
            var providers = new List<ExternalProviderInfo>();
            if (!string.IsNullOrEmpty(_config["Authentication:Google:ClientId"]))
                providers.Add(new ExternalProviderInfo { Name = GoogleDefaults.AuthenticationScheme, DisplayName = "Google" });
            if (!string.IsNullOrEmpty(_config["Authentication:Microsoft:ClientId"]))
                providers.Add(new ExternalProviderInfo { Name = MicrosoftAccountDefaults.AuthenticationScheme, DisplayName = "Microsoft" });
            return providers;
        }

        /// <summary>
        /// Starts an external login. The browser is redirected to the provider and,
        /// after consent, back to <see cref="ExternalCallback"/>.
        /// </summary>
        [HttpGet("external/{provider}")]
        public IActionResult ExternalLogin(string provider, [FromQuery] string returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(ExternalCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external/callback")]
        public async Task<IActionResult> ExternalCallback([FromQuery] string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(ExternalCookieScheme);
            if (!result.Succeeded)
            {
                return Redirect(BuildSpaRedirect(returnUrl, error: "external_failed"));
            }

            var principal = result.Principal;
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? email;
            var providerName = result.Ticket?.AuthenticationScheme ?? "External";
            var providerKey = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Redirect(BuildSpaRedirect(returnUrl, error: "no_email"));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { Email = email, UserName = name, EmailConfirmed = true };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Redirect(BuildSpaRedirect(returnUrl, error: "create_failed"));
                }
                await _userManager.AddToRoleAsync(user, "User");
            }

            // Link the external login so the account shows its provider.
            var logins = await _userManager.GetLoginsAsync(user);
            if (providerKey != null && !logins.Any(l => l.LoginProvider == providerName))
            {
                await _userManager.AddLoginAsync(user, new UserLoginInfo(providerName, providerKey, providerName));
            }

            await HttpContext.SignOutAsync(ExternalCookieScheme);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwt.CreateToken(user, roles);
            return Redirect(BuildSpaRedirect(returnUrl, token: token));
        }

        private async Task<ActionResult<AuthResponse>> BuildAuthResponse(ApplicationUser user, string provider)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwt.CreateToken(user, roles);
            return new AuthResponse
            {
                Token = token,
                User = new UserInfo
                {
                    IsAuthenticated = true,
                    Name = user.UserName,
                    Id = user.Id,
                    Email = user.Email,
                    IdentityProvider = provider,
                    Roles = roles.ToArray()
                }
            };
        }

        private string BuildSpaRedirect(string returnUrl, string token = null, string error = null)
        {
            if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith("/"))
            {
                returnUrl = "/";
            }
            // The SPA reads token/error from the hash on the auth-callback route.
            var target = "/auth-callback";
            var fragment = token != null ? $"#token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(returnUrl)}"
                                          : $"#error={Uri.EscapeDataString(error ?? "unknown")}";
            return target + fragment;
        }
    }
}
