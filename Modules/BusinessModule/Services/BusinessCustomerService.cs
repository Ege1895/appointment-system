using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using AppointmentSystem.Api.Shared.Notifications;
using AppointmentSystem.Api.Shared.Notifications.Templates;
using Shared.Responses;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public class BusinessCustomerService : IBusinessCustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;

        private const string DELETED_USER_NAME = "[Silinmiş Kullanıcı]";
        private const string DELETED_USER_CONTACT = "-";
        private const int MAX_MANUAL_EMAILS_PER_DAY = 10;

        public BusinessCustomerService(ApplicationDbContext context, ICurrentUserService currentUserService, IEmailService emailService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _emailService = emailService;
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

        public async Task BlockCustomerAsync(BlockCustomerRequest request)
        {
            var businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            // Kullanıcı var mı?
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                throw new KeyNotFoundException(ErrorMessages.USER_NOT_FOUND_FOR_EMPLOYEE);
            }

            // Zaten engellenmiş mi?
            var alreadyBlocked = await _context.BusinessCustomerBlocks
                .AnyAsync(b => b.BusinessId == businessId && b.UserId == request.UserId && !b.IsDeleted);

            if (alreadyBlocked)
            {
                throw new InvalidOperationException("Bu kullanıcı zaten engellenmiş.");
            }

            var block = new BusinessCustomerBlock
            {
                BusinessId = businessId,
                UserId = request.UserId,
                Reason = request.Reason,
                BlockedAt = DateTime.UtcNow
            };

            await _context.BusinessCustomerBlocks.AddAsync(block);
            await _context.SaveChangesAsync();
        }

        public async Task UnblockCustomerAsync(Guid userId)
        {
            var businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            var block = await _context.BusinessCustomerBlocks
                .FirstOrDefaultAsync(b => b.BusinessId == businessId && b.UserId == userId && !b.IsDeleted);

            if (block == null)
            {
                throw new KeyNotFoundException("Engelleme kaydı bulunamadı.");
            }

            block.IsDeleted = true;
            block.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<BlockedCustomerResponseDto>> GetBlockedCustomersAsync()
        {
            var businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            return await _context.BusinessCustomerBlocks
                .Where(b => b.BusinessId == businessId && !b.IsDeleted)
                .Select(b => new BlockedCustomerResponseDto
                {
                    UserId = b.UserId,
                    FirstName = b.User.IsDeleted ? DELETED_USER_NAME : b.User.FirstName,
                    LastName = b.User.IsDeleted ? string.Empty : b.User.LastName,
                    Email = b.User.IsDeleted ? DELETED_USER_CONTACT : b.User.Email,
                    Reason = b.Reason,
                    BlockedAt = b.BlockedAt
                })
                .ToListAsync();
        }

        public async Task<bool> IsBlockedAsync(Guid businessId, Guid userId)
        {
            return await _context.BusinessCustomerBlocks
                .AnyAsync(b => b.BusinessId == businessId && b.UserId == userId && !b.IsDeleted);
        }

        public async Task SendEmailToCustomerAsync(SendCustomerEmailRequest request)
        {
            var businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            // Sınır kontrolü (Günde 10 adet)
            var startOfToday = DateTime.UtcNow.Date;
            var sentTodayCount = await _context.BusinessCustomerEmailLogs
                .CountAsync(l => l.BusinessId == businessId && 
                                 l.UserId == request.CustomerId && 
                                 l.SentAt >= startOfToday &&
                                 !l.IsDeleted);

            if (sentTodayCount >= MAX_MANUAL_EMAILS_PER_DAY)
            {
                throw new InvalidOperationException($"Bu müşteriye günlük maksimum e-posta gönderim sınırına ({MAX_MANUAL_EMAILS_PER_DAY}) ulaştınız.");
            }

            // İşletme-Müşteri ilişkisi var mı? (En az bir randevu)
            var hasRelationship = await _context.Appointments
                .AnyAsync(a => a.BusinessId == businessId && a.UserId == request.CustomerId && !a.IsDeleted);

            if (!hasRelationship)
            {
                throw new InvalidOperationException("Sadece randevusu olan müşterilere mesaj gönderebilirsiniz.");
            }

            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == request.CustomerId);
            if (user == null || user.IsDeleted)
            {
                throw new KeyNotFoundException("Müşteri bulunamadı veya hesabı silinmiş.");
            }

            // İşletme bilgisi
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null) throw new KeyNotFoundException("İşletme bulunamadı.");

            // E-posta gönder
            var subject = EmailTemplates.Format(EmailTemplates.MANUAL_MESSAGE_SUBJECT, business.CompanyName);
            var body = EmailTemplates.Format(EmailTemplates.MANUAL_MESSAGE_BODY, user.FirstName, business.CompanyName, request.Message);

            await _emailService.SendAsync(user.Email, subject, body);

            // Log kaydet
            var log = new BusinessCustomerEmailLog
            {
                BusinessId = businessId,
                UserId = request.CustomerId,
                Subject = request.Subject,
                MessageBody = request.Message,
                SentAt = DateTime.UtcNow
            };

            await _context.BusinessCustomerEmailLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
