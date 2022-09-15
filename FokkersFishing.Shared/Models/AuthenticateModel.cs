using System.ComponentModel.DataAnnotations;

namespace FokkersFishing.Shared.Models
{
    public class AuthenticateModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Key { get; set; }
    }
}