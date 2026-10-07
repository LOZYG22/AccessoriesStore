using AccessoriesStore.Application.Abstractions.Auth;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessoriesStore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            return Ok(
                    ApiResponse<AuthResponse>.SuccessResponse(
                        result,
                        "Registration successful.")
                );
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(
                    ApiResponse<AuthResponse>.SuccessResponse(
                        result,
                        "Login successful.")
                );
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            return Ok(
                    ApiResponse<AuthResponse>.SuccessResponse(
                        result,
                        "Token refreshed successfully.")
                );
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);

            return Ok(
                    ApiResponse<object>.SuccessResponse(
                        null,
                        "Logged out successfully.")
                );
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(
                    ApiResponse<object>.SuccessResponse(
                        new
                        {
                            userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                        },
                        "You are authenticated!"
                    )
                );
        }

        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<ActionResult<ApiResponse<object>>> ConfirmEmail(
            [FromQuery] string userId,
            [FromQuery] string token)
        {
            await _authService.ConfirmEmailAsync(
                userId,
                token);

            return Ok(
                ApiResponse<object>.SuccessResponse(
                    null,
                    "Email confirmed successfully."));
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
            ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request.Email);

            return Ok(
                ApiResponse<object>.SuccessResponse(
                    null,
                    "If the email exists, a password reset link has been sent."));
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
            ResetPasswordRequest request)
        {
            await _authService.ResetPasswordAsync(request);

            return Ok(
                ApiResponse<object>.SuccessResponse(
                    null,
                    "Password has been reset successfully."));
        }
    }
}
