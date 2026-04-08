using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public class AppointmentMessageService : IAppointmentMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AppointmentMessageService(ApplicationDbContext context, ICurrentUserService currentUserService)
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

        public async Task<AppointmentMessageResponseDto> SendAsync(SendAppointmentMessageRequest request)
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

            // Status check
            if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Declined)
            {
                throw new BusinessException("İptal edilmiş veya reddedilmiş randevulara mesaj gönderilemez.");
            }

            // Authorization
            await EnsureCanAccessAppointmentAsync(appointment, currentUserId, role);

            // Rate Limit: Max 20 messages per hour per appointment per sender
            DateTime oneHourAgo = DateTime.UtcNow.AddHours(-1);
            int count = await _context.AppointmentMessages
                .CountAsync(m => m.AppointmentId == request.AppointmentId && m.SenderId == currentUserId.ToString() && m.CreatedAt > oneHourAgo);

            if (count >= 20)
            {
                throw new BusinessException("Mesaj gönderim limitine ulaşıldı. Lütfen daha sonra tekrar deneyin.");
            }

            // Sanitization
            string sanitizedContent = Regex.Replace(request.Content ?? string.Empty, "<.*?>", string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(sanitizedContent))
            {
                throw new BusinessException(ValidationConstants.REQUIRED);
            }

            AppointmentMessage message = new AppointmentMessage
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                BusinessId = appointment.BusinessId,
                SenderId = currentUserId.ToString(),
                SenderRole = role,
                Content = sanitizedContent,
                CreatedAt = DateTime.UtcNow
            };

            // Read Status logic
            if (role == Roles.Business.ToString())
            {
                message.IsReadByBusiness = true;
                message.IsReadByCustomer = false;
            }
            else if (role == Roles.User.ToString())
            {
                // Is this User also an Employee?
                bool isEmployee = appointment.Employee?.UserId == currentUserId;
                if (isEmployee)
                {
                    message.IsReadByBusiness = true; // Employee represents business side
                    message.IsReadByCustomer = false;
                }
                else
                {
                    message.IsReadByCustomer = true;
                    message.IsReadByBusiness = false;
                }
            }

            await _context.AppointmentMessages.AddAsync(message);
            await _context.SaveChangesAsync();

            return MapToDto(message);
        }

        public async Task<List<AppointmentMessageResponseDto>> GetThreadAsync(Guid appointmentId)
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

            await EnsureCanAccessAppointmentAsync(appointment, currentUserId, role);

            var messages = await _context.AppointmentMessages
                .Where(m => m.AppointmentId == appointmentId && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // Auto-mark as read
            bool wasReadStatusUpdated = false;
            foreach (var message in messages)
            {
                if (role == Roles.Business.ToString() || appointment.Employee?.UserId == currentUserId)
                {
                    if (!message.IsReadByBusiness)
                    {
                        message.IsReadByBusiness = true;
                        wasReadStatusUpdated = true;
                    }
                }
                else if (role == Roles.User.ToString() && appointment.UserId == currentUserId)
                {
                    if (!message.IsReadByCustomer)
                    {
                        message.IsReadByCustomer = true;
                        wasReadStatusUpdated = true;
                    }
                }
            }

            if (wasReadStatusUpdated)
            {
                await _context.SaveChangesAsync();
            }

            return messages.Select(MapToDto).ToList();
        }

        public async Task MarkAsReadAsync(Guid appointmentId)
        {
            Guid currentUserId = GetRequiredUserId();
            string role = _currentUserService.Role ?? string.Empty;

            Appointment? appointment = await _context.Appointments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) throw new NotFoundException(ErrorMessages.APPOINTMENT_NOT_FOUND);
            
            await EnsureCanAccessAppointmentAsync(appointment, currentUserId, role);

            var unreadMessages = await _context.AppointmentMessages
                .Where(m => m.AppointmentId == appointmentId && !m.IsDeleted)
                .ToListAsync();

            bool changed = false;
            foreach (var msg in unreadMessages)
            {
                if (role == Roles.Business.ToString() || appointment.Employee?.UserId == currentUserId)
                {
                    if (!msg.IsReadByBusiness) { msg.IsReadByBusiness = true; changed = true; }
                }
                else
                {
                    if (!msg.IsReadByCustomer) { msg.IsReadByCustomer = true; changed = true; }
                }
            }

            if (changed) await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            Guid currentUserId = GetRequiredUserId();
            string role = _currentUserService.Role ?? string.Empty;

            if (role == Roles.Business.ToString())
            {
                // Business owner: All unread messages in their business
                return await _context.AppointmentMessages
                    .CountAsync(m => m.BusinessId == currentUserId && !m.IsReadByBusiness && !m.IsDeleted);
            }
            else
            {
                // Regular User (Customer) or Employee
                // This is more complex because we need to join with Appointments.
                // We'll use a manual query here to ensure correct scoping.
                
                // First find appointments where this user is involved
                var customerUnread = await _context.AppointmentMessages
                    .Where(m => !m.IsReadByCustomer && !m.IsDeleted)
                    .Join(_context.Appointments, 
                        m => m.AppointmentId, 
                        a => a.Id, 
                        (m, a) => new { m, a })
                    .Where(x => x.a.UserId == currentUserId)
                    .CountAsync();

                var employeeUnread = await _context.AppointmentMessages
                    .Where(m => !m.IsReadByBusiness && !m.IsDeleted)
                    .Join(_context.Appointments,
                        m => m.AppointmentId,
                        a => a.Id,
                        (m, a) => new { m, a })
                    .Include(x => x.a.Employee)
                    .Where(x => x.a.Employee.UserId == currentUserId)
                    .CountAsync();

                return customerUnread + employeeUnread;
            }
        }

        private async Task EnsureCanAccessAppointmentAsync(Appointment appointment, Guid currentUserId, string role)
        {
            // Business Owner
            if (role == Roles.Business.ToString() && appointment.BusinessId == currentUserId)
            {
                return;
            }

            // Customer
            if (role == Roles.User.ToString() && appointment.UserId == currentUserId)
            {
                return;
            }

            // Employee
            if (role == Roles.User.ToString())
            {
                if (appointment.Employee?.UserId == currentUserId)
                {
                    return;
                }
            }

            throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
        }

        private AppointmentMessageResponseDto MapToDto(AppointmentMessage message)
        {
            return new AppointmentMessageResponseDto
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                SenderRole = message.SenderRole,
                Content = message.Content,
                IsReadByBusiness = message.IsReadByBusiness,
                IsReadByCustomer = message.IsReadByCustomer,
                CreatedAt = message.CreatedAt
            };
        }
    }
}
