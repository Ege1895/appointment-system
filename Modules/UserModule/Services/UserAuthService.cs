using AppointmentSystem.Api.Modules.UserModule.DTOs;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using AppointmentSystem.Api.Shared.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace AppointmentSystem.Api.Modules.UserModule.Services
{
    public class UserAuthService : IUserAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<UserAuthService> _logger;

        public UserAuthService(
            ApplicationDbContext context,
            JwtTokenGenerator jwtTokenGenerator,
            IOptions<JwtSettings> jwtOptions,
            ILogger<UserAuthService> logger)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<UserAuthResponseDto> RegisterAsync(RegisterUserRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            _logger.LogDebug("Starting user registration for email: {Email}", normalizedEmail);

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (emailExists)
            {
                _logger.LogWarning("User registration failed — email already exists. Email: {Email}", normalizedEmail);
                throw new BusinessException(ErrorMessages.EMAIL_ALREADY_EXISTS);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogDebug("Hashing password and generating refresh token.");
                string refreshToken = TokenGenerator.GenerateRefreshToken();

                User user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = normalizedEmail,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = PasswordHasher.Hash(request.Password),
                    RefreshTokenHash = TokenGenerator.HashToken(refreshToken, _jwtSettings.SecretKey),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(BusinessRules.REFRESH_TOKEN_EXPIRY_DAYS),
                    RefreshTokenUsed = false
                };

                await _context.Users.AddAsync(user);
                _logger.LogDebug("Saving user to database.");
                await _context.SaveChangesAsync();

                _logger.LogDebug("Generating JWT token.");
                string token = _jwtTokenGenerator.GenerateToken(user.Id, normalizedEmail, Roles.User);

                await transaction.CommitAsync();

                _logger.LogInformation("User registered successfully. UserId: {UserId}", user.Id);

                return new UserAuthResponseDto
                {
                    User = MapToResponse(user),
                    Token = token,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(exception, "User registration failed. Email: {Email}", request.Email);
                throw;
            }
        }

        public async Task<UserAuthResponseDto> LoginAsync(LoginUserRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login failed — user not found or inactive. Email: {Email}", request.Email);
                throw new BusinessException(ErrorMessages.INVALID_EMAIL_OR_PASSWORD);
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValidPassword)
            {
                _logger.LogWarning("Login failed — invalid password. UserId: {UserId}", user.Id);
                throw new BusinessException(ErrorMessages.INVALID_EMAIL_OR_PASSWORD);
            }

            string token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, Roles.User);
            string refreshToken = TokenGenerator.GenerateRefreshToken();

            user.RefreshTokenHash = TokenGenerator.HashToken(refreshToken, _jwtSettings.SecretKey);
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(BusinessRules.REFRESH_TOKEN_EXPIRY_DAYS);
            user.RefreshTokenUsed = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in. UserId: {UserId}", user.Id);

            return new UserAuthResponseDto
            {
                User = MapToResponse(user),
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<UserAuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request)
        {
            string requestHash = TokenGenerator.HashToken(request.RefreshToken, _jwtSettings.SecretKey);

            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshTokenHash == requestHash);

            if (user == null || string.IsNullOrEmpty(user.RefreshTokenHash))
            {
                throw new BusinessException(ErrorMessages.INVALID_REFRESH_TOKEN);
            }

            // Constant-time comparison (timing attack prevention)
            byte[] dbHashBytes = Convert.FromBase64String(user.RefreshTokenHash);
            byte[] reqHashBytes = Convert.FromBase64String(requestHash);
            if (!CryptographicOperations.FixedTimeEquals(dbHashBytes, reqHashBytes))
            {
                throw new BusinessException(ErrorMessages.INVALID_REFRESH_TOKEN);
            }

            // Reuse detection
            if (user.RefreshTokenUsed)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();
                _logger.LogWarning("Refresh token reuse detected. Sessions invalidated. UserId: {UserId}", user.Id);
                throw new BusinessException(ErrorMessages.SECURITY_BREACH_DETECTED);
            }

            // Null-safe expiry check
            if (!user.RefreshTokenExpiry.HasValue || user.RefreshTokenExpiry.Value < DateTime.UtcNow)
            {
                throw new BusinessException(ErrorMessages.INVALID_REFRESH_TOKEN);
            }

            string newAccessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, Roles.User);
            string newRefreshToken = TokenGenerator.GenerateRefreshToken();

            user.RefreshTokenHash = TokenGenerator.HashToken(newRefreshToken, _jwtSettings.SecretKey);
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(BusinessRules.REFRESH_TOKEN_EXPIRY_DAYS);
            user.RefreshTokenUsed = false;
            await _context.SaveChangesAsync();

            return new UserAuthResponseDto
            {
                User = MapToResponse(user),
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(Guid userId)
        {
            User? user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiry = null;
                user.RefreshTokenUsed = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("User logged out. UserId: {UserId}", userId);
            }
        }

        private UserResponseDto MapToResponse(User user)
        {
            return new UserResponseDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
}
