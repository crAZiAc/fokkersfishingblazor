using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace FokkersFishing.Api.Models.Dtos
{
    public class Role
    {
        public string Name { get; set; }
        public bool IsInRole { get; set; }
        public string Id { get; set; }
    }

    public class User
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public string LoginProvider { get; set; }

        public Role[] RoleArray => Roles.ToArray();

        public string RoleList
        {
            get
            {
                var roleList = new StringBuilder();
                int roleCount = Roles.Count(r => r.IsInRole);
                int roleNumber = 1;
                foreach (var role in Roles.Where(r => r.IsInRole))
                {
                    roleList.Append(role.Name);
                    if (roleNumber < roleCount) roleList.Append(", ");
                    roleNumber++;
                }
                return roleList.ToString();
            }
        }

        public List<User> UsersNotInTeam(List<User> userList)
        {
            var usersNotInTeam = new List<User>();
            foreach (var user in userList)
            {
                if (!Roles.Any() && user.Email == this.Email) continue;
                usersNotInTeam.Add(user);
            }
            return usersNotInTeam;
        }
    }

    public class UserInfo
    {
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string IdentityProvider { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; } = new string[0];
    }
}
