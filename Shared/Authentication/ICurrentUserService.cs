namespace AppointmentSystem.Api.Shared.Authentication
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Role { get; }
        Guid? TenantId { get; }
    }
}