using API.DTOs;
using API.Services.InternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserService _userService;
        public AdminController(UserService userService) => _userService = userService;

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers() => Ok(await _userService.GetAllUsersAsync());

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery]string id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            var result = await _userService.CreateUserAsync(dto);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromQuery] string username, [FromBody] UpdateUserDTO updatedUser)
        {
            var result = await _userService.UpdateUserAsync(username, updatedUser);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}
