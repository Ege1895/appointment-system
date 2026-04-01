namespace AppointmentSystem.Api.Shared.Interfaces
{
    public interface IMustHaveTenant
    {
        public Guid BusinessId { get; set; }
    }
}
