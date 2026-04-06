using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public class BusinessCustomerService : IBusinessCustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        private const string DELETED_USER_NAME = "[Silinmiş Kullanıcı]";
        private const string DELETED_USER_CONTACT = "-";

        public BusinessCustomerService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<(IReadOnlyList<BusinessCustomerSummaryDto> Items, PaginationMeta Meta)> GetCustomersAsync(GetCustomersQuery query)
        {
            query.Normalize();

            // Sadece bu işletmeye ait randevuları alıyoruz (Global Query Filter zaten BusinessId kontrolü yapıyor)
            // Ancak istatistikler için UserId bazlı gruplama yapacağız.
            var baseQuery = _context.Appointments
                .Where(a => !a.IsDeleted);

            var groupedQuery = baseQuery
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(a => a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = g.Count(a => a.Status == AppointmentStatus.Cancelled || a.Status == AppointmentStatus.Declined),
                    LastAppointmentDate = g.Max(a => a.StartTimeUtc),
                    FirstAppointmentDate = g.Min(a => a.StartTimeUtc)
                });

            // Kullanıcı bilgilerini JOIN ile ekliyoruz (Silinmiş kullanıcıları da görmek için IgnoreQueryFilters kullanıyoruz)
            var customerQuery = from stats in groupedQuery
                                join user in _context.Users.IgnoreQueryFilters() on stats.UserId equals user.Id
                                select new BusinessCustomerSummaryDto
                                {
                                    UserId = stats.UserId,
                                    FirstName = user.IsDeleted ? DELETED_USER_NAME : user.FirstName,
                                    LastName = user.IsDeleted ? string.Empty : user.LastName,
                                    Email = user.IsDeleted ? DELETED_USER_CONTACT : user.Email,
                                    PhoneNumber = user.IsDeleted ? DELETED_USER_CONTACT : user.PhoneNumber,
                                    TotalAppointments = stats.TotalAppointments,
                                    CompletedAppointments = stats.CompletedAppointments,
                                    CancelledAppointments = stats.CancelledAppointments,
                                    LastAppointmentDate = stats.LastAppointmentDate,
                                    FirstAppointmentDate = stats.FirstAppointmentDate
                                };

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                customerQuery = customerQuery.Where(c =>
                    c.FirstName.ToLower().Contains(search) ||
                    c.LastName.ToLower().Contains(search) ||
                    c.Email.ToLower().Contains(search));
            }

            // Sıralama
            customerQuery = query.SortBy?.ToLower() switch
            {
                "totalappointments" => query.SortDirection == "desc" ? customerQuery.OrderByDescending(c => c.TotalAppointments) : customerQuery.OrderBy(c => c.TotalAppointments),
                "lastappointmentdate" => query.SortDirection == "desc" ? customerQuery.OrderByDescending(c => c.LastAppointmentDate) : customerQuery.OrderBy(c => c.LastAppointmentDate),
                "name" => query.SortDirection == "desc" ? customerQuery.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName) : customerQuery.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
                _ => customerQuery.OrderByDescending(c => c.LastAppointmentDate)
            };

            var totalCount = await customerQuery.CountAsync();
            var items = await customerQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, PaginationMeta.Create(query.Page, query.PageSize, totalCount));
        }

        public async Task<BusinessCustomerDetailDto> GetCustomerDetailAsync(Guid customerId)
        {
            var businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            // Müşterinin bu işletmedeki randevu istatistikleri
            var stats = await _context.Appointments
                .Where(a => a.UserId == customerId && !a.IsDeleted)
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(a => a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = g.Count(a => a.Status == AppointmentStatus.Cancelled || a.Status == AppointmentStatus.Declined),
                    LastAppointmentDate = g.Max(a => a.StartTimeUtc),
                    FirstAppointmentDate = g.Min(a => a.StartTimeUtc)
                })
                .FirstOrDefaultAsync();

            if (stats == null)
            {
                throw new KeyNotFoundException("Müşteri bulunamadı veya bu işletme ile randevusu yok.");
            }

            // Kullanıcı bilgileri
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == customerId);
            if (user == null)
            {
                throw new KeyNotFoundException("Kullanıcı kaydı bulunamadı.");
            }

            // Son 10 randevu
            var recentAppointments = await _context.Appointments
                .Where(a => a.UserId == customerId && !a.IsDeleted)
                .OrderByDescending(a => a.StartTimeUtc)
                .Take(10)
                .Select(a => new AppointmentResponseDto
                {
                    AppointmentId = a.Id,
                    BusinessId = a.BusinessId,
                    BusinessName = a.Business.CompanyName,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    UserId = a.UserId,
                    CustomerName = user.IsDeleted ? DELETED_USER_NAME : user.FirstName + " " + user.LastName,
                    OfferingId = a.OfferingId,
                    OfferingName = a.Offering.Name,
                    StartTimeUtc = a.StartTimeUtc,
                    EndTimeUtc = a.EndTimeUtc,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    BranchId = a.BranchId,
                    BranchName = a.Branch != null ? a.Branch.Name : null
                })
                .ToListAsync();

            return new BusinessCustomerDetailDto
            {
                UserId = user.Id,
                FirstName = user.IsDeleted ? DELETED_USER_NAME : user.FirstName,
                LastName = user.IsDeleted ? string.Empty : user.LastName,
                Email = user.IsDeleted ? DELETED_USER_CONTACT : user.Email,
                PhoneNumber = user.IsDeleted ? DELETED_USER_CONTACT : user.PhoneNumber,
                TotalAppointments = stats.TotalAppointments,
                CompletedAppointments = stats.CompletedAppointments,
                CancelledAppointments = stats.CancelledAppointments,
                LastAppointmentDate = stats.LastAppointmentDate,
                FirstAppointmentDate = stats.FirstAppointmentDate,
                RecentAppointments = recentAppointments
            };
        }
    }
}
