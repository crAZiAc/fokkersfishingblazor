using System.ComponentModel.DataAnnotations;

namespace FokkersFishing.Api.Models.Dtos
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }

    public class ExternalProviderInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string NewPassword { get; set; }
    }
}
