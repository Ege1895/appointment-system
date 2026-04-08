using System;

namespace AppointmentSystem.Api.Modules.AppointmentModule.DTOs
{
    public class AppointmentNoteResponseDto
    {
        public Guid NoteId { get; set; }
        public Guid AppointmentId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
