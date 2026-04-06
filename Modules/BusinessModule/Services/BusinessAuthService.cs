using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Enums;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using AppointmentSystem.Api.Shared.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public class BusinessAuthService : IBusinessAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<BusinessAuthService> _logger;

        public BusinessAuthService(
            ApplicationDbContext context,
            JwtTokenGenerator jwtTokenGenerator,
            IOptions<JwtSettings> jwtOptions,
            ILogger<BusinessAuthService> logger)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        public async Task<BusinessAuthResponseDto> RegisterAsync(RegisterBusinessRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            _logger.LogDebug("Starting business registration for email: {Email}, Subdomain: {Subdomain}", normalizedEmail, request.Subdomain);

            bool existsAsBusiness = await _context.Businesses.AnyAsync(b => b.Email == normalizedEmail || b.Subdomain == request.Subdomain);
            bool existsAsUser = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
            
            if (existsAsBusiness || existsAsUser)
            {
                _logger.LogWarning("Business registration failed — email or subdomain already exists. Email: {Email}, Subdomain: {Subdomain}", normalizedEmail, request.Subdomain);
                throw new BusinessException(ErrorMessages.EMAIL_ALREADY_EXISTS);
            }

            if (!request.KvkkAccepted)
            {
                throw new BusinessException(ErrorMessages.KVKK_NOT_ACCEPTED);
            }

            if (!request.PrivacyPolicyAccepted)
            {
                throw new BusinessException(ErrorMessages.PRIVACY_POLICY_NOT_ACCEPTED);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogDebug("Hashing password and generating refresh token for business.");
                string passwordHash = PasswordHasher.Hash(request.Password);
                string refreshToken = TokenGenerator.GenerateRefreshToken();

                Guid businessId = Guid.NewGuid();

                Business business = new Business
                {
                    Id = businessId,
                    CompanyName = request.CompanyName,
                    Subdomain = request.Subdomain,
                    OwnerFirstName = request.OwnerFirstName,
                    OwnerLastName = request.OwnerLastName,
                    Email = normalizedEmail,
                    ContactPhone = request.ContactPhone,
                    PasswordHash = passwordHash,
                    EmployeeCapacity = BusinessRules.DEFAULT_EMPLOYEE_CAPACITY,
                    Category = request.Category,
                    KvkkAccepted = request.KvkkAccepted,
                    PrivacyPolicyAccepted = request.PrivacyPolicyAccepted,
                    AcceptedAt = DateTime.UtcNow,
                    KvkkVersion = ValidationConstants.LATEST_KVKK_VERSION,
                    PrivacyVersion = ValidationConstants.LATEST_PRIVACY_POLICY_VERSION,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    RefreshTokenHash = TokenGenerator.HashToken(refreshToken, _jwtSettings.SecretKey),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(BusinessRules.REFRESH_TOKEN_EXPIRY_DAYS),
                    RefreshTokenUsed = false
                };

                await _context.Businesses.AddAsync(business);
                _logger.LogDebug("Saving business to database. BusinessId: {BusinessId}", businessId);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Generating JWT token for business.");
                string token = _jwtTokenGenerator.GenerateToken(businessId, normalizedEmail, Roles.Business.ToString());

                await transaction.CommitAsync();

                _logger.LogInformation("Business registered successfully. BusinessId: {BusinessId}", business.Id);

                return new BusinessAuthResponseDto
                {
                    Business = MapToResponse(business),
                    Token = token,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(exception, "Business registration failed and transaction rolled back. Email: {Email}", normalizedEmail);
                throw;
            }
        }

        public async Task<BusinessAuthResponseDto> LoginAsync(LoginBusinessRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();

            Business? business = await _context.Businesses.FirstOrDefaultAsync(b => b.Email == normalizedEmail);

            if (business == null || !business.IsActive)
            {
                _logger.LogWarning("Business login failed — not found or inactive. Email: {Email}", normalizedEmail);
                throw new BusinessException(ErrorMessages.INVALID_EMAIL_OR_PASSWORD);
            }

            bool isPasswordValid = PasswordHasher.Verify(request.Password, business.PasswordHash);

            if (!isPasswordValid)
            {
                _logger.LogWarning("Business login failed — invalid password. BusinessId: {BusinessId}", business.Id);
                throw new BusinessException(ErrorMessages.INVALID_EMAIL_OR_PASSWORD);
            }

            string token = _jwtTokenGenerator.GenerateToken(business.Id, business.Email, Roles.Business.ToString());
            string refreshToken = TokenGenerator.GenerateRefreshToken();

            business.RefreshTokenHash = TokenGenerator.HashToken(refreshToken, _jwtSettings.SecretKey);
            business.RefreshTokenExpiry = DateTime.UtcNow.AddDays(BusinessRules.REFRESH_TOKEN_EXPIRY_DAYS);
            business.RefreshTokenUsed = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Business logged in. BusinessId: {BusinessId}", business.Id);

            return new BusinessAuthResponseDto
            {
                Business = MapToResponse(business),
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<BusinessAuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request)
        {
            string requestHash = TokenGenerator.HashToken(request.RefreshToken, _jwtSettings.SecretKey);

            Business? business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.RefreshTokenHash == requestHash);

            if (business == null || string.IsNullOrEmpty(business.RefreshTokenHash))
            {
                throw new BusinessException(ErrorMessages.INVALID_REFRESH_TOKEN);
            }

            // Constant-time comparison (timing attack prevention)
            byte[] dbHashBytes = Convert.FromBase64String(business.RefreshTokenHash);
            byte[] reqHashBytes = Convert.FromBase64String(requestHash);
            if (!CryptographicOperations.FixedTimeEquals(dbHashBytes, reqHashBytes))
            {
                throw new BusinessException(ErrorMessages.INVALID_REFRESH_TOKEN);
            }

            // Reuse detection
            if (business.RefreshTokenUsed)
            {
                business.RefreshTokenHash = null;
                business.RefreshTokenExpiry = null;
                await _context.SaveChangesAsync();
                _logger.LogWarning("Business refresh token reuse detected. Sessions invalidated. BusinessId: {BusinessId}", business.Id);
                throw new BusinessException(ErrorMessages.SECURITY_BREACH_DETECTED);
            }

            // Null-safe expiry check
            if (!business.RefreshTokenExpiry.HasValue || business.RefreshTokenExpiry.Value < DateTime.UtcNow)
            {
                throw new BusinessException(ErrorMessages.INVALID_REFRESH_TOKEN);
            }

            string newAccessToken = _jwtTokenGenerator.GenerateToken(business.Id, business.Email, Roles.Business.ToString());
            string newRefreshToken = TokenGenerator.GenerateRefreshToken();

            business.RefreshTokenHash = TokenGenerator.HashToken(newRefreshToken, _jwtSettings.SecretKey);
            business.RefreshTokenExpiry = DateTime.UtcNow.AddDays(BusinessRules.REFRESH_TOKEN_EXPIRY_DAYS);
            business.RefreshTokenUsed = false;
            await _context.SaveChangesAsync();

            return new BusinessAuthResponseDto
            {
                Business = MapToResponse(business),
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(Guid businessId)
        {
            Business? business = await _context.Businesses.FindAsync(businessId);
            if (business != null)
            {
                business.RefreshTokenHash = null;
                business.RefreshTokenExpiry = null;
                business.RefreshTokenUsed = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Business logged out. BusinessId: {BusinessId}", businessId);
            }
        }

        private BusinessResponseDto MapToResponse(Business business)
        {
            return new BusinessResponseDto
            {
                BusinessId = business.Id,
                CompanyName = business.CompanyName,
                Subdomain = business.Subdomain,
                OwnerFirstName = business.OwnerFirstName,
                OwnerLastName = business.OwnerLastName,
                Email = business.Email,
                ContactPhone = business.ContactPhone,
                EmployeeCapacity = business.EmployeeCapacity,
                Category = business.Category,
                City = business.City,
                District = business.District,
                Neighborhood = business.Neighborhood,
                Description = business.Description,
                IsOpen = business.IsOpen,
                WorkingHourStart = business.WorkingHourStart,
                WorkingHourEnd = business.WorkingHourEnd,
                WorkingDays = business.WorkingDays,
                GalleryUrls = business.GalleryUrls,
                CreatedAt = business.CreatedAt
            };
        }
    }
}
