using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Notifications;
using AppointmentSystem.Api.Shared.Notifications.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Workers
{
    public class AppointmentReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderWorker> _logger;
        private readonly string _instanceId;

        private const string LOCK_KEY = "worker:appointment-reminder:lock";

        public AppointmentReminderWorker(IServiceProvider serviceProvider, ILogger<AppointmentReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _instanceId = Environment.MachineName + "_" + Guid.NewGuid().ToString("N");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing appointment reminders.");
                }

                await Task.Delay(TimeSpan.FromMinutes(BusinessRules.WORKER_INTERVAL_MINUTES), stoppingToken);
            }
        }

        private async Task ProcessRemindersAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IDistributedCache distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

            bool lockAcquired = await TryAcquireLockAsync(distributedCache, stoppingToken);
            if (!lockAcquired)
            {
                return;
            }

            try
            {
                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await SendRemindersAsync(context, emailService, stoppingToken);
            }
            finally
            {
                await ReleaseLockAsync(distributedCache, stoppingToken);
            }
        }

        private async Task<bool> TryAcquireLockAsync(IDistributedCache cache, CancellationToken stoppingToken)
        {
            byte[]? existing = await cache.GetAsync(LOCK_KEY, stoppingToken);
            if (existing != null) return false;

            var lockOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(BusinessRules.WORKER_LOCK_EXPIRY_MINUTES)
            };

            await cache.SetAsync(LOCK_KEY, Encoding.UTF8.GetBytes(_instanceId), lockOptions, stoppingToken);
            byte[]? written = await cache.GetAsync(LOCK_KEY, stoppingToken);
            return written != null && Encoding.UTF8.GetString(written) == _instanceId;
        }

        private async Task ReleaseLockAsync(IDistributedCache cache, CancellationToken stoppingToken)
        {
            try
            {
                byte[]? existing = await cache.GetAsync(LOCK_KEY, stoppingToken);
                if (existing != null && Encoding.UTF8.GetString(existing) == _instanceId)
                {
                    await cache.RemoveAsync(LOCK_KEY, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to release distributed lock for appointment reminder worker.");
            }
        }

        private async Task SendRemindersAsync(ApplicationDbContext context, IEmailService emailService, CancellationToken stoppingToken)
        {
            DateTime now = DateTime.UtcNow;
            DateTime windowStart = now.AddHours(23);
            DateTime windowEnd = now.AddHours(25);

            var upcomingAppointments = await context.Appointments
                .Include(a => a.User)
                .Include(a => a.Business)
                .Include(a => a.Offering)
                .Where(a => a.Status == AppointmentStatus.Confirmed &&
                            a.ReminderSentAt == null &&
                            a.StartTimeUtc >= windowStart &&
                            a.StartTimeUtc <= windowEnd)
                .ToListAsync(stoppingToken);

            foreach (var app in upcomingAppointments)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    string dateStr = $"{app.StartTimeUtc:dd.MM.yyyy HH:mm}";
                    string subject = EmailTemplates.Format(EmailTemplates.APPOINTMENT_REMINDER_SUBJECT, app.Business.CompanyName);
                    string body = EmailTemplates.Format(EmailTemplates.APPOINTMENT_REMINDER_BODY, app.User.FirstName, app.Business.CompanyName, app.Offering.Name, dateStr);

                    await emailService.SendAsync(app.User.Email, subject, body);

                    app.ReminderSentAt = now;
                    app.UpdatedAt = now;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for appointment {AppointmentId}.", app.Id);
                }
            }

            if (upcomingAppointments.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("{Count} appointment reminders sent.", upcomingAppointments.Count);
            }
        }
    }
}
