using API.Context;
using API.Models;
using API.Services.ExternalServices;
using API.Services.InternalServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;

namespace API
{
    public static class DependencyInjection
    {

        public static void AddPersistenceRegister(this IServiceCollection services)
        {

            // Context

            services.AddDbContext<AppDbContext>(options => {
                ConfigurationBuilder configurationBuilder = new();
                var builder = configurationBuilder.AddJsonFile("appsettings.json").Build();
                options.UseSqlServer(builder.GetConnectionString("Default"));
                //.UseSqlServer(builder.GetConnectionString("AzureFree"));
            });


            services.AddIdentity<User, IdentityRole>()
                                        .AddEntityFrameworkStores<AppDbContext>()
                                        .AddDefaultTokenProviders();

        }


        public static async Task AddRoleServicesAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Check by Email or Username
            var adminUser = await userManager.FindByNameAsync("admin");

            if (adminUser is null)
            {
                var newAdmin = new User
                {
                    UserName = "admin",
                    FirstName = "Admin",
                    LastName = "Adminov",
                    Email = "mya8min@gmail.com",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(newAdmin, "aHsgd527gY-1");

                if (result.Succeeded)
                {
                    // Use the object we just created instead of fetching it again
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }

        public static void AddInfrastructureRegister(this WebApplicationBuilder builder)
        {

            // Dependancy Injection

            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddTransient<EmailService>();


            // Add Auth JWT

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        RoleClaimType = ClaimTypes.Role,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        LifetimeValidator = (before, expires, token, param) => expires > DateTime.UtcNow,
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]!))
                    };
                });

            builder.Services.AddAuthorization();
        }
    }

}
