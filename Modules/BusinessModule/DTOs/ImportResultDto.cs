using System.Collections.Generic;

namespace AppointmentSystem.Api.Modules.BusinessModule.DTOs
{
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<ImportRowErrorDto> Errors { get; set; } = new();
    }

    public class ImportRowErrorDto
    {
        public int RowNumber { get; set; }
        public string? Email { get; set; }
        public string Reason { get; set; } = null!;
    }
}
