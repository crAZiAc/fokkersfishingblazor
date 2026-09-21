using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FokkersFishing.Api.Data;
using FokkersFishing.Api.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FokkersFishing.Api.Helpers
{
    /// <summary>
    /// Maps between Identity users (SQLite) and the app's User DTO, resolves the
    /// current caller from JWT claims, and performs role/user administration.
    /// Ported from the original server UserHelper.
    /// </summary>
    public class UserHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserHelper(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public ApplicationUser GetUser()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return null;

            var user = _dbContext.Users.FirstOrDefault(c => c.Id == userId);
            if (user == null)
            {
                // Externally-authenticated user seen for the first time: persist a shell record.
                user = new ApplicationUser
                {
                    Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                    UserName = principal.FindFirst(ClaimTypes.Name)?.Value,
                    Id = userId
                };
                _dbContext.Add(user);
                _dbContext.SaveChanges();
            }
            return user;
        }

        public List<User> GetUsers()
        {
            var userList = new List<User>();
            var dbUsers = from d in _dbContext.Users
                          orderby d.UserName, d.Email
                          select d;
            var roles = _dbContext.Roles.ToList();
            foreach (var user in dbUsers)
            {
                var userToAdd = new User { UserName = user.UserName, Email = user.Email };
                foreach (var role in roles)
                {
                    var inRole = _dbContext.UserRoles.Any(r => r.RoleId == role.Id && r.UserId == user.Id);
                    userToAdd.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = inRole });
                }
                userToAdd.LoginProvider = GetLoginProvider(user.Id);
                userList.Add(userToAdd);
            }
            return userList;
        }

        public User GetUser(string userEmail)
        {
            var user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            if (user == null)
            {
                return new User { Email = "non-existent", UserName = "Unknown/Deleted user" };
            }
            return BuildUser(user);
        }

        public User GetUser(Guid userId)
        {
            var user = _dbContext.Users.FirstOrDefault(c => c.Id == userId.ToString());
            if (user == null)
            {
                return new User { Email = "non-existent", UserName = "Unknown/Deleted user" };
            }
            return BuildUser(user);
        }

        private User BuildUser(ApplicationUser user)
        {
            var userToReturn = user.GetUser();
            foreach (var role in _dbContext.Roles.ToList())
            {
                var inRole = _dbContext.UserRoles.Any(r => r.RoleId == role.Id && r.UserId == user.Id);
                userToReturn.Roles.Add(new Role { Id = role.Id, Name = role.Name, IsInRole = inRole });
            }
            userToReturn.LoginProvider = GetLoginProvider(user.Id);
            return userToReturn;
        }

        private string GetLoginProvider(string userId)
        {
            var login = _dbContext.UserLogins.FirstOrDefault(l => l.UserId == userId);
            return login != null ? login.LoginProvider : "Local Account";
        }

        public User ChangePassword(string userEmail, string newPassword)
        {
            var user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            if (user == null) return null;

            var token = _userManager.GeneratePasswordResetTokenAsync(user).GetAwaiter().GetResult();
            var result = _userManager.ResetPasswordAsync(user, token, newPassword).GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }
            return user.GetUser();
        }

        public User UpdateUser(User user)
        {
            var userToUpdate = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == user.Email.ToLower());
            if (userToUpdate == null) return null;

            userToUpdate.UserName = user.UserName;
            userToUpdate.EmailConfirmed = true;
            _dbContext.SaveChanges();

            foreach (var userRole in user.Roles)
            {
                var role = _dbContext.Roles.FirstOrDefault(r => r.Name.ToLower() == userRole.Name.ToLower());
                if (role == null) continue;

                var existing = _dbContext.UserRoles.FirstOrDefault(r => r.RoleId == role.Id && r.UserId == userToUpdate.Id);
                if (existing != null)
                {
                    if (!userRole.IsInRole)
                    {
                        _dbContext.UserRoles.Remove(existing);
                        _dbContext.SaveChanges();
                    }
                }
                else
                {
                    if (userRole.IsInRole)
                    {
                        _dbContext.UserRoles.Add(new IdentityUserRole<string> { RoleId = role.Id, UserId = userToUpdate.Id });
                        _dbContext.SaveChanges();
                    }
                }
            }
            return BuildUser(userToUpdate);
        }

        public bool DeleteUser(string userEmail)
        {
            var user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            if (user == null) return false;
            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
            return true;
        }

        public List<Role> GetRoles()
        {
            return _dbContext.Roles
                .ToList()
                .Select(role => new Role { Id = role.Id, Name = role.Name, IsInRole = false })
                .ToList();
        }

        public User AddUserToRole(string userEmail, string role)
        {
            var user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            var roleToAdd = _dbContext.Roles.FirstOrDefault(r => r.Name.ToLower() == role.ToLower());
            if (roleToAdd == null || user == null) return null;

            if (!_dbContext.UserRoles.Any(r => r.RoleId == roleToAdd.Id && r.UserId == user.Id))
            {
                _dbContext.UserRoles.Add(new IdentityUserRole<string> { RoleId = roleToAdd.Id, UserId = user.Id });
                _dbContext.SaveChanges();
            }
            return BuildUser(user);
        }

        public User RemoveRoleFromUser(string userEmail, string role)
        {
            var user = _dbContext.Users.FirstOrDefault(c => c.Email.ToLower() == userEmail.ToLower());
            var roleToRemove = _dbContext.Roles.FirstOrDefault(r => r.Name.ToLower() == role.ToLower());
            if (roleToRemove == null || user == null) return null;

            var existing = _dbContext.UserRoles.FirstOrDefault(r => r.RoleId == roleToRemove.Id && r.UserId == user.Id);
            if (existing != null)
            {
                _dbContext.UserRoles.Remove(existing);
                _dbContext.SaveChanges();
            }
            return BuildUser(user);
        }
    }
}
