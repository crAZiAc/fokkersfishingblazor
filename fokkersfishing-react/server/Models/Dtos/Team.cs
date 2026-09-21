using System;
using System.Collections.Generic;
using System.Linq;

namespace FokkersFishing.Api.Models.Dtos
{
    public class Team
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<User> Users { get; set; } = new List<User>();

        public List<User> UsersNotInTeam(List<User> userList)
        {
            var usersNotInTeam = new List<User>();
            foreach (var user in userList)
            {
                var inTeam = this.Users.Any(u => u.Email == user.Email);
                if (!inTeam) usersNotInTeam.Add(user);
            }
            return usersNotInTeam;
        }
    }
}
