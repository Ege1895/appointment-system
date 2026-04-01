using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AppointmentSystem.Api.Shared.Authentication
{
    public class JwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(IOptions<JwtSettings> options)
        {
            _jwtSettings = options.Value;
        }

        public string GenerateToken(Guid id, string email, string role, Guid? businessId = null)
        {
            ValidateJwtSettings();

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            if (businessId.HasValue)
            {
                claims.Add(new Claim("BusinessId", businessId.Value.ToString()));
            }

            byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);

            SigningCredentials credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void ValidateJwtSettings()
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("Jwt:SecretKey is missing from configuration.");
            }

            if (_jwtSettings.SecretKey.Length < 16)
            {
                throw new InvalidOperationException("Jwt:SecretKey must be at least 16 characters long for HMAC SHA-256.");
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
            {
                throw new InvalidOperationException("Jwt:Issuer is missing from configuration.");
            }

            if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            {
                throw new InvalidOperationException("Jwt:Audience is missing from configuration.");
            }
        }
    }
}