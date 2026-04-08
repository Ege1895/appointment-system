namespace AppointmentSystem.Api.Modules.BranchModule.DTOs
{
    public class UpdateBranchRequest
    {
        public required string Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
