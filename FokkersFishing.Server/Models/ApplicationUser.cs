using FokkersFishing.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FokkersFishing.Models
{
    public class ApplicationUser : IdentityUser
    {
        public User GetUser()
        {
            User user = new User();
            user.Email = this.Email;
            user.UserName = this.UserName;
            return user;
        }
    } // end c
} // end ns
