using AppointmentSystem.Api.Modules.AppointmentModule.DTOs;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.AppointmentModule.Services;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using AppointmentSystem.Api.Shared.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AppointmentSystem.Tests
{
    public class AppointmentServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IBusinessCustomerService> _businessCustomerServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<AppointmentService>> _loggerMock;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _businessId = Guid.NewGuid();

        public AppointmentServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _currentUserServiceMock.Setup(x => x.UserId).Returns(_userId);
            _currentUserServiceMock.Setup(x => x.Role).Returns("User");

            _businessCustomerServiceMock = new Mock<IBusinessCustomerService>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<AppointmentService>>();
        }

        [Fact]
        public async Task CreateAsync_ShouldStatusBlockedWhenCustomerIsBlocked()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, _currentUserServiceMock.Object);
            _businessCustomerServiceMock.Setup(x => x.IsBlockedAsync(_businessId, _userId)).ReturnsAsync(true);

            var service = new AppointmentService(context, _currentUserServiceMock.Object, _businessCustomerServiceMock.Object, _emailServiceMock.Object, _loggerMock.Object);

            var request = new CreateAppointmentRequest { BusinessId = _businessId, OfferingId = Guid.NewGuid(), StartTimeUtc = DateTime.UtcNow.AddDays(1) };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(() => service.CreateAsync(request));
            Assert.Equal(ErrorMessages.CUSTOMER_BLOCKED_BY_BUSINESS, ex.Message);
        }
    }
}
