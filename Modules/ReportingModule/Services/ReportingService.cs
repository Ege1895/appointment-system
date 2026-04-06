using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.ReportingModule.DTOs;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentSystem.Api.Modules.ReportingModule.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICacheService _cacheService;

        public ReportingService(ApplicationDbContext context, ICurrentUserService currentUserService, ICacheService cacheService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _cacheService = cacheService;
        }

        public async Task<AppointmentCountReportResponseDto> GetAppointmentCountReportAsync(AppointmentCountReportRequest request)
        {
            var businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            // 1. Validasyon
            ValidateRequest(request);

            // 2. Cache Kontrolü
            string cacheKey = string.Format("report:count:{0}:{1}:{2:yyyyMMdd}:{3:yyyyMMdd}:{4}",
                businessId, request.Period, request.StartDate, request.EndDate, request.BranchId ?? Guid.Empty);

            var cachedData = await _cacheService.GetAsync<AppointmentCountReportResponseDto>(cacheKey);
            if (cachedData != null) return cachedData;

            // 3. Bucket'ları oluştur (Boşluk doldurma için)
            var buckets = GenerateBuckets(request.Period, request.StartDate, request.EndDate);

            // 4. Veritabanından veriyi çek (Gruplanmış)
            var query = _context.Appointments
                .Where(a => a.BusinessId == businessId && 
                            a.StartTimeUtc >= request.StartDate && 
                            a.StartTimeUtc <= request.EndDate && 
                            !a.IsDeleted);

            if (request.BranchId.HasValue)
            {
                query = query.Where(a => a.BranchId == request.BranchId.Value);
            }

            // DB'de gruplama yapıyoruz
            // Not: date_trunc kullanımı için SQL tarafına geçiyoruz
            var rawData = await query
                .Select(a => new { a.StartTimeUtc, a.Status })
                .ToListAsync();

            // 5. Verileri bucket'lara dağıt
            foreach (var item in rawData)
            {
                var bucket = FindBucket(buckets, request.Period, item.StartTimeUtc);
                if (bucket != null)
                {
                    bucket.Total++;
                    switch (item.Status)
                    {
                        case AppointmentStatus.Confirmed: bucket.Confirmed++; break;
                        case AppointmentStatus.Completed: bucket.Completed++; break;
                        case AppointmentStatus.Cancelled: bucket.Cancelled++; break;
                        case AppointmentStatus.Declined: bucket.Declined++; break;
                        case AppointmentStatus.PendingApproval: bucket.PendingApproval++; break;
                    }
                }
            }

            var response = new AppointmentCountReportResponseDto
            {
                BusinessId = businessId,
                Period = request.Period.ToString(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalInRange = buckets.Sum(b => b.Total),
                Buckets = buckets
            };

            // 6. Önbelleğe al (15 dakika)
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

            return response;
        }

        private void ValidateRequest(AppointmentCountReportRequest request)
        {
            if (request.StartDate >= request.EndDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");

            var diff = request.EndDate - request.StartDate;

            switch (request.Period)
            {
                case ReportPeriod.Daily:
                    if (diff.TotalDays > 90) throw new ArgumentException("Günlük rapor için maksimum 90 gün seçilebilir.");
                    break;
                case ReportPeriod.Weekly:
                    if (diff.TotalDays > 366) throw new ArgumentException("Haftalık rapor için maksimum 1 yıl seçilebilir.");
                    break;
                case ReportPeriod.Monthly:
                    if (diff.TotalDays > 31 * 24) throw new ArgumentException("Aylık rapor için maksimum 2 yıl seçilebilir.");
                    break;
                case ReportPeriod.Yearly:
                    if (diff.TotalDays > 365 * 5 + 2) throw new ArgumentException("Yıllık rapor için maksimum 5 yıl seçilebilir.");
                    break;
            }
        }

        private List<AppointmentCountBucketDto> GenerateBuckets(ReportPeriod period, DateTime start, DateTime end)
        {
            var result = new List<AppointmentCountBucketDto>();
            DateTime current = start;

            // Start tarihini periodun başına yuvarla
            current = period switch
            {
                ReportPeriod.Daily => current.Date,
                ReportPeriod.Weekly => GetStartOfWeek(current),
                ReportPeriod.Monthly => new DateTime(current.Year, current.Month, 1),
                ReportPeriod.Yearly => new DateTime(current.Year, 1, 1),
                _ => current
            };

            while (current <= end)
            {
                var label = period switch
                {
                    ReportPeriod.Daily => current.ToString("yyyy-MM-dd"),
                    ReportPeriod.Weekly => string.Format("{0}-W{1:D2}", current.Year, GetIsoWeekOfYear(current)),
                    ReportPeriod.Monthly => current.ToString("yyyy-MM"),
                    ReportPeriod.Yearly => current.ToString("yyyy"),
                    _ => current.ToString()
                };

                result.Add(new AppointmentCountBucketDto
                {
                    PeriodLabel = label,
                    PeriodStart = current
                });

                current = period switch
                {
                    ReportPeriod.Daily => current.AddDays(1),
                    ReportPeriod.Weekly => current.AddDays(7),
                    ReportPeriod.Monthly => current.AddMonths(1),
                    ReportPeriod.Yearly => current.AddYears(1),
                    _ => current.AddDays(1)
                };
            }

            return result;
        }

        private AppointmentCountBucketDto? FindBucket(List<AppointmentCountBucketDto> buckets, ReportPeriod period, DateTime date)
        {
            return period switch
            {
                ReportPeriod.Daily => buckets.FirstOrDefault(b => b.PeriodStart.Date == date.Date),
                ReportPeriod.Weekly => buckets.FirstOrDefault(b => b.PeriodStart <= date && date < b.PeriodStart.AddDays(7)),
                ReportPeriod.Monthly => buckets.FirstOrDefault(b => b.PeriodStart.Year == date.Year && b.PeriodStart.Month == date.Month),
                ReportPeriod.Yearly => buckets.FirstOrDefault(b => b.PeriodStart.Year == date.Year),
                _ => null
            };
        }

        private DateTime GetStartOfWeek(DateTime dt)
        {
            int diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        private int GetIsoWeekOfYear(DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}
