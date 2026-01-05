using API.Context;
using API.DTOs;
using API.Models;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;

namespace API.Services.InternalServices
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;

        public UserService(UserManager<User> userManager, AuthService authservice,TokenService tokenService)
        {
            _userManager = userManager;
            _authService = authservice;
            _tokenService = tokenService;
        }

        // --- Admin Level Methods ---
        public async Task<List<User>> GetAllUsersAsync() => await _userManager.Users.ToListAsync();

        public async Task<Result> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            if (await _userManager.FindByEmailAsync(createUserDTO.Email!) != null)
            {
                return new Error("CONFLICT", "Email is already in use.");
            }

            var newUser = new User()
            {
                FirstName = createUserDTO.Firstname,
                LastName = createUserDTO.Lastname,
                UserName = createUserDTO.Username,
                Email = createUserDTO.Email,
                CreatedAt = DateTime.UtcNow,
            };

            var registerResult = await _userManager.CreateAsync(newUser, createUserDTO.Password);



            if (registerResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser!, "User");
                newUser.EmailConfirmed=true;
                await _userManager.UpdateAsync(newUser);
                return Result.Success();
            }
            else return new Error("BAD_REQUEST", string.Join(", ", registerResult.Errors.Select(e => e.Description))); // error
        }

        public async Task<Result> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user is not null)
            {
                var deleteresult=await _userManager.DeleteAsync(user);
                return deleteresult.Succeeded ? Result.Success() : 
                    Result.Failure(new Error("DELETE_ERROR", string.Join(", ", deleteresult.Errors.Select(e => e.Description))));
            }
            return Result.Failure(new Error("NOT_FOUND","User not found"));
        }

        // --- Shared/User Level Methods ---

        public async Task<Result> UpdateUserAsync(string username,UpdateUserDTO user)
        {
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser == null) return Result.Failure(new Error("ERROR","User not found"));

            // Update specific fields
            existingUser.FirstName = user.Firstname;
            existingUser.LastName = user.Lastname;
            existingUser.UpdatedAt = DateTime.UtcNow;
            existingUser.Email= user.Email;
            existingUser.UserName= user.Username;

            var token = _tokenService.CreateRefreshToken();
            await _authService.SetRefreshToken(existingUser, token);
            await _authService.RefreshToken(token.Token);

            var updateresult = await _userManager.UpdateAsync(existingUser);
            return updateresult.Succeeded ? Result.Success() :
                    Result.Failure(new Error("UPDATE_ERROR", string.Join(", ", updateresult.Errors.Select(e => e.Description))));
        }

        public async Task<Result<User>> GetUserByUsername(string username)
        {
            var existingUser = await _userManager.FindByNameAsync(username);
            return existingUser is not null ? Result<User>.Success(existingUser) :
                    Result<User>.Failure(new Error("NOT_FOUND", "This user does not exist"));
        }
    }
}
