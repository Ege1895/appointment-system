using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Enums;
using AppointmentSystem.Api.Modules.SubscriptionModule.Services;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using AppointmentSystem.Api.Shared.Requests;
using AppointmentSystem.Api.Shared.Models;
using AppointmentSystem.Api.Shared.Notifications;
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
        private readonly IEmailService _emailService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<BusinessAuthService> _logger;

        public BusinessAuthService(
            ApplicationDbContext context,
            JwtTokenGenerator jwtTokenGenerator,
            IOptions<JwtSettings> jwtOptions,
            IEmailService emailService,
            ISubscriptionService subscriptionService,
            ILogger<BusinessAuthService> logger)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtSettings = jwtOptions.Value;
            _emailService = emailService;
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        public async Task<BusinessAuthResponseDto> RegisterAsync(RegisterBusinessRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            _logger.LogDebug("Starting business registration for email: {Email}, Subdomain: {Subdomain}", normalizedEmail, request.Subdomain);

            bool existsAsBusiness = await _context.Businesses.AnyAsync(b => b.Email == normalizedEmail || b.Subdomain == request.Subdomain);
            
            if (existsAsBusiness)
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
                    IsEmailVerified = true,
                    RefreshTokenHash = TokenGenerator.HashToken(refreshToken, _jwtSettings.SecretKey),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(BusinessRules.REFRESH_TOKEN_EXPIRY_DAYS),
                    RefreshTokenUsed = false
                };

                await _context.Businesses.AddAsync(business);
                _logger.LogDebug("Saving business to database. BusinessId: {BusinessId}", businessId);
                await _context.SaveChangesAsync();

                // Assign default subscription pack
                await _subscriptionService.GetOrAssignDefaultAsync(businessId);

                await transaction.CommitAsync();

                // 2. OTP Üret ve Kaydet
                string otpCode = TokenGenerator.GenerateOTP();
                string codeHash = TokenGenerator.HashToken(otpCode, _jwtSettings.SecretKey);

                EmailVerification verification = new EmailVerification
                {
                    Email = normalizedEmail,
                    CodeHash = codeHash,
                    Purpose = "BusinessRegistration",
                    ExpiryDate = DateTime.UtcNow.AddMinutes(15),
                    IsActive = true
                };

                await _context.EmailVerifications.AddAsync(verification);
                await _context.SaveChangesAsync();

                // 3. E-posta Gönder
                string emailBody = $@"
                    <h1>Hoş Geldiniz!</h1>
                    <p>İşletme kaydınızı tamamlamak için lütfen aşağıdaki doğrulama kodunu kullanın:</p>
                    <h2 style='color: #4CAF50; letter-spacing: 5px;'>{otpCode}</h2>
                    <p>Bu kod 15 dakika boyunca geçerlidir.</p>";

                await _emailService.SendAsync(normalizedEmail, "E-posta Doğrulama Kodu", emailBody);

                _logger.LogInformation("Business registered. Verification email sent to {Email}. CODE (Only for Dev): {OTP}", normalizedEmail, otpCode);

                string token = _jwtTokenGenerator.GenerateToken(business.Id, normalizedEmail, Roles.Business.ToString());

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

            if (!business.IsEmailVerified)
            {
                throw new BusinessException("E-posta adresi doğrulanmamış. Lütfen e-postanıza gönderilen kodu girin.");
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

        public async Task<bool> VerifyBusinessEmailAsync(VerifyEmailRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            string codeHash = TokenGenerator.HashToken(request.Code, _jwtSettings.SecretKey);

            EmailVerification? verification = await _context.EmailVerifications
                .Where(v => v.Email == normalizedEmail && v.IsActive && !v.IsDeleted)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (verification == null)
            {
                throw new BusinessException("Doğrulama kodu bulunamadı.");
            }

            if (verification.ExpiryDate < DateTime.UtcNow)
            {
                throw new BusinessException("Doğrulama kodunun süresi dolmuş.");
            }

            if (verification.CodeHash != codeHash)
            {
                verification.AttemptCount++;
                if (verification.AttemptCount >= 5)
                {
                    verification.IsActive = false;
                }
                await _context.SaveChangesAsync();
                throw new BusinessException("Hatalı doğrulama kodu.");
            }

            // Doğrulama başarılı
            Business? business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.Email == normalizedEmail);

            if (business != null)
            {
                business.IsEmailVerified = true;
                verification.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task ResendBusinessVerificationCodeAsync(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            // Son 1 dakika içinde kod istenmiş mi kontrol et (Rate limit)
            bool recentlySent = await _context.EmailVerifications
                .AnyAsync(v => v.Email == normalizedEmail && v.CreatedAt > DateTime.UtcNow.AddMinutes(-1));

            if (recentlySent)
            {
                throw new BusinessException("Çok sık kod talebi gönderiyorsunuz. Lütfen 1 dakika sonra tekrar deneyin.");
            }

            string otpCode = TokenGenerator.GenerateOTP();
            string codeHash = TokenGenerator.HashToken(otpCode, _jwtSettings.SecretKey);

            EmailVerification verification = new EmailVerification
            {
                Email = normalizedEmail,
                CodeHash = codeHash,
                Purpose = "BusinessRegistration",
                ExpiryDate = DateTime.UtcNow.AddMinutes(15),
                IsActive = true
            };

            await _context.EmailVerifications.AddAsync(verification);
            await _context.SaveChangesAsync();

            string emailBody = $@"
                <h1>Yeni Doğrulama Kodunuz</h1>
                <p>Kayıt işlemini tamamlamak için aşağıdaki yeni kodu kullanabilirsiniz:</p>
                <h2 style='color: #4CAF50; letter-spacing: 5px;'>{otpCode}</h2>
                <p>Bu kod 15 dakika boyunca geçerlidir.</p>";

            await _emailService.SendAsync(normalizedEmail, "Yeni Doğrulama Kodu", emailBody);
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
