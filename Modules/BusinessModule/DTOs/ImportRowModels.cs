namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class EmployeeImportRow
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        public bool CanManageOwnSchedule { get; set; }
    }

    public class CustomerImportRow
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }
}
