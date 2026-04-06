using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Services;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Notifications;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppointmentSystem.Tests
{
    public class BusinessCustomerServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Guid _tenantId = Guid.NewGuid();

        public BusinessCustomerServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _currentUserServiceMock.Setup(x => x.TenantId).Returns(_tenantId);

            _emailServiceMock = new Mock<IEmailService>();
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldGroupAppointmentsByUserId()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, _currentUserServiceMock.Object);
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FirstName = "Test", LastName = "User", Email = "test@test.com" };
            
            context.Users.Add(user);
            context.Appointments.AddRange(new List<Appointment>
            {
                new Appointment { BusinessId = _tenantId, UserId = userId, Status = AppointmentStatus.Completed, StartTimeUtc = DateTime.UtcNow.AddDays(-1) },
                new Appointment { BusinessId = _tenantId, UserId = userId, Status = AppointmentStatus.Cancelled, StartTimeUtc = DateTime.UtcNow.AddDays(-2) }
            });
            await context.SaveChangesAsync();

            var service = new BusinessCustomerService(context, _currentUserServiceMock.Object, _emailServiceMock.Object);

            // Act
            var (items, meta) = await service.GetCustomersAsync(new GetCustomersQuery { Page = 1, PageSize = 10 });

            // Assert
            Assert.Single(items);
            Assert.Equal(2, items[0].TotalAppointments);
        }

        [Fact]
        public async Task SendEmailToCustomerAsync_ShouldThrowWhenDailyLimitExceeded()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, _currentUserServiceMock.Object);
            var customerId = Guid.NewGuid();
            
            // Add existing relationship
            context.Appointments.Add(new Appointment { BusinessId = _tenantId, UserId = customerId, Status = AppointmentStatus.Completed, StartTimeUtc = DateTime.UtcNow });

            // Add 10 logs
            for (int i = 0; i < 10; i++)
            {
                context.BusinessCustomerEmailLogs.Add(new BusinessCustomerEmailLog { BusinessId = _tenantId, UserId = customerId, SentAt = DateTime.UtcNow });
            }
            await context.SaveChangesAsync();

            var service = new BusinessCustomerService(context, _currentUserServiceMock.Object, _emailServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                service.SendEmailToCustomerAsync(new SendCustomerEmailRequest { CustomerId = customerId, Subject = "Test", Message = "Msg" }));
        }
    }
}
