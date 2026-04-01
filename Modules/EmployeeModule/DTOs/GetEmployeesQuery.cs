namespace AppointmentSystem.Api.Modules.EmployeeModule.DTOs
{
    public class GetEmployeesQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
