using System.Security.Cryptography;
using System.Text;

namespace AppointmentSystem.Api.Shared.Authentication
{
    public static class TokenGenerator
    {
        public static string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static string HashToken(string token, string secretKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            using var hmac = new HMACSHA256(keyBytes);
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }

        public static string GenerateOTP()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }
    }
}
