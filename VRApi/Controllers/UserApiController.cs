using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VRApi.Models;

namespace VRApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "user, admin")]
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<User> _userManager;

        private readonly ILogger<UserApiController> _logger;

        private readonly JwtOptions _jwtOptions;

        public UserApiController(RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager, 
            JwtOptions jwtOptions,
            ILogger<UserApiController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
            _jwtOptions = jwtOptions;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return Unauthorized(new
                {
                    error = "user not found"
                });
            var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isValid)
                return Unauthorized(new
                {
                    error = "invalid password"
                });
            var claims = await CreateClaims(user);

            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                expires: DateTime.MaxValue,
                audience: _jwtOptions.Audience,
                claims: claims,
                signingCredentials: new SigningCredentials(_jwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256));
            return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
        }

        [HttpGet("getme")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            var roles = await _userManager.GetRolesAsync(user);
            var response = new
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles,
            };
            return Ok(response);
        }

        private async Task<List<Claim>> CreateClaims(User user)
        {
            IdentityOptions _options = new IdentityOptions();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            return claims;
        }
    }
}
