using System.ComponentModel.DataAnnotations;

namespace VRApi.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
