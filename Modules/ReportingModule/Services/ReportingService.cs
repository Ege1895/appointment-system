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

        public async Task<RevenueReportResponseDto> GetRevenueReportAsync(RevenueReportRequest request)
        {
            Guid businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            // 1. Validasyon (Ortak validasyon mantığı kullanılabilir)
            ValidateRevenueRequest(request);

            // 2. Cache Kontrolü
            string cacheKey = string.Format("report:revenue:{0}:{1}:{2:yyyyMMdd}:{3:yyyyMMdd}:{4}",
                businessId, request.Period, request.StartDate, request.EndDate, request.BranchId ?? Guid.Empty);

            RevenueReportResponseDto? cachedData = await _cacheService.GetAsync<RevenueReportResponseDto>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            // 3. İşletme bilgilerini al (Para birimi için)
            var business = await _context.Businesses
                .Where(b => b.Id == businessId)
                .Select(b => new { b.CurrencyCode })
                .FirstOrDefaultAsync();

            string currencyCode = business?.CurrencyCode ?? AppointmentSystem.Api.Shared.Constants.BusinessRules.DEFAULT_CURRENCY;

            // 4. Bucket'ları oluştur
            List<RevenueBucketDto> buckets = GenerateRevenueBuckets(request.Period, request.StartDate, request.EndDate);

            // 5. Veritabanından veriyi çek
            var query = _context.Appointments
                .Include(a => a.Offering)
                .Where(a => a.BusinessId == businessId &&
                            a.StartTimeUtc >= request.StartDate &&
                            a.StartTimeUtc <= request.EndDate &&
                            a.Status == AppointmentStatus.Completed &&
                            !a.IsDeleted);

            if (request.BranchId.HasValue)
            {
                query = query.Where(a => a.BranchId == request.BranchId.Value);
            }

            // Fiyatlandırma tipi Fixed olanları filtrele (Free veya Variable olanları hesaplamaya katmıyoruz)
            var rawData = await query
                .Select(a => new
                {
                    a.StartTimeUtc,
                    a.OfferingId,
                    OfferingName = a.Offering.Name,
                    a.Offering.PriceType,
                    // Deesign Decision: PriceAtBooking varsa onu kullan, yoksa güncel offering fiyatını kullan
                    Price = a.PriceAtBooking ?? (a.Offering.Price ?? 0)
                })
                .Where(x => x.PriceType == OfferingModule.Enums.PriceType.Fixed)
                .ToListAsync();

            // 6. Verileri işle
            List<RevenueByServiceDto> serviceStats = new List<RevenueByServiceDto>();

            foreach (var item in rawData)
            {
                // Bucket güncelleme
                RevenueBucketDto? bucket = FindRevenueBucket(buckets, request.Period, item.StartTimeUtc);
                if (bucket != null)
                {
                    bucket.TotalRevenue += item.Price;
                    bucket.AppointmentCount++;
                }

                // Servis istatistiği güncelleme
                RevenueByServiceDto? sStat = serviceStats.FirstOrDefault(s => s.OfferingId == item.OfferingId);
                if (sStat == null)
                {
                    sStat = new RevenueByServiceDto
                    {
                        OfferingId = item.OfferingId,
                        OfferingName = item.OfferingName,
                        TotalRevenue = 0,
                        CompletedCount = 0
                    };
                    serviceStats.Add(sStat);
                }
                sStat.TotalRevenue += item.Price;
                sStat.CompletedCount++;
            }

            // Sonuçları hazırla
            RevenueReportResponseDto response = new RevenueReportResponseDto
            {
                TotalRevenue = buckets.Sum(b => b.TotalRevenue),
                TotalCompletedAppointments = buckets.Sum(b => b.AppointmentCount),
                CurrencyCode = currencyCode,
                Buckets = buckets,
                TopServices = serviceStats.OrderByDescending(s => s.TotalRevenue).Take(10).ToList(),
                HasPricedOfferings = await _context.Offerings.AnyAsync(o => o.BusinessId == businessId && o.PriceType == OfferingModule.Enums.PriceType.Fixed && !o.IsDeleted)
            };

            // 7. Önbelleğe al (15 dakika)
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

            return response;
        }

        private void ValidateRevenueRequest(RevenueReportRequest request)
        {
            if (request.StartDate >= request.EndDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");

            TimeSpan diff = request.EndDate - request.StartDate;

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

        private List<RevenueBucketDto> GenerateRevenueBuckets(ReportPeriod period, DateTime start, DateTime end)
        {
            List<RevenueBucketDto> result = new List<RevenueBucketDto>();
            DateTime current = start;

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
                string label = period switch
                {
                    ReportPeriod.Daily => current.ToString("yyyy-MM-dd"),
                    ReportPeriod.Weekly => string.Format("{0}-W{1:D2}", current.Year, GetIsoWeekOfYear(current)),
                    ReportPeriod.Monthly => current.ToString("yyyy-MM"),
                    ReportPeriod.Yearly => current.ToString("yyyy"),
                    _ => current.ToString()
                };

                result.Add(new RevenueBucketDto
                {
                    PeriodLabel = label,
                    PeriodStart = current,
                    TotalRevenue = 0,
                    AppointmentCount = 0
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

        private RevenueBucketDto? FindRevenueBucket(List<RevenueBucketDto> buckets, ReportPeriod period, DateTime date)
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

        public async Task<BranchStatsReportResponseDto> GetBranchStatsAsync(BranchStatsRequest request)
        {
            Guid businessId = _currentUserService.TenantId ?? throw new UnauthorizedAccessException();

            // 1. Validasyon (Max 2 yıl)
            if (request.StartDate >= request.EndDate)
                throw new ArgumentException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");

            if ((request.EndDate - request.StartDate).TotalDays > 365 * 2 + 1)
                throw new ArgumentException("Maksimum 2 yıllık tarih aralığı seçilebilir.");

            // 2. Cache Kontrolü
            string cacheKey = string.Format("report:branches:{0}:{1:yyyyMMdd}:{2:yyyyMMdd}",
                businessId, request.StartDate, request.EndDate);

            BranchStatsReportResponseDto? cachedData = await _cacheService.GetAsync<BranchStatsReportResponseDto>(cacheKey);
            if (cachedData != null) return cachedData;

            // 3. Şube Listesi (Silinenler dahil - IgnoreQueryFilters)
            var allBranches = await _context.Branches
                .IgnoreQueryFilters()
                .Where(b => b.BusinessId == businessId)
                .Select(b => new { b.Id, b.Name, b.IsMain })
                .ToListAsync();

            // 4. Randevuları Çek
            var appointments = await _context.Appointments
                .Include(a => a.Offering)
                .Where(a => a.BusinessId == businessId &&
                            a.StartTimeUtc >= request.StartDate &&
                            a.StartTimeUtc <= request.EndDate &&
                            !a.IsDeleted)
                .Select(a => new
                {
                    a.BranchId,
                    a.Status,
                    a.EmployeeId,
                    a.StartTimeUtc,
                    Price = a.PriceAtBooking ?? (a.Offering.Price ?? 0),
                    a.Offering.PriceType
                })
                .ToListAsync();

            List<BranchStatDto> branchStats = new List<BranchStatDto>();

            // Mevcut (veya silinmiş) tüm şubeler için istatistikleri oluştur
            foreach (var branch in allBranches)
            {
                var branchApps = appointments.Where(a => a.BranchId == branch.Id).ToList();
                branchStats.Add(MapToBranchStatDto(branch.Id, branch.Name, branch.IsMain, branchApps));
            }

            // Atanmamış (Unassigned) randevular varsa ekle
            var unassignedApps = appointments.Where(a => a.BranchId == null).ToList();
            if (unassignedApps.Any())
            {
                branchStats.Add(MapToBranchStatDto(null, "Unassigned", false, unassignedApps));
            }

            // Sonuçları Sırala ve Paketle
            var orderedBranches = branchStats.OrderByDescending(b => b.TotalAppointments).ToList();

            var response = new BranchStatsReportResponseDto
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Branches = orderedBranches,
                TotalAcrossAllBranches = appointments.Count,
                BusiestBranchId = orderedBranches.FirstOrDefault()?.BranchId
            };

            // 5. Önbelleğe al (15 dakika)
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

            return response;
        }

        private BranchStatDto MapToBranchStatDto(Guid? branchId, string branchName, bool isMain, List<dynamic> apps)
        {
            var completedApps = apps.Where(a => a.Status == AppointmentStatus.Completed).ToList();

            return new BranchStatDto
            {
                BranchId = branchId,
                BranchName = branchName,
                IsMain = isMain,
                TotalAppointments = apps.Count,
                CompletedAppointments = completedApps.Count,
                CancelledAppointments = apps.Count(a => a.Status == AppointmentStatus.Cancelled),
                TotalRevenue = completedApps
                    .Where(a => a.PriceType == OfferingModule.Enums.PriceType.Fixed)
                    .Sum(a => (decimal)a.Price),
                ActiveEmployees = apps.Select(a => a.EmployeeId).Distinct().Count(),
                BusiestDayOfWeek = GetBusiestDay(apps.Select(a => (DateTime)a.StartTimeUtc).ToList())
            };
        }

        private string? GetBusiestDay(List<DateTime> dates)
        {
            if (!dates.Any()) return null;

            return dates
                .GroupBy(d => d.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .First()
                .Key.ToString();
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
