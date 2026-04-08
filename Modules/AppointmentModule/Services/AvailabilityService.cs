using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Modules.WorkingHourModule.Models;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;

        public AvailabilityService(ApplicationDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<List<TimeSpan>> GetAvailableSlotsAsync(Guid employeeId, DateTime date, Guid offeringId, Guid? branchId = null)
        {
            string cacheKey = string.Format(CacheKeys.AVAILABILITY_SLOTS, employeeId, date.ToString("yyyyMMdd"), offeringId, branchId?.ToString() ?? "none");

            List<TimeSpan>? cached = await _cacheService.GetAsync<List<TimeSpan>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // Employee exists and belongs to branch (if provided)
            var employeeQuery = _context.Employees.AsNoTracking().Where(e => e.Id == employeeId);
            if (branchId.HasValue)
            {
                // Unassigned employees can work in any branch. Assigned ones must match the branchId.
                employeeQuery = employeeQuery.Where(e => e.BranchId == null || e.BranchId == branchId.Value);
            }

            bool employeeValid = await employeeQuery.AnyAsync();
            if (!employeeValid)
            {
                // If branchId was provided and employee doesn't match/exist, return empty list
                return new List<TimeSpan>();
            }

            Offering? offering = await _context.Offerings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == offeringId);
            if (offering == null)
            {
                throw new NotFoundException(ErrorMessages.OFFERING_NOT_FOUND);
            }

            DayOfWeek dayOfWeek = date.DayOfWeek;

            EmployeeWorkingHour? workingHour = await _context.EmployeeWorkingHours
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.DayOfWeek == dayOfWeek);

            if (workingHour == null || workingHour.IsDayOff)
            {
                return new List<TimeSpan>();
            }

            DateTime startOfDayUtc = date.Date.ToUniversalTime();
            DateTime endOfDayUtc = startOfDayUtc.AddDays(1);

            List<Appointment> appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId &&
                            a.StartTimeUtc >= startOfDayUtc && a.StartTimeUtc < endOfDayUtc &&
                            a.Status != AppointmentStatus.Cancelled &&
                            a.Status != AppointmentStatus.Declined)
                .ToListAsync();

            List<TimeSpan> availableSlots = new List<TimeSpan>();
            TimeSpan currentSlot = workingHour.StartTime;
            TimeSpan duration = TimeSpan.FromMinutes(offering.DurationInMinutes);
            TimeSpan step = TimeSpan.FromMinutes(BusinessRules.SLOT_STEP_MINUTES);

            while (currentSlot.Add(duration) <= workingHour.EndTime)
            {
                TimeSpan slotStart = currentSlot;
                TimeSpan slotEnd = currentSlot.Add(duration);

                bool overlapsBreak = workingHour.BreakStartTime.HasValue && workingHour.BreakEndTime.HasValue &&
                                     slotStart < workingHour.BreakEndTime.Value &&
                                     slotEnd > workingHour.BreakStartTime.Value;

                if (!overlapsBreak)
                {
                    DateTime slotStartUtc = date.Date.Add(slotStart).ToUniversalTime();
                    DateTime slotEndUtc = date.Date.Add(slotEnd).ToUniversalTime();

                    bool overlapsAppointment = appointments.Any(a =>
                        a.StartTimeUtc < slotEndUtc &&
                        a.EndTimeUtc > slotStartUtc);

                    if (!overlapsAppointment)
                    {
                        availableSlots.Add(slotStart);
                    }
                }

                currentSlot = currentSlot.Add(step);
            }

            await _cacheService.SetAsync(cacheKey, availableSlots, TimeSpan.FromMinutes(BusinessRules.CACHE_AVAILABILITY_TTL_MINUTES));

            return availableSlots;
        }
    }
}
