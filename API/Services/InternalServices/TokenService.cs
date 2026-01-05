using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services.InternalServices
{
    public class TokenService
    {
        // Fields

        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        // Constructor

        public TokenService(IConfiguration configuration, UserManager<User> userManager)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<TokenCredentials> CreateAccessToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            IEnumerable<Claim> claims = [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                ..roles.Select(r=>new Claim(ClaimTypes.Role,r))
            ];

            var tokenDescription = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow.AddMinutes(2),
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
                SigningCredentials = credentials,
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken? token = tokenHandler.CreateToken(tokenDescription);

            var accessToken = new TokenCredentials()
            {
                Token = tokenHandler.WriteToken(token),
                ExpireTime = tokenDescription.Expires
            };

            return accessToken;


        }

        
        public TokenCredentials CreateRefreshToken()
        {

            var refreshToken = new TokenCredentials()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpireTime = DateTime.UtcNow.AddDays(7)
            };
            return refreshToken;
        }
    }
}
