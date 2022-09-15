using System.ComponentModel.DataAnnotations;

namespace FokkersFishing.Models
{
    public class ApiAuthenticateUser
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Key { get; set; }
    }
}