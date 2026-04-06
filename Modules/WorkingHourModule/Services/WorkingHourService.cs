using AppointmentSystem.Api.Modules.WorkingHourModule.DTOs;
using AppointmentSystem.Api.Modules.WorkingHourModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.WorkingHourModule.Services
{
    public class WorkingHourService : IWorkingHourService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public WorkingHourService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task SetWorkingHoursAsync(Guid employeeId, List<WorkingHourDto> workingHours)
        {
            Employee? employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorMessages.EMPLOYEE_NOT_FOUND);
            }

            EnsureWorkingHourAccess(employee);

            List<EmployeeWorkingHour> existingHours = await _context.EmployeeWorkingHours
                .Where(w => w.EmployeeId == employeeId)
                .ToListAsync();

            _context.EmployeeWorkingHours.RemoveRange(existingHours);

            IEnumerable<EmployeeWorkingHour> newHours = workingHours.Select(dto => new EmployeeWorkingHour
            {
                BusinessId = employee.BusinessId,
                EmployeeId = employeeId,
                DayOfWeek = dto.DayOfWeek,
                IsDayOff = dto.IsDayOff,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                BreakStartTime = dto.BreakStartTime,
                BreakEndTime = dto.BreakEndTime
            });

            await _context.EmployeeWorkingHours.AddRangeAsync(newHours);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkingHourDto>> GetWorkingHoursAsync(Guid employeeId)
        {
            Employee? employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorMessages.EMPLOYEE_NOT_FOUND);
            }

            EnsureWorkingHourAccess(employee);

            List<EmployeeWorkingHour> hours = await _context.EmployeeWorkingHours
                .AsNoTracking()
                .Where(w => w.EmployeeId == employeeId)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();

            return hours.Select(h => new WorkingHourDto
            {
                DayOfWeek = h.DayOfWeek,
                IsDayOff = h.IsDayOff,
                StartTime = h.StartTime,
                EndTime = h.EndTime,
                BreakStartTime = h.BreakStartTime,
                BreakEndTime = h.BreakEndTime
            }).ToList();
        }

        private void EnsureWorkingHourAccess(Employee employee)
        {
            string? userIdStr = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid currentUserId))
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }

            string? role = _currentUserService.Role;

            bool isAuthorized =
                (role == Roles.Business.ToString() && employee.BusinessId == currentUserId) ||
                (role == Roles.User.ToString() && employee.UserId == currentUserId && employee.CanManageOwnSchedule);

            if (!isAuthorized)
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }
        }
    }
}
