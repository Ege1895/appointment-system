using AppointmentSystem.Api.Modules.AppointmentModule.Enums;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.CampaignModule.Services;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AppointmentSystem.Api.Modules.AppointmentModule.Workers
{
    public class AppointmentStateWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentStateWorker> _logger;
        private readonly string _instanceId;

        public AppointmentStateWorker(IServiceProvider serviceProvider, ILogger<AppointmentStateWorker> logger)
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
                    await ProcessTimeoutsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing appointment timeouts.");
                }

                await Task.Delay(TimeSpan.FromMinutes(BusinessRules.WORKER_INTERVAL_MINUTES), stoppingToken);
            }
        }

        private async Task ProcessTimeoutsAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IDistributedCache distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();

            // Distributed lock: only one instance processes timeouts at a time
            bool lockAcquired = await TryAcquireLockAsync(distributedCache, stoppingToken);
            if (!lockAcquired)
            {
                _logger.LogDebug("Appointment state worker lock not acquired — another instance is processing.");
                return;
            }

            try
            {
                ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ILoyaltyService loyaltyService = scope.ServiceProvider.GetRequiredService<ILoyaltyService>();
                await ProcessAppointmentTimeoutsAsync(context, stoppingToken);
                await ProcessAutoCompletionAsync(context, loyaltyService, stoppingToken);
            }
            finally
            {
                await ReleaseLockAsync(distributedCache, stoppingToken);
            }
        }

        private async Task<bool> TryAcquireLockAsync(IDistributedCache cache, CancellationToken stoppingToken)
        {
            byte[]? existing = await cache.GetAsync(CacheKeys.WORKER_DISTRIBUTED_LOCK, stoppingToken);
            if (existing != null)
            {
                return false;
            }

            DistributedCacheEntryOptions lockOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(BusinessRules.WORKER_LOCK_EXPIRY_MINUTES)
            };

            await cache.SetAsync(
                CacheKeys.WORKER_DISTRIBUTED_LOCK,
                Encoding.UTF8.GetBytes(_instanceId),
                lockOptions,
                stoppingToken);

            // Re-read to confirm we own the lock (simple optimistic check)
            byte[]? written = await cache.GetAsync(CacheKeys.WORKER_DISTRIBUTED_LOCK, stoppingToken);
            if (written == null)
            {
                return false;
            }

            return Encoding.UTF8.GetString(written) == _instanceId;
        }

        private async Task ReleaseLockAsync(IDistributedCache cache, CancellationToken stoppingToken)
        {
            try
            {
                byte[]? existing = await cache.GetAsync(CacheKeys.WORKER_DISTRIBUTED_LOCK, stoppingToken);
                if (existing != null && Encoding.UTF8.GetString(existing) == _instanceId)
                {
                    await cache.RemoveAsync(CacheKeys.WORKER_DISTRIBUTED_LOCK, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to release distributed lock for appointment state worker.");
            }
        }

        private async Task ProcessAppointmentTimeoutsAsync(ApplicationDbContext context, CancellationToken stoppingToken)
        {
            DateTime now = DateTime.UtcNow;

            // 1. Process Pending Approval timeouts
            List<Appointment> pendingApprovalTimeouts = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.PendingApproval && a.ActionTimeoutUtc <= now)
                .ToListAsync(stoppingToken);

            foreach (Appointment app in pendingApprovalTimeouts)
            {
                app.Status = AppointmentStatus.Declined;
                app.ActionTimeoutUtc = null;
            }

            // 2. Process Pending Transfer timeouts
            List<Appointment> pendingTransferTimeouts = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.PendingTransfer && a.ActionTimeoutUtc <= now)
                .ToListAsync(stoppingToken);

            foreach (Appointment app in pendingTransferTimeouts)
            {
                app.Status = AppointmentStatus.Declined;
                app.ActionTimeoutUtc = null;
                app.TransferredToEmployeeId = null;
            }

            // 3. Process Reschedule timeouts
            List<Appointment> rescheduleTimeouts = await context.Appointments
                .Where(a => a.IsRescheduleRequested && a.RescheduleTimeoutUtc <= now)
                .ToListAsync(stoppingToken);

            foreach (Appointment app in rescheduleTimeouts)
            {
                app.IsRescheduleRequested = false;
                app.ProposedRescheduleDateUtc = null;
                app.RescheduleTimeoutUtc = null;
            }

            if (pendingApprovalTimeouts.Any() || pendingTransferTimeouts.Any() || rescheduleTimeouts.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation(
                    "Appointment timeouts processed. Approvals: {ApprovalCount}, Transfers: {TransferCount}, Reschedules: {RescheduleCount}",
                    pendingApprovalTimeouts.Count,
                    pendingTransferTimeouts.Count,
                    rescheduleTimeouts.Count);
            }
        }

        private async Task ProcessAutoCompletionAsync(ApplicationDbContext context, ILoyaltyService loyaltyService, CancellationToken stoppingToken)
        {
            DateTime now = DateTime.UtcNow;

            // Find Confirmed appointments that have ended
            List<Appointment> completedAppointments = await context.Appointments
                .Where(a => a.Status == AppointmentStatus.Confirmed && a.EndTimeUtc <= now)
                .ToListAsync(stoppingToken);

            if (!completedAppointments.Any()) return;

            foreach (Appointment app in completedAppointments)
            {
                app.Status = AppointmentStatus.Completed;
            }

            await context.SaveChangesAsync(stoppingToken);

            foreach (Appointment app in completedAppointments)
            {
                try
                {
                    await loyaltyService.EarnPointsForAppointmentAsync(app.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to earn points for completed appointment {AppointmentId}.", app.Id);
                }
            }
        }
    }
}
