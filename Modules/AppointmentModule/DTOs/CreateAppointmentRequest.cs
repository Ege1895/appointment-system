namespace AppointmentSystem.Api.Modules.AppointmentModule.DTOs
{
    public class CreateAppointmentRequest
    {
        public Guid BusinessId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid OfferingId { get; set; }
        
        // Müşterinin seçtiği başlangıç tarihi/saati (UTC olarak gelmeli)
        public DateTime StartTimeUtc { get; set; }
    }
}
