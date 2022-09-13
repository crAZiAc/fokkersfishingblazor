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

        public List<User> GetUsers()
        {
            List<User> userList = new List<User>();
            foreach (var user in _dbContext.Users)
            {
                User userToAdd = new User { UserName = user.UserName, Email = user.Email};
                foreach (var role in _dbContext.Roles)
                {
                    var userRole = _dbContext.UserRoles.FirstOrDefault(r => r.RoleId == role.Id & r.UserId == user.Id);
                    if (userRole != null)
                    {
                        userToAdd.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = true });
                    }
                    else
                    {
                        userToAdd.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = false }); ;
                    }
                }
                userList.Add(userToAdd);
            }
            return userList;
        } // end f

        public User GetUser(string userEmail)
        {
            ApplicationUser user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            if (user != null)
            {
                User userToReturn = user.GetUser();
                foreach (var role in _dbContext.Roles)
                {
                    var userRole = _dbContext.UserRoles.FirstOrDefault(r => r.RoleId == role.Id & r.UserId == user.Id);
                    if (userRole != null)
                    {
                        userToReturn.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = true });
                    }
                    else
                    {
                        userToReturn.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = false }); ;
                    }
                }
                return userToReturn;
            }
            else
            {
                return new User
                {
                    Email = "non-existent",
                    UserName = "Unknown/Deleted user"
                };
            }
        } // end f

        public User GetUser(Guid userId)
        {
            ApplicationUser user = _dbContext.Users.FirstOrDefault(c => c.Id == userId.ToString());
            if (user != null)
            {
                User userToReturn = user.GetUser();
                foreach (var role in _dbContext.Roles)
                {
                    var userRole = _dbContext.UserRoles.FirstOrDefault(r => r.RoleId == role.Id & r.UserId == user.Id);
                    if (userRole != null)
                    {
                        userToReturn.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = true });
                    }
                    else
                    {
                        userToReturn.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = false }); ;
                    }
                }
                return userToReturn;
            }
            else
            {
                return new User
                {
                    Email = "non-existent",
                    UserName = "Unknown/Deleted user"
                };
            }
        } // end f

        public User UpdateUser(User user)
        {
            ApplicationUser userToUpdate = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == user.Email.ToLower());
            if (userToUpdate != null)
            {
                userToUpdate.UserName = user.UserName;
                userToUpdate.EmailConfirmed = true;
                _dbContext.SaveChanges();

                foreach(var userRole in user.Roles)
                {
                    var role = _dbContext.Roles.FirstOrDefault(r => r.Name.ToLower() == userRole.Name.ToLower());
                    if (role != null)
                    {
                        // Check if role already there
                        var userRoleToCheck = _dbContext.UserRoles.FirstOrDefault(r => r.RoleId == role.Id & r.UserId == userToUpdate.Id);
                        if (userRoleToCheck != null)
                        {
                            // check if it needs to be there
                            if (!userRole.IsInRole)
                            {
                                // Should not be there, but is in collection, so remove it
                                _dbContext.UserRoles.Remove(userRoleToCheck);
                                _dbContext.SaveChanges();
                            }
                            
                        }   
                        else
                        {
                            // check if it needs to be there
                            if (userRole.IsInRole)
                            {
                                // Should be there, but is not in collection, so add
                                _dbContext.UserRoles.Add(new IdentityUserRole<string> { RoleId = role.Id, UserId = userToUpdate.Id });
                                _dbContext.SaveChanges();
                            }
                        }
                    }
                }
                User userToReturn = userToUpdate.GetUser();
                foreach (var role in _dbContext.Roles)
                {
                    var userRole = _dbContext.UserRoles.FirstOrDefault(r => r.RoleId == role.Id & r.UserId == userToUpdate.Id);
                    if (userRole != null)
                    {
                        userToReturn.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = true });
                    }
                    else
                    {
                        userToReturn.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = false }); ;
                    }
                }
                return userToReturn;
            }
            else
            {
                return null;
            }

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

        public List<Role> GetRoles()
        {
            List<Role> rolesToReturn = new List<Role>();
            List<IdentityRole> roles = _dbContext.Roles.ToList();
            if (roles != null )
            {
                foreach(var role in roles)
                {

                    rolesToReturn.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = false }); ;
                }
                return rolesToReturn;
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
                userToReturn.Roles.Where(r => r.Id == roleToAdd.Id & r.Name == roleToAdd.Name).FirstOrDefault().IsInRole = true;
            }
            return userToReturn;
        } // end f

        public User RemoveRoleFromUser(string userEmail, string role)
        {
            User userToReturn = null;
            ApplicationUser user = null;
            user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            IdentityRole roleToRemove = _dbContext.Roles.FirstOrDefault(r => r.Name.ToLower() == role.ToLower());
            if (roleToRemove != null & user != null)
            {
                userToReturn = user.GetUser();
                _dbContext.UserRoles.Remove(new IdentityUserRole<string> { RoleId = roleToRemove.Id, UserId = user.Id });
                _dbContext.SaveChanges();
                userToReturn.Roles.Where(r => r.Id == roleToRemove.Id & r.Name == roleToRemove.Name).FirstOrDefault().IsInRole = false;
                return userToReturn;
            }
            else
            {
                return null;
            }
        } // end f

    } // end c
} // end ns
