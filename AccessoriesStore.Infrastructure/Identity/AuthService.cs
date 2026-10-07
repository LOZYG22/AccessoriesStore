using AccessoriesStore.Application.Abstractions.Auth;
using AccessoriesStore.Application.Abstractions.Email;
using AccessoriesStore.Application.DTOs.Auth;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AccessoriesStore.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
            _emailService = emailService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ",
                    result.Errors.Select(e => e.Description));

                throw new BadRequestException(errors);
            }

            var confirmationToken =
                await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(confirmationToken);

            var confirmationLink =
                $"https://localhost:7043/api/auth/confirm-email" +
                $"?userId={user.Id}&token={encodedToken}";

            var emailBody = $"""
        <h2>Welcome to Accessories Store</h2>

        <p>Hello {user.FirstName},</p>

        <p>Thank you for creating your account.</p>

        <p>Please confirm your email address by clicking the button below:</p>

        <p>
            <a href="{confirmationLink}"
               style="
                    display:inline-block;
                    padding:10px 20px;
                    background-color:#000;
                    color:#fff;
                    text-decoration:none;
                    border-radius:5px;">
                Confirm Email
            </a>
        </p>

        <p>If you did not create this account, you can ignore this email.</p>
        """;

            await _emailService.SendAsync(
                user.Email!,
                "Confirm your Accessories Store account",
                emailBody);

            return new AuthResponse
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                AccessTokenExpiration = DateTime.UtcNow
            };
        }
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!user.EmailConfirmed)
            {
                throw new UnauthorizedException(
                    "Please confirm your email before logging in.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Email!,
                roles);

            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return new AuthResponse
            {
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken,
                AccessTokenExpiration = accessToken.Expiration
            };
        }

        private async Task<string> CreateRefreshTokenAsync(string userId)
        {
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenHash = HashRefreshToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                TokenHash = refreshTokenHash,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var refreshTokenHash = HashRefreshToken(request.RefreshToken);

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.TokenHash == refreshTokenHash);

            if (refreshToken is null)
            {
                throw new UnauthorizedException("Invalid refresh token.");
            }

            if (refreshToken.IsRevoked)
            {
                throw new UnauthorizedException("Refresh token has been revoked.");
            }

            if (refreshToken.IsExpired)
            {
                throw new UnauthorizedException("Refresh token has expired.");
            }

            var user = await _userManager.FindByIdAsync(refreshToken.UserId);

            if (user is null)
            {
                throw new NotFoundException("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Email!,
                roles);

            var newRefreshToken = await CreateRefreshTokenAsync(user.Id);

            refreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken.Token,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = accessToken.Expiration
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var refreshTokenHash = HashRefreshToken(refreshToken);

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.TokenHash == refreshTokenHash);

            if (token is null)
            {
                return;
            }

            if (!token.IsRevoked)
            {
                token.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private static string HashRefreshToken(string refreshToken)
        {
            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(refreshToken));

            return Convert.ToBase64String(hash);
        }

        public async Task ConfirmEmailAsync(
            string userId,
            string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new NotFoundException("User not found.");

            if (user.EmailConfirmed)
                throw new BadRequestException(
                    "Email is already confirmed.");

            var result = await _userManager.ConfirmEmailAsync(
                user,
                token);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    result.Errors.Select(e => e.Description));

                throw new BadRequestException(errors);
            }
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return;
            }

            var resetToken =
                await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(resetToken);

            var resetLink =
                $"https://localhost:7043/api/auth/reset-password" +
                $"?userId={user.Id}&token={encodedToken}";

            var emailBody = $"""
        <h2>Password Reset</h2>

        <p>Hello {user.FirstName},</p>

        <p>We received a request to reset your password.</p>

        <p>
            <a href="{resetLink}"
               style="
                    display:inline-block;
                    padding:10px 20px;
                    background-color:#000;
                    color:#fff;
                    text-decoration:none;
                    border-radius:5px;">
                Reset Password
            </a>
        </p>

        <p>If you did not request a password reset, you can ignore this email.</p>
        """;

            await _emailService.SendAsync(
                user.Email!,
                "Reset your Accessories Store password",
                emailBody);
        }

        public async Task ResetPasswordAsync(
            ResetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user is null)
            {
                throw new BadRequestException(
                    "Invalid password reset request.");
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                request.Token,
                request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    result.Errors.Select(e => e.Description));

                throw new BadRequestException(errors);
            }
        }
    }
}
