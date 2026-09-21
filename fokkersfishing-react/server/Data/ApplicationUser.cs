using FokkersFishing.Api.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace FokkersFishing.Api.Data
{
    public class ApplicationUser : IdentityUser
    {
        public User GetUser()
        {
            return new User
            {
                Email = this.Email,
                UserName = this.UserName
            };
        }
    }
}
