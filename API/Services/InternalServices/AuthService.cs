using API.Context;
using API.DTOs;
using API.Models;
using API.Services.ExternalServices;
using API.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace API.Services.InternalServices
{
    public class AuthService
    {
        protected readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;
        //private readonly RoleManager<User> _roleManager;
        

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            EmailService emailService,
            TokenService tokenService,
            AppDbContext dbContext
            //RoleManager<User> roleManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _tokenService = tokenService;
            _context=dbContext;
            //_roleManager = roleManager;
        }



        public async Task<Result> ConfirmEmail(string username, string token)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null) return new Error("NOT_FOUND", "User not found");
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            return result.Succeeded ? Result.Success() : new Error("CONFIRMATION_ERROR", string.Join(", ", result.Errors.Select(e => e.Description)));
        }



        

        public async Task<Result<LoginVM>> Login(LoginDTO loginDTO, HttpResponse response)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.Username);
            if (user is null)
                return Result<LoginVM>.Failure(new Error("NOT_FOUND", "This user does not exist"));

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, true);

            if (signInResult.Succeeded is false)
                return Result<LoginVM>.Failure(new Error("INVALID_USER", "Password is not correct"));

            var confirmEmailCheckResult = await _userManager.IsEmailConfirmedAsync(user);
            if (confirmEmailCheckResult is false)
                return Result<LoginVM>.Failure(new Error("CONFIRMATION_ERROR", "Your email is not confirmed"));

            var accessToken = await _tokenService.CreateAccessToken(user);
            var refreshtoken = _tokenService.CreateRefreshToken();
            var setRefreshTokenResult = await SetRefreshToken(user, refreshtoken);

            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            user.LastLoginTime= DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            

            return Result<LoginVM>.Success(new LoginVM() { AccessToken = accessToken, Roles = roles, RefreshToken = refreshtoken });
        }

        public async Task<Result<LoginVM>> RefreshToken(string refreshtoken)
        {
            var tokenRecord = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshtoken);

            if (tokenRecord == null || tokenRecord.ExpireTime < DateTime.UtcNow)
            {
                return Result<LoginVM>.Failure(new Error("INVALID_TOKEN", "Token is invalid or expired"));
            }

            var user = tokenRecord.User;

            var newAccessToken = await _tokenService.CreateAccessToken(user);
            var newRefreshToken = _tokenService.CreateRefreshToken();

            _context.RefreshTokens.Remove(tokenRecord);
            await SetRefreshToken(user, newRefreshToken);
            user.LastLoginTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return Result<LoginVM>.Success(new LoginVM() { AccessToken=newAccessToken,RefreshToken=newRefreshToken,Roles=user.Roles});
        }



        public async Task<Result> Register(RegisterDTO registerDTO)
        {
            if (await _userManager.FindByEmailAsync(registerDTO.Email!) != null)
            {
                return new Error("CONFLICT", "Email is already in use.");
            }

            var user = CreateNewUser(registerDTO);

            var registerResult = await _userManager.CreateAsync(user, registerDTO.Password);



            if (registerResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user!, "User");

                return await SendConfirmEmail(user);

            }
            else return new Error("BAD_REQUEST", string.Join(", ", registerResult.Errors.Select(e => e.Description))); // error
        }

        public async Task<Result> SendConfirmationLink(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null) return new Error("NOT_FOUND", "This user does not exist");
            return await SendConfirmEmail(user);
        }

        public async Task<Result> SetRefreshToken(User user, TokenCredentials refreshToken, HttpResponse response = null)
        {

            var refreshUserToken = new RefreshToken()
            {
                UserId = user.Id,
                Token = refreshToken.Token,
                ExpireTime=refreshToken.ExpireTime
            };
            await _context.RefreshTokens.AddAsync(refreshUserToken);
            await _context.SaveChangesAsync();
            return Result.Success();
        }




        private User CreateNewUser(RegisterDTO registerDTO)
        {

            
            var newUser = new User()
            {
                FirstName = registerDTO.Firstname,
                LastName = registerDTO.Lastname,
                UserName = registerDTO.Username,
                Email = registerDTO.Email,
                CreatedAt= DateTime.UtcNow,
            };
           
            return newUser;
        }        


        private async Task<Result> SendConfirmEmail(User newUser)
        {

            var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmEmailToken));
            var actionUrl = $@"https://localhost:7244/api/Auth/ConfirmEmail?username={newUser.UserName}&token={encodedToken}";
            var result = await _emailService.sendMailAsync(newUser.Email!, "Confirm Your Email", $"Confirm your email by <a href='{actionUrl}'>clicking here</a>.");
            return result;
        }
    }
}

