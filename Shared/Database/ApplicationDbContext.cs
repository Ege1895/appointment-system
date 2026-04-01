using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Modules.WorkingHourModule.Models;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.FeedbackModule.Models;
using Microsoft.EntityFrameworkCore;

using AppointmentSystem.Api.Shared.Authentication;

namespace AppointmentSystem.Api.Shared.Database
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Business> Businesses { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Offering> Offerings { get; set; }
        public DbSet<EmployeeWorkingHour> EmployeeWorkingHours { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Global Query Filters (Override Configurations to include 100% Tenant Isolation)
            
            // Soft-delete only (Global)
            modelBuilder.Entity<Business>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

            // Multi-Tenant Isolation + Soft-delete
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || e.BusinessId == _currentUserService.TenantId.Value));
                
            modelBuilder.Entity<Offering>().HasQueryFilter(s => !s.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || s.BusinessId == _currentUserService.TenantId.Value));
                
            modelBuilder.Entity<EmployeeWorkingHour>().HasQueryFilter(w => !w.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || w.BusinessId == _currentUserService.TenantId.Value));
                
            modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || a.BusinessId == _currentUserService.TenantId.Value));
        }
    }
}