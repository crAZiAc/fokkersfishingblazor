using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace FokkersFishing.Shared.Models
{
    public class User
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<IdentityRole<string>> Roles { get; set; }

        public User()
        {
            Roles = new List<IdentityRole<string>>();
        }
        public string RoleList
        {
            get
            {
                StringBuilder roleList = new StringBuilder();
                foreach (var role in Roles)
                {
                    roleList.Append(role.Name);
                    roleList.Append(",");
                }
                return roleList.ToString();
            }
        }
    } // end c
} // end ns
