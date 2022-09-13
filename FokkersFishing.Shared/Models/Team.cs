using Azure.Data.Tables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Shared.Models
{
    public class Team
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "users")]
        public List<User> Users { get; set; }

        public Team()
        {
            Users = new List<User>();
        }

        public List<User> UsersNotInTeam(List<User> userList)
        {
            List<User> usersNotInTeam = new List<User>();
            foreach (User user in userList)
            {
                var unselected = from u in this.Users
                                 where u.Email == user.Email
                                 select u;
                if (unselected.Count()> 0)
                {
                    // In team, do not add
                }
                else
                {
                    usersNotInTeam.Add(user);
                }
            }
            return usersNotInTeam;
        }
    } // end c
} // end ns
