using System.Globalization;
using System.Text;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        private const int MAX_EXCEL_ROWS = 10000;
        private const int DEFAULT_DAYS_BACK = 90;
        private const int MAX_YEARS_RANGE = 2;

        public ExportService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private Guid GetBusinessId()
        {
            if (!_currentUserService.TenantId.HasValue)
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }
            return _currentUserService.TenantId.Value;
        }

        public async Task<(byte[] Content, string FileName, string ContentType)> ExportAppointmentsAsync(ExportAppointmentsRequest request)
        {
            Guid businessId = GetBusinessId();
            Business? business = await _context.Businesses.FindAsync(businessId);
            if (business == null) throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);

            // Tarih Aralığı Filtresi
            DateTime endDate = request.EndDate ?? DateTime.UtcNow;
            DateTime startDate = request.StartDate ?? endDate.AddDays(-DEFAULT_DAYS_BACK);

            if ((endDate - startDate).TotalDays > 365 * MAX_YEARS_RANGE)
            {
                throw new BusinessException("Dışa aktarma aralığı maksimum 2 yıl olabilir.");
            }

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Employee)
                .Include(a => a.Offering)
                .Include(a => a.User)
                .Include(a => a.Branch)
                .Where(a => a.BusinessId == businessId && a.StartTimeUtc >= startDate && a.StartTimeUtc <= endDate);

            if (request.Status.HasValue)
            {
                query = query.Where(a => a.Status == request.Status.Value);
            }

            if (request.BranchId.HasValue)
            {
                query = query.Where(a => a.BranchId == request.BranchId.Value);
            }

            var data = await query.OrderByDescending(a => a.StartTimeUtc)
                .Select(a => new
                {
                    AppointmentId = a.Id,
                    Date = a.StartTimeUtc.ToString("yyyy-MM-dd"),
                    StartTime = a.StartTimeUtc.ToString("HH:mm"),
                    EndTime = a.EndTimeUtc.ToString("HH:mm"),
                    CustomerName = a.User.FirstName + " " + a.User.LastName,
                    CustomerEmail = a.User.Email,
                    CustomerPhone = a.User.PhoneNumber,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    ServiceName = a.Offering.Name,
                    Price = a.PriceAtBooking,
                    Status = a.Status.ToString(),
                    BranchName = a.Branch != null ? a.Branch.Name : "-",
                    CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmm");
            string fileNameBase = $"appointments_{business.Subdomain}_{timestamp}";

            if (request.Format == ExportFormat.Excel)
            {
                byte[] content = GenerateExcel(data.Take(MAX_EXCEL_ROWS).ToList(), "Appointments");
                return (content, $"{fileNameBase}.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            else
            {
                byte[] content = GenerateCsv(data);
                return (content, $"{fileNameBase}.csv", "text/csv");
            }
        }

        public async Task<(byte[] Content, string FileName, string ContentType)> ExportCustomersAsync(ExportCustomersRequest request)
        {
            Guid businessId = GetBusinessId();
            Business? business = await _context.Businesses.FindAsync(businessId);
            if (business == null) throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);

            // Müşteri İstatistikleri
            var statsQuery = _context.Appointments
                .Where(a => a.BusinessId == businessId && !a.IsDeleted)
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalAppointments = g.Count(),
                    CompletedAppointments = g.Count(a => a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = g.Count(a => a.Status == AppointmentStatus.Cancelled || a.Status == AppointmentStatus.Declined),
                    LastVisit = g.Max(a => a.StartTimeUtc),
                    FirstVisit = g.Min(a => a.StartTimeUtc)
                });

            var customerDataBase = await (from stats in statsQuery
                                join user in _context.Users.IgnoreQueryFilters() on stats.UserId equals user.Id
                                select new
                                {
                                    CustomerId = stats.UserId,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    TotalAppointments = stats.TotalAppointments,
                                    CompletedAppointments = stats.CompletedAppointments,
                                    CancelledAppointments = stats.CancelledAppointments,
                                    LastVisitDate = stats.LastVisit,
                                    FirstVisitDate = stats.FirstVisit
                                })
                                .ToListAsync();

            var customerData = customerDataBase.Select(c => new
            {
                c.CustomerId,
                c.FirstName,
                c.LastName,
                c.Email,
                c.PhoneNumber,
                c.TotalAppointments,
                c.CompletedAppointments,
                c.CancelledAppointments,
                LastVisit = c.LastVisitDate.ToString("yyyy-MM-dd"),
                FirstVisit = c.FirstVisitDate.ToString("yyyy-MM-dd")
            }).ToList();

            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmm");
            string fileNameBase = $"customers_{business.Subdomain}_{timestamp}";

            if (request.Format == ExportFormat.Excel)
            {
                byte[] content = GenerateExcel(customerData.Take(MAX_EXCEL_ROWS).ToList(), "Customers");
                return (content, $"{fileNameBase}.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            else
            {
                byte[] content = GenerateCsv(customerData);
                return (content, $"{fileNameBase}.csv", "text/csv");
            }
        }

        private byte[] GenerateCsv<T>(IEnumerable<T> data)
        {
            using var ms = new MemoryStream();
            // UTF-8 with BOM for Excel compatibility
            using (var writer = new StreamWriter(ms, new UTF8Encoding(true)))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
            return ms.ToArray();
        }

        private byte[] GenerateExcel<T>(List<T> data, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            if (data.Any())
            {
                var table = worksheet.Cell(1, 1).InsertTable(data);
                worksheet.Columns().AdjustToContents();
                
                // Header styling
                var headerRange = worksheet.Range(1, 1, 1, table.ColumnCount());
                headerRange.Style.Font.Bold = true;
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
