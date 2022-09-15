using FokkersFishing.Server.Interfaces;
using FokkersFishing.Shared.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Services
{
    public class UserService : IUserService
    {

        private string m_ApiUser;
        private string m_ApiKey;

        public UserService(string apiUser, string apiKey)
        {
            m_ApiUser = apiUser;
            m_ApiKey = apiKey;
        }
        public async Task<bool> Authenticate(string username, string key)
        {
            if (username == m_ApiUser & key == m_ApiKey)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

    } // end c
} // end ns