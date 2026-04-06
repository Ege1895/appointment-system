using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.ReportingModule.DTOs;
using AppointmentSystem.Api.Modules.ReportingModule.Services;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Caching;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppointmentSystem.Tests
{
    public class ReportingServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Guid _tenantId = Guid.NewGuid();

        public ReportingServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _currentUserServiceMock.Setup(x => x.TenantId).Returns(_tenantId);

            _cacheServiceMock = new Mock<ICacheService>();
        }

        [Fact]
        public async Task GetAppointmentCountReportAsync_ShouldReturnZeroForEmptyBuckets()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, _currentUserServiceMock.Object);
            DateTime start = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc);

            context.Appointments.Add(new Appointment { BusinessId = _tenantId, UserId = Guid.NewGuid(), Status = AppointmentStatus.Confirmed, StartTimeUtc = start.AddHours(1) });
            await context.SaveChangesAsync();

            var service = new ReportingService(context, _currentUserServiceMock.Object, _cacheServiceMock.Object);

            var request = new AppointmentCountReportRequest { Period = ReportPeriod.Daily, StartDate = start, EndDate = end };

            // Act
            var response = await service.GetAppointmentCountReportAsync(request);

            // Assert
            Assert.Equal(3, response.Buckets.Count);
            Assert.Equal(1, response.Buckets[0].Total);
            Assert.Equal(0, response.Buckets[1].Total);
        }
    }
}
