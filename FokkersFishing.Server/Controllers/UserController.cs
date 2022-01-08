using FokkersFishing.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FokkersFishing.Server
{
    [ApiController]
    public class UserController : Controller
    {
        private static UserInfo LoggedOutUser = new UserInfo { IsAuthenticated = false };

        [HttpGet("user")]
        public UserInfo GetUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                UserInfo userInfo = new UserInfo() 
                {
                    Name = User.Identity.Name, 
                    IsAuthenticated = true, 
                    IdentityProvider = User.Identity.AuthenticationType.ToString(), 
                    Id = User.FindFirst(ClaimTypes.NameIdentifier).Value, 
                    Email = User.FindFirst(ClaimTypes.Email).Value
                };
                return userInfo;
            }
            else
            {
                return LoggedOutUser;
            }
        }


        [HttpGet("user/signin")]
        public async Task SignIn(string redirectUri, [FromQuery] string idp)
        {
            if (string.IsNullOrEmpty(redirectUri) || !Url.IsLocalUrl(redirectUri))
            {
                redirectUri = "/";
            }
            switch (idp.ToLower())
            {
                case "facebook":
                    await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectUri });
                    break;
                case "google":
                    await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectUri });
                    break;
                case "microsoft":
                    await HttpContext.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = redirectUri });
                    break;
            }
        } // end f

        [HttpGet("user/signout")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/");
        }
    }
}
