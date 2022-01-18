using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FokkersFishing.Shared.Models
{
    public class User
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        [JsonIgnore]
        public List<Role> Roles { get; set; }
        public Role[] RoleArray
        {
            get
            {
                return Roles.ToArray();
            }
        }

        public User()
        {
            Roles = new List<Role>();
        }
        public string RoleList
        {
            get
            {
                StringBuilder roleList = new StringBuilder();
                int roleCount = this.Roles.Where(r => r.IsInRole).Count();
                int roleNumber = 1;
                foreach (var role in this.Roles.Where(r => r.IsInRole))
                {
                    roleList.Append(role.Name);
                    if (roleNumber < roleCount)
                    {
                        roleList.Append(", ");
                    }
                    roleNumber++;
                }
                return roleList.ToString();
            }
        }
    } // end c
} // end ns
