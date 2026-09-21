using System.Collections.Generic;
using System.Threading.Tasks;
using FokkersFishing.Api.Helpers;
using FokkersFishing.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FokkersFishing.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminUserController : ControllerBase
    {
        private readonly UserHelper _userHelper;

        public AdminUserController(UserHelper userHelper)
        {
            _userHelper = userHelper;
        }

        [Authorize(Roles = "Administrator, User")]
        [HttpGet("users")]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            var users = _userHelper.GetUsers();
            if (users == null) return NotFound();
            return users;
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("roles")]
        public ActionResult<IEnumerable<Role>> GetRoles()
        {
            var roles = _userHelper.GetRoles();
            if (roles == null) return NotFound();
            return roles;
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("users/{userEmail}")]
        public ActionResult<User> GetUserByEmail(string userEmail)
        {
            return _userHelper.GetUser(userEmail);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("users")]
        public ActionResult<User> UpdateUser(User user)
        {
            return _userHelper.UpdateUser(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("roles/{userEmail}/{roleName}")]
        public ActionResult<User> AddUserToRole(string userEmail, string roleName)
        {
            var user = _userHelper.AddUserToRole(userEmail, roleName);
            if (user == null) return BadRequest();
            return CreatedAtAction(nameof(AddUserToRole), user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("users/{userEmail}/password")]
        public ActionResult<User> ResetPassword(string userEmail, ResetPasswordRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "A new password is required." });
            }
            try
            {
                var user = _userHelper.ChangePassword(userEmail, request.NewPassword);
                if (user == null) return NotFound();
                return user;
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("users/{userEmail}")]
        public ActionResult<bool> DeleteUser(string userEmail)
        {
            return _userHelper.DeleteUser(userEmail);
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("roles/{userEmail}/{roleName}")]
        public ActionResult<User> DeleteRoleFromUser(string userEmail, string roleName)
        {
            return _userHelper.RemoveRoleFromUser(userEmail, roleName);
        }
    }
}
