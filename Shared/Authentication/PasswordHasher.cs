namespace AppointmentSystem.Api.Shared.Authentication
{
    public static class PasswordHasher
    {
        private const int WORK_FACTOR = 12;

        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WORK_FACTOR);
        }

        public static bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}