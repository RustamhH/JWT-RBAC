using API.DTOs;
using API.Models;
using API.Services.InternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService) => _userService = userService;

        [HttpGet("GetMyProfile")]
        public async Task<IActionResult> GetMyProfile([FromQuery]string username)
        {
            if (username == User.FindFirstValue(ClaimTypes.Name))
            {
                var result = await _userService.GetUserByUsername(username);
                return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
            }
            return BadRequest("You can only view your own profile");

        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] string username, [FromBody] UpdateUserDTO updatedUser)
        {
           if(username == User.FindFirstValue(ClaimTypes.Name))
           {
                var result = await _userService.UpdateUserAsync(username,updatedUser);
                return result.IsSuccess ? Ok() : BadRequest(result.Error);
           }
           return BadRequest("You can only update your own profile.");
        }
    }
}
