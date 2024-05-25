using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRApi.Data;
using VRApi.Models;

namespace VRApi.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        private readonly ILogger _logger;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext _context;

        private readonly SignInManager<User> _signInManager;

        public UsersController(ILogger<UsersController> logger, 
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            var errors = TempData["Errors"] as IEnumerable<string>;
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            var users = _userManager.Users.ToList();
            var userViewmodels = new List<UserViewmodel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var strRoles = string.Join(',', roles);
                var userViewmodel = new UserViewmodel()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Roles = strRoles
                };
                userViewmodels.Add(userViewmodel);
            }
            return View(userViewmodels);
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "admin");
                if (!isAdmin)
                {
                    ModelState.AddModelError(string.Empty, "Вы не имеете доступа к данному ресурсу");
                    return View();
                }
            }
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return View();
            }
            return RedirectToAction("Index");
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(AddUserViewmodel model)
        {
            var existUser = await _userManager.FindByNameAsync(model.Username);
            if (existUser != null)
            {   
                ModelState.AddModelError(string.Empty, $"Пользователь {model.Username} уже существует!");
            }
            var isRoleExist = await _roleManager.RoleExistsAsync(model.Role);
            if (!isRoleExist)
            {
                ModelState.AddModelError(string.Empty, $"Роли {model.Username} не существует!");
            }
            if (!ModelState.IsValid)
            {
                TempData["Errors"] = ModelState.Values.SelectMany(v => v.Errors)
                                                      .Select(x => x.ErrorMessage)
                                                      .ToList();
                return RedirectToAction("Index");
            }
            var user = new User()
            {
                UserName = model.Username,
            };
            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                TempData["Errors"] = createResult.Errors.Select(x => x.Description).ToList();
                return RedirectToAction("Index");
            }
            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                TempData["Errors"] = createResult.Errors.Select(x => x.Description).ToList();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(DeleteViewmodel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
