using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FokkersFishing.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FokkersFishing.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using FokkersFishing.Data;
using FokkersFishing.Shared.Models;
using FokkersFishing.Server.Helpers;
using Microsoft.AspNetCore.Identity;

namespace FokkersFishing.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly ILogger<CatchController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private UserHelper _userHelper;

        public AdminController(ILogger<CatchController> logger, IFokkersDbService fokkersDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = fokkersDbService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
        }

        [HttpGet("/users")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            List<User> usersToReturn = new List<User>();
            IEnumerable<ApplicationUser> users = null;
            users = _userHelper.GetUsers();
            if (users == null)
            {
                return NotFound();
            }
            else
            {
                foreach (var user in users)
                {
                    User userToReturn = user.GetUser();
                    var roles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id);
                    foreach (var userRole in roles)
                    {
                        var role = _dbContext.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                        userToReturn.Roles.Add(role);
                    }
                    usersToReturn.Add(userToReturn);
                }
            }
            return usersToReturn.ToList();
        }

        [HttpGet("/roles")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<IdentityRole>>> GetRoles()
        {
            var roles = _userHelper.GetRoles();
            if (roles == null)
            {
                return NotFound();
            }
            return roles.ToList();
        }

        [HttpGet("/users/{userEmail}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUserByEmail(string userEmail)
        {
            ApplicationUser user = _userHelper.GetUser(userEmail);
            User userToReturn = user.GetUser();
            var roles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id);
            foreach (var userRole in roles)
            {
                var role = _dbContext.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                userToReturn.Roles.Add(role);
            }
            return userToReturn;
        }


        [HttpPost("/roles/{userEmail}/{roleName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> AddUserToRole(string userEmail, string roleName)
        {
            User user = _userHelper.AddUserToRole(userEmail, roleName);
            if (user != null)
            {
                return CreatedAtAction(nameof(AddUserToRole), user);
            }
            else
            {
                return null;
            }
        }

        [HttpDelete("/users/{userEmail}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteUsers(string userEmail)
        {
            bool result = _userHelper.DeleteUser(userEmail);
            return result;
        }

        [HttpDelete("/roles/{userEmail}/{roleName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> DeleteRoleFromUser(string userEmail, string roleName)
        {
            bool result = _userHelper.RemoveRoleFromUser(userEmail, roleName);
            return result;
        }




    } // end c
} // end ns
