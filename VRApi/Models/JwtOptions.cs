using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VRApi.Models
{
    public class JwtOptions
    {
        private string secretKey { get; set; }

        public SymmetricSecurityKey SymmetricSecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public JwtOptions(string secretKey, string issuer, string audience = null)
        {
            this.secretKey = secretKey;
            Issuer = issuer;
            Audience = audience;
        }

    }
}
