using FokkersFishing.Data;
using FokkersFishing.Models;
using FokkersFishing.Shared.Models;
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
            foreach (var user in _dbContext.Users)
            {
                userList.Add(user);
            }
            return userList;
        } // end f

        public ApplicationUser GetUser(string userEmail)
        {
            ApplicationUser user = null;
            user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            return user;
        } // end f

        public bool DeleteUser(string userEmail)
        {
            ApplicationUser user = null;
            user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        } // end f

        public List<IdentityRole> GetRoles()
        {
            List<IdentityRole> roles = _dbContext.Roles.ToList();
            if (roles != null )
            {
                return roles;
            }
            return null;
        } // end f

        public User AddUserToRole(string userEmail, string role)
        {
            User userToReturn = null;
            ApplicationUser user = null;
            user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            IdentityRole roleToAdd = _dbContext.Roles.FirstOrDefault(r => r.Name.ToLower() == role.ToLower());
            if (roleToAdd != null & user != null)
            {
                userToReturn = user.GetUser();
                _dbContext.UserRoles.Add(new IdentityUserRole<string> { RoleId = roleToAdd.Id, UserId = user.Id });
                _dbContext.SaveChanges();
                userToReturn.Roles.Add(roleToAdd);
            }
            return userToReturn;
        } // end f

        public bool RemoveRoleFromUser(string userEmail, string role)
        {
            ApplicationUser user = null;
            user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            IdentityRole roleToRemove = _dbContext.Roles.FirstOrDefault(r => r.Name.ToLower() == role.ToLower());
            if (roleToRemove != null & user != null)
            {
                _dbContext.UserRoles.Remove(new IdentityUserRole<string> { RoleId = roleToRemove.Id, UserId = user.Id });
                _dbContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        } // end f

    } // end c
} // end ns
