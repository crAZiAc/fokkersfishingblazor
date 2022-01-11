using FokkersFishing.Data;
using FokkersFishing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FokkersFishing.Server.Helpers
{
    public class UserHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        public UserHelper(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }
        public ApplicationUser GetUser()
        {
            ApplicationUser user = null;
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userId != null)
            {
                // Lookup 
                user = _dbContext.Users.FirstOrDefault(c => c.Id == userId);
                if (user == null)
                {
                    user = new ApplicationUser()
                    {
                        Email = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value,
                        UserName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value,
                        Id = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value
                    };

                    _dbContext.Add(user);
                    _dbContext.SaveChangesAsync();
                }
            }
            return user;
        } // end f


      

        public List<ApplicationUser> GetUsers()
        {
            List<ApplicationUser> userList = new List<ApplicationUser>();
            foreach(var user in _dbContext.Users)
            {
                userList.Add(user);
            }
            return userList;
        } // end f

    } // end c
} // end ns
