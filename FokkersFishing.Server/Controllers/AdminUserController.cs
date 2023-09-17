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

    [ApiController]
    [Route("[controller]")]
    public class AdminUserController : Controller
    {
        private readonly ILogger<AdminUserController> _logger;
        private readonly IFokkersDbService _fokkersDbService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private UserHelper _userHelper;

        public AdminUserController(ILogger<AdminUserController> logger, IFokkersDbService fokkersDbService, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _fokkersDbService = fokkersDbService;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _userHelper = new UserHelper(_httpContextAccessor, _dbContext);
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            List<User> usersToReturn = new List<User>();
            usersToReturn = _userHelper.GetUsers();
            if (usersToReturn == null)
            {
                return NotFound();
            }
            else
            {
                return usersToReturn.ToList();
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("roles")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            List<Role> rolesToReturn = _userHelper.GetRoles();
            if (rolesToReturn == null)
            {
                return NotFound();
            }
            else
            {
                return rolesToReturn.ToList();
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("users/{userEmail}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUserByEmail(string userEmail)
        {
            User userToReturn = _userHelper.GetUser(userEmail);
            return userToReturn;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("users")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> UpdateUser(User user)
        {
            User userUpdated = _userHelper.UpdateUser(user);
            if (userUpdated != null)
            {
                return userUpdated;
            }
            else
            {
                return null;
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("roles/{userEmail}/{roleName}")]
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

        [Authorize(Roles = "Administrator")]
        [HttpDelete("users/{userEmail}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> DeleteUser(string userEmail)
        {
            bool result = _userHelper.DeleteUser(userEmail);
            return result;
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("roles/{userEmail}/{roleName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> DeleteRoleFromUser(string userEmail, string roleName)
        {
            User result = _userHelper.RemoveRoleFromUser(userEmail, roleName);
            return result;
        }
    } // end c
} // end ns
