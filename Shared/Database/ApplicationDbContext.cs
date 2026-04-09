using AppointmentSystem.Api.Shared.Models;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Modules.OfferingModule.Models;
using AppointmentSystem.Api.Modules.WorkingHourModule.Models;
using AppointmentSystem.Api.Modules.AppointmentModule.Models;
using AppointmentSystem.Api.Modules.FeedbackModule.Models;
using AppointmentSystem.Api.Modules.BranchModule.Models;
using AppointmentSystem.Api.Modules.CampaignModule.Models;
using AppointmentSystem.Api.Modules.SubscriptionModule.Models;
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
        public DbSet<Branch> Branches { get; set; }
        public DbSet<EmployeeOffering> EmployeeOfferings { get; set; }
        public DbSet<AppointmentNote> AppointmentNotes { get; set; }
        public DbSet<AppointmentMessage> AppointmentMessages { get; set; }
        public DbSet<BusinessCustomerBlock> BusinessCustomerBlocks { get; set; }
        public DbSet<BusinessCustomerEmailLog> BusinessCustomerEmailLogs { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<BusinessKnownCustomer> BusinessKnownCustomers { get; set; }
        public DbSet<PendingCustomerInvitation> PendingCustomerInvitations { get; set; }
        public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
        public DbSet<UserLoyaltyPoints> UserLoyaltyPoints { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
        public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
        public DbSet<BusinessSubscription> BusinessSubscriptions { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Global Query Filters (Override Configurations to include 100% Tenant Isolation)
            
            // Soft-delete only (Global)
            modelBuilder.Entity<Business>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<EmailVerification>().HasQueryFilter(ev => !ev.IsDeleted);

            // Multi-Tenant Isolation + Soft-delete
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || e.BusinessId == _currentUserService.TenantId.Value));
                
            modelBuilder.Entity<Offering>().HasQueryFilter(s => !s.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || s.BusinessId == _currentUserService.TenantId.Value));
                
            modelBuilder.Entity<EmployeeWorkingHour>().HasQueryFilter(w => !w.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || w.BusinessId == _currentUserService.TenantId.Value));
                
            modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || a.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<Branch>().HasQueryFilter(b => !b.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || b.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<EmployeeOffering>().HasQueryFilter(eo => !eo.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || eo.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<AppointmentNote>().HasQueryFilter(n => !n.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || n.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<AppointmentMessage>().HasQueryFilter(m => !m.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || m.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<BusinessCustomerBlock>().HasQueryFilter(b => !b.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || b.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<BusinessCustomerEmailLog>().HasQueryFilter(l => !l.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || l.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<BusinessKnownCustomer>().HasQueryFilter(c => !c.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || c.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<PendingCustomerInvitation>().HasQueryFilter(i => !i.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || i.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<LoyaltyProgram>().HasQueryFilter(lp => !lp.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || lp.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<UserLoyaltyPoints>().HasQueryFilter(ulp => !ulp.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || ulp.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<LoyaltyTransaction>().HasQueryFilter(lt => 
                (!_currentUserService.TenantId.HasValue || lt.BusinessId == _currentUserService.TenantId.Value));

            modelBuilder.Entity<BusinessSubscription>().HasQueryFilter(bs => bs.IsActive && 
                (!_currentUserService.TenantId.HasValue || bs.BusinessId == _currentUserService.TenantId.Value));
            
            modelBuilder.Entity<Campaign>().HasQueryFilter(c => !c.IsDeleted && 
                (!_currentUserService.TenantId.HasValue || c.BusinessId == _currentUserService.TenantId.Value));
        }
    }
}