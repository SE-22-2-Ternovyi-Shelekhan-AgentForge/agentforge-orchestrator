using AgentForge.Orchestrator.Exceptions;
using AgentForge.Orchestrator.Models;
using AgentForge.Orchestrator.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentForge.Orchestrator.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (EmailAlreadyExistsException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}
