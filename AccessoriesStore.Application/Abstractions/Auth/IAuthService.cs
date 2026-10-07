using AccessoriesStore.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task LogoutAsync(string refreshToken);
        Task ConfirmEmailAsync(string userId,string token);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordRequest request);

    }
}
