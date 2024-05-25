using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VRApi.Data;
using VRApi.Helpers;
using VRApi.Models;

namespace VRApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration["DB_CONNECTION"] ?? throw new InvalidOperationException("Connection string not found.");
            var issuer = builder.Configuration["ISSUER"] ?? throw new InvalidOperationException("Issuer not found.");
            var secretKey = builder.Configuration["SECRET_KEY"] ?? throw new InvalidOperationException("Secret key not found.");
            var audience = builder.Configuration["AUDIENCE"] ?? throw new InvalidOperationException("Audience not found.");

            var jwtOptions = new JwtOptions(secretKey, issuer, audience);
            builder.Services.AddSingleton(options => jwtOptions);


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = jwtOptions.SymmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                };
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options => 
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // enables immediate logout, after updating the user's security stamp.
                options.ValidationInterval = TimeSpan.Zero;
            });

            builder.Services.AddDefaultIdentity<User>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 2;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/users/login";
                options.AccessDeniedPath = "/";
            });
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Initialize data
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await DataInitializer.SeedData(dbContext, userManager, roleManager);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapControllers();

            app.Run();
        }
    }
}
