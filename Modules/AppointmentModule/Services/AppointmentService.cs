using System.Data;
using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Shared.Notifications;
using AppointmentSystem.Api.Shared.Notifications.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBusinessCustomerService _businessCustomerService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            IBusinessCustomerService businessCustomerService,
            IEmailService emailService,
            ILogger<AppointmentService> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _businessCustomerService = businessCustomerService;
            _emailService = emailService;
            _logger = logger;
        }

        private Guid GetRequiredUserId()
        {
            if (string.IsNullOrEmpty(_currentUserService.UserId) || !Guid.TryParse(_currentUserService.UserId, out Guid userId))
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }
            return userId;
        }

        public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentRequest request)
        {
            Guid userId = GetRequiredUserId();

            if (_currentUserService.Role != Roles.User.ToString())
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            bool isBlocked = await _businessCustomerService.IsBlockedAsync(request.BusinessId, userId);
            if (isBlocked) 
            {
                throw new BusinessException(ErrorMessages.CUSTOMER_BLOCKED_BY_BUSINESS);
            }

            Offering? offering = await _context.Offerings
                .FirstOrDefaultAsync(s => s.Id == request.OfferingId && s.BusinessId == request.BusinessId && s.IsActive);

            if (offering == null)
            {
                throw new NotFoundException(ErrorMessages.OFFERING_NOT_FOUND);
            }

            // Branch validation
            if (request.BranchId.HasValue)
            {
                bool branchExists = await _context.Branches
                    .AnyAsync(b => b.Id == request.BranchId.Value && b.BusinessId == request.BusinessId);

                if (!branchExists)
                {
                    throw new NotFoundException(ErrorMessages.BRANCH_NOT_FOUND);
                }

                // Employee-Branch validation: Employee must be in the same branch OR unassigned (null branch)
                bool employeeInBranch = await _context.Employees
                    .AnyAsync(e => e.Id == request.EmployeeId && e.BusinessId == request.BusinessId 
                                   && (e.BranchId == null || e.BranchId == request.BranchId.Value));

                if (!employeeInBranch)
                {
                    // Personel bu şubede çalışmıyor (ve genel personel de değil)
                    throw new BusinessException(ErrorMessages.EMPLOYEE_NOT_FOUND);
                }
            }

            // Employee-Offering validation
            bool hasAnyOffering = await _context.EmployeeOfferings.AnyAsync(eo => eo.EmployeeId == request.EmployeeId);
            if (hasAnyOffering)
            {
                bool isAssigned = await _context.EmployeeOfferings
                    .AnyAsync(eo => eo.EmployeeId == request.EmployeeId && eo.OfferingId == request.OfferingId);

                if (!isAssigned)
                {
                    throw new BusinessException(ErrorMessages.EMPLOYEE_NOT_ASSIGNED_TO_OFFERING);
                }
            }

            DateTime endTimeUtc = request.StartTimeUtc.AddMinutes(offering.DurationInMinutes);

            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                bool isOverlapping = await _context.Appointments
                    .AnyAsync(a => a.EmployeeId == request.EmployeeId &&
                                   a.Status != AppointmentStatus.Cancelled &&
                                   a.StartTimeUtc < endTimeUtc &&
                                   a.EndTimeUtc > request.StartTimeUtc);

                if (isOverlapping)
                {
                    throw new BusinessException(ErrorMessages.APPOINTMENT_SLOT_TAKEN);
                }

                Appointment appointment = new Appointment
                {
                    BusinessId = request.BusinessId,
                    EmployeeId = request.EmployeeId,
                    OfferingId = request.OfferingId,
                    BranchId = request.BranchId,
                    UserId = userId,
                    StartTimeUtc = request.StartTimeUtc,
                    EndTimeUtc = endTimeUtc,
                    Status = AppointmentStatus.PendingApproval,
                    ActionTimeoutUtc = DateTime.UtcNow.AddHours(BusinessRules.ACTION_TIMEOUT_HOURS)
                };

                await _context.Appointments.AddAsync(appointment);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();

                _logger.LogInformation("Appointment created. AppointmentId: {AppointmentId}, UserId: {UserId}", appointment.Id, userId);

                // Fetching again or manually mapping with name if needed, but for response we can use the entity
                return MapToDto(appointment);
            }
            catch (DbUpdateException)
            {
                throw new BusinessException(ErrorMessages.APPOINTMENT_SLOT_TAKEN);
            }
        }

        public async Task<List<AppointmentResponseDto>> GetMyAsync()
        {
            Guid userId = GetRequiredUserId();

            if (_currentUserService.Role != Roles.User.ToString())
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            return await GetAppointmentsQuery(query => query.Where(a => a.UserId == userId));
        }

        public async Task<List<AppointmentResponseDto>> GetMyUpcomingAsync(int limit)
        {
            Guid userId = GetRequiredUserId();

            if (_currentUserService.Role != Roles.User.ToString())
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            if (limit < 1)
            {
                limit = BusinessRules.DEFAULT_UPCOMING_LIMIT;
            }

            if (limit > BusinessRules.MAX_UPCOMING_LIMIT)
            {
                limit = BusinessRules.MAX_UPCOMING_LIMIT;
            }

            DateTime now = DateTime.UtcNow;

            List<Appointment> results = await _context.Appointments
                .Include(a => a.Business)
                .Include(a => a.Employee)
                .Include(a => a.User)
                .Include(a => a.Offering)
                .Include(a => a.Branch)
                .Include(a => a.TransferredToEmployee)
                .AsNoTracking()
                .Where(a => a.UserId == userId
                    && a.StartTimeUtc >= now
                    && a.Status != AppointmentStatus.Cancelled
                    && a.Status != AppointmentStatus.Declined
                    && a.Status != AppointmentStatus.Completed)
                .OrderBy(a => a.StartTimeUtc)
                .Take(limit)
                .ToListAsync();

            return results.Select(MapToDto).ToList();
        }

        public async Task<List<AppointmentResponseDto>> GetBusinessAppointmentsAsync(DateTime? date)
        {
            Guid businessId = GetRequiredUserId();

            if (_currentUserService.Role != Roles.Business.ToString())
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            IQueryable<Appointment> query = _context.Appointments.Where(a => a.BusinessId == businessId);

            if (date.HasValue)
            {
                DateTime nextDay = date.Value.AddDays(1);
                query = query.Where(a => a.StartTimeUtc >= date.Value && a.StartTimeUtc < nextDay);
            }

            return await GetAppointmentsQuery(q => query);
        }

        public async Task CancelAsync(Guid appointmentId)
        {
            Appointment appointment = await GetAppointmentForAuthorizationAsync(appointmentId);
            await EnsureCanModifyAppointmentAsync(appointment, allowCustomer: true, allowAssignedEmployee: true);

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            // Notify Customer (Fire-and-forget)
            _ = SendNotificationAsync(appointment, "AppointmentCancelled");
        }

        public async Task AcceptAsync(Guid appointmentId)
        {
            Appointment app = await GetAppointmentForAuthorizationAsync(appointmentId);
            await EnsureCanModifyAppointmentAsync(app, allowCustomer: false, allowAssignedEmployee: true);
            app.Status = AppointmentStatus.Confirmed;
            app.ActionTimeoutUtc = null;
            await _context.SaveChangesAsync();

            // Notify Customer (Fire-and-forget)
            _ = SendNotificationAsync(app, "AppointmentConfirmed");
        }

        public async Task DeclineAsync(Guid appointmentId)
        {
            Appointment app = await GetAppointmentForAuthorizationAsync(appointmentId);
            await EnsureCanModifyAppointmentAsync(app, allowCustomer: false, allowAssignedEmployee: true);
            app.Status = AppointmentStatus.Declined;
            app.ActionTimeoutUtc = null;
            await _context.SaveChangesAsync();

            // Notify Customer (Fire-and-forget)
            _ = SendNotificationAsync(app, "AppointmentDeclined");
        }

        public async Task TransferAsync(Guid appointmentId, Guid newEmployeeId)
        {
            Appointment app = await GetAppointmentForAuthorizationAsync(appointmentId);
            await EnsureCanModifyAppointmentAsync(app, allowCustomer: false, allowAssignedEmployee: true);
            app.TransferredToEmployeeId = newEmployeeId;
            app.Status = AppointmentStatus.PendingTransfer;
            app.ActionTimeoutUtc = DateTime.UtcNow.AddHours(BusinessRules.ACTION_TIMEOUT_HOURS);
            await _context.SaveChangesAsync();
        }

        public async Task RespondToTransferAsync(Guid appointmentId, bool isAccepted)
        {
            Appointment app = await GetAppointmentForAuthorizationAsync(appointmentId);
            await EnsureCanModifyAppointmentAsync(app, allowCustomer: false, allowAssignedEmployee: true);
            if (isAccepted && app.TransferredToEmployeeId.HasValue)
            {
                app.EmployeeId = app.TransferredToEmployeeId.Value;
                app.Status = AppointmentStatus.Confirmed;
            }
            else
            {
                app.Status = AppointmentStatus.Declined;
            }
            app.TransferredToEmployeeId = null;
            app.ActionTimeoutUtc = null;
            await _context.SaveChangesAsync();
        }

        public async Task RequestRescheduleAsync(Guid appointmentId, DateTime newStartTimeUtc)
        {
            Appointment app = await GetAppointmentForAuthorizationAsync(appointmentId);
            await EnsureCanModifyAppointmentAsync(app, allowCustomer: true, allowAssignedEmployee: true);
            app.IsRescheduleRequested = true;
            app.ProposedRescheduleDateUtc = newStartTimeUtc;
            app.RescheduleTimeoutUtc = DateTime.UtcNow.AddHours(BusinessRules.ACTION_TIMEOUT_HOURS);
            await _context.SaveChangesAsync();
        }

        public async Task RespondToRescheduleAsync(Guid appointmentId, bool isAccepted)
        {
            Appointment app = await GetAppointmentForAuthorizationAsync(appointmentId);
            await EnsureCanModifyAppointmentAsync(app, allowCustomer: false, allowAssignedEmployee: true);
            if (isAccepted && app.ProposedRescheduleDateUtc.HasValue)
            {
                Offering? offering = await _context.Offerings.FindAsync(app.OfferingId);
                int duration = offering?.DurationInMinutes ?? BusinessRules.DEFAULT_DURATION_MINUTES;
                app.StartTimeUtc = app.ProposedRescheduleDateUtc.Value;
                app.EndTimeUtc = app.StartTimeUtc.AddMinutes(duration);
            }
            app.IsRescheduleRequested = false;
            app.ProposedRescheduleDateUtc = null;
            app.RescheduleTimeoutUtc = null;
            await _context.SaveChangesAsync();
        }

        private async Task<List<AppointmentResponseDto>> GetAppointmentsQuery(Func<IQueryable<Appointment>, IQueryable<Appointment>> filter)
        {
            IQueryable<Appointment> baseQuery = _context.Appointments
                .Include(a => a.Business)
                .Include(a => a.Employee)
                .Include(a => a.User)
                .Include(a => a.Offering)
                .Include(a => a.Branch)
                .Include(a => a.TransferredToEmployee)
                .AsNoTracking();

            baseQuery = filter(baseQuery);

            List<Appointment> results = await baseQuery
                .OrderByDescending(a => a.StartTimeUtc)
                .ToListAsync();

            return results.Select(MapToDto).ToList();
        }

        private async Task<Appointment> GetAppointmentForAuthorizationAsync(Guid appointmentId)
        {
            Appointment? appointment = await _context.Appointments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            return appointment ?? throw new NotFoundException(ErrorMessages.APPOINTMENT_NOT_FOUND);
        }

        private async Task EnsureCanModifyAppointmentAsync(
            Appointment appointment,
            bool allowCustomer,
            bool allowAssignedEmployee)
        {
            Guid currentUserId = GetRequiredUserId();
            string role = _currentUserService.Role ?? string.Empty;

            if (role == Roles.Business.ToString() && appointment.BusinessId == currentUserId)
            {
                return;
            }

            if (role == Roles.User.ToString())
            {
                if (allowCustomer && appointment.UserId == currentUserId)
                {
                    return;
                }

                if (allowAssignedEmployee && await IsAssignedEmployeeAsync(appointment, currentUserId))
                {
                    return;
                }
            }

            throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
        }

        private async Task<bool> IsAssignedEmployeeAsync(Appointment appointment, Guid currentUserId)
        {
            Guid? employeeUserId = appointment.Employee?.UserId;
            if (employeeUserId.HasValue)
            {
                return employeeUserId.Value == currentUserId;
            }

            Guid? queryUserId = await _context.Employees
                .Where(e => e.Id == appointment.EmployeeId)
                .Select(e => e.UserId)
                .FirstOrDefaultAsync();

            return queryUserId.HasValue && queryUserId.Value == currentUserId;
        }

        private async Task SendNotificationAsync(Appointment app, string type)
        {
            try
            {
                // Ensure relations are loaded
                if (app.User == null || app.Business == null || app.Offering == null)
                {
                    app = await _context.Appointments
                        .Include(a => a.User)
                        .Include(a => a.Business)
                        .Include(a => a.Offering)
                        .FirstOrDefaultAsync(a => a.Id == app.Id) ?? app;
                }

                if (app.User == null) return;

                string subject = string.Empty;
                string body = string.Empty;
                string dateStr = $"{app.StartTimeUtc:dd.MM.yyyy HH:mm}";

                switch (type)
                {
                    case "AppointmentConfirmed":
                        subject = EmailTemplates.Format(EmailTemplates.APPOINTMENT_CONFIRMED_SUBJECT, app.Business.CompanyName);
                        body = EmailTemplates.Format(EmailTemplates.APPOINTMENT_CONFIRMED_BODY, app.User.FirstName, app.Business.CompanyName, app.Offering.Name, dateStr);
                        break;
                    case "AppointmentDeclined":
                        subject = EmailTemplates.Format(EmailTemplates.APPOINTMENT_DECLINED_SUBJECT, app.Business.CompanyName);
                        body = EmailTemplates.Format(EmailTemplates.APPOINTMENT_DECLINED_BODY, app.User.FirstName, app.Business.CompanyName, app.Offering.Name, dateStr);
                        break;
                    case "AppointmentCancelled":
                        subject = EmailTemplates.Format(EmailTemplates.APPOINTMENT_CANCELLED_SUBJECT, app.Business.CompanyName);
                        body = EmailTemplates.Format(EmailTemplates.APPOINTMENT_CANCELLED_BODY, app.User.FirstName, app.Business.CompanyName, app.Offering.Name, dateStr);
                        break;
                }

                if (!string.IsNullOrEmpty(subject))
                {
                    await _emailService.SendAsync(app.User.Email, subject, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send transactional email for appointment {AppointmentId} type {Type}.", app.Id, type);
            }
        }

        private static AppointmentResponseDto MapToDto(Appointment a)
        {
            return new AppointmentResponseDto
            {
                AppointmentId = a.Id,
                BusinessId = a.BusinessId,
                BusinessName = a.Business.CompanyName,
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
                UserId = a.UserId,
                CustomerName = $"{a.User.FirstName} {a.User.LastName}",
                OfferingId = a.OfferingId,
                OfferingName = a.Offering.Name,
                BranchId = a.BranchId,
                BranchName = a.Branch?.IsDeleted == true ? "(Branch Deleted)" : a.Branch?.Name,
                StartTimeUtc = a.StartTimeUtc,
                EndTimeUtc = a.EndTimeUtc,
                Status = a.Status,
                ActionTimeoutUtc = a.ActionTimeoutUtc,
                TransferredToEmployeeId = a.TransferredToEmployeeId,
                TransferredToEmployeeName = a.TransferredToEmployee != null
                    ? $"{a.TransferredToEmployee.FirstName} {a.TransferredToEmployee.LastName}"
                    : null,
                IsRescheduleRequested = a.IsRescheduleRequested,
                ProposedRescheduleDateUtc = a.ProposedRescheduleDateUtc,
                RescheduleTimeoutUtc = a.RescheduleTimeoutUtc,
                CreatedAt = a.CreatedAt
            };
        }
    }
}
