namespace AppointmentSystem.Api.Modules.UserModule.DTOs
{
    public class UserResponseDto
    {
        public Guid UserId { get; set; }
        
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
