using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public class AppointmentNoteService : IAppointmentNoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AppointmentNoteService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private Guid GetRequiredUserId()
        {
            if (string.IsNullOrEmpty(_currentUserService.UserId) || !Guid.TryParse(_currentUserService.UserId, out Guid userId))
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }
            return userId;
        }

        public async Task<AppointmentNoteResponseDto> AddNoteAsync(CreateAppointmentNoteRequest request)
        {
            Guid currentUserId = GetRequiredUserId();
            string role = _currentUserService.Role ?? string.Empty;

            Appointment? appointment = await _context.Appointments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId);

            if (appointment == null)
            {
                throw new NotFoundException(ErrorMessages.APPOINTMENT_NOT_FOUND);
            }

            // Authorization Check
            await EnsureCanAccessAppointmentNotesAsync(appointment, currentUserId, role);

            // Note count check
            int noteCount = await _context.AppointmentNotes
                .CountAsync(n => n.AppointmentId == request.AppointmentId && !n.IsDeleted);

            if (noteCount >= BusinessRules.MAX_NOTES_PER_APPOINTMENT)
            {
                throw new BusinessException($"{ErrorMessages.GENERIC_SYSTEM_ERROR} (Max notes reached: {BusinessRules.MAX_NOTES_PER_APPOINTMENT})");
            }

            AppointmentNote note = new AppointmentNote
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                BusinessId = appointment.BusinessId,
                AuthorId = currentUserId.ToString(),
                AuthorRole = role,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.AppointmentNotes.AddAsync(note);
            await _context.SaveChangesAsync();

            return await MapToDtoAsync(note);
        }

        public async Task<List<AppointmentNoteResponseDto>> GetNotesAsync(Guid appointmentId)
        {
            Guid currentUserId = GetRequiredUserId();
            string role = _currentUserService.Role ?? string.Empty;

            Appointment? appointment = await _context.Appointments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new NotFoundException(ErrorMessages.APPOINTMENT_NOT_FOUND);
            }

            await EnsureCanAccessAppointmentNotesAsync(appointment, currentUserId, role);

            List<AppointmentNote> notes = await _context.AppointmentNotes
                .Where(n => n.AppointmentId == appointmentId && !n.IsDeleted)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();

            List<AppointmentNoteResponseDto> dtos = new List<AppointmentNoteResponseDto>();
            foreach (var note in notes)
            {
                dtos.Add(await MapToDtoAsync(note));
            }

            return dtos;
        }

        public async Task UpdateNoteAsync(Guid noteId, UpdateAppointmentNoteRequest request)
        {
            Guid currentUserId = GetRequiredUserId();

            AppointmentNote? note = await _context.AppointmentNotes
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
            {
                throw new NotFoundException("Note not found.");
            }

            if (note.AuthorId != currentUserId.ToString())
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            note.Content = request.Content.Trim();
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(Guid noteId)
        {
            Guid currentUserId = GetRequiredUserId();
            string role = _currentUserService.Role ?? string.Empty;

            AppointmentNote? note = await _context.AppointmentNotes
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
            {
                return;
            }

            bool isBusinessOwner = role == Roles.Business.ToString() && Guid.Parse(note.AuthorId) == currentUserId; // Wait, business owner should be able to delete ANY note of their business
            
            // Re-check business owner properly
            if (role == Roles.Business.ToString())
            {
                // Verify the appointment belongs to this business
                bool ownsBusiness = await _context.Appointments.AnyAsync(a => a.Id == note.AppointmentId && a.BusinessId == currentUserId);
                if (ownsBusiness)
                {
                    note.IsDeleted = true;
                    await _context.SaveChangesAsync();
                    return;
                }
            }

            // If not business owner, must be the author
            if (note.AuthorId != currentUserId.ToString())
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            note.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        private async Task EnsureCanAccessAppointmentNotesAsync(Appointment appointment, Guid currentUserId, string role)
        {
            if (role == Roles.Business.ToString() && appointment.BusinessId == currentUserId)
            {
                return;
            }

            if (role == Roles.User.ToString())
            {
                // Check if this user is the employee assigned to the appointment
                Guid? employeeUserId = appointment.Employee?.UserId;
                if (employeeUserId.HasValue && employeeUserId.Value == currentUserId)
                {
                    return;
                }
            }

            throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
        }

        private async Task<AppointmentNoteResponseDto> MapToDtoAsync(AppointmentNote note)
        {
            string authorName = "Unknown";
            Guid authorGuid = Guid.Parse(note.AuthorId);

            if (note.AuthorRole == Roles.Business.ToString())
            {
                authorName = await _context.Businesses
                    .Where(b => b.Id == authorGuid)
                    .Select(b => b.CompanyName)
                    .FirstOrDefaultAsync() ?? "Business";
            }
            else
            {
                authorName = await _context.Employees
                    .Where(e => e.UserId == authorGuid)
                    .Select(e => $"{e.FirstName} {e.LastName}")
                    .FirstOrDefaultAsync() ?? "Employee";
            }

            return new AppointmentNoteResponseDto
            {
                NoteId = note.Id,
                AppointmentId = note.AppointmentId,
                AuthorId = note.AuthorId,
                AuthorRole = note.AuthorRole,
                AuthorName = authorName,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }
}
