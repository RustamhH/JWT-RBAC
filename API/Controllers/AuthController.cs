using API.DTOs;
using API.Services.InternalServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Fields

        private readonly AuthService _authService;

        // Constructor

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Methods

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {

            var result = await _authService.Register(registerDTO);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result.Error);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string username, string token)
        {

            var result = await _authService.ConfirmEmail(username, token);
            return result.IsSuccess ? Ok("Your email is confirmed!") : BadRequest(result.Error);
        }


        [HttpGet("SendConfirmationLink")]
        public async Task<IActionResult> SendConfirmationLink([FromQuery] string username)
        {

            var result = await _authService.SendConfirmationLink(username);
            return result.IsSuccess ? Ok("Confirmation Link Sent") : BadRequest(result.Error);
        }



        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {

            var result = await _authService.Login(loginDTO, Response);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpPost("RefreshLogin")]
        public async Task<IActionResult> RefreshLogin([FromQuery] string refreshToken)
        {

            var result = await _authService.RefreshToken(refreshToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

    }
}
