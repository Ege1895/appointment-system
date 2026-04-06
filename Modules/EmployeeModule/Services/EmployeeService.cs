using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.EmployeeModule.DTOs;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public EmployeeService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private Guid GetBusinessId()
        {
            if (!_currentUserService.TenantId.HasValue)
            {
                throw new BusinessException(ErrorMessages.BUSINESS_CONTEXT_NOT_FOUND);
            }
            return _currentUserService.TenantId.Value;
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            Guid businessId = GetBusinessId();

            int currentCount = await _context.Employees.CountAsync(e => e.BusinessId == businessId);
            Business? business = await _context.Businesses
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);
            }

            if (currentCount >= business.EmployeeCapacity)
            {
                throw new BusinessException($"{ErrorMessages.CAPACITY_LIMIT_EXCEEDED} {business.EmployeeCapacity}");
            }

            bool isBusinessEmail = await _context.Businesses.AnyAsync(b => b.Email == request.Email);
            if (isBusinessEmail)
            {
                throw new BusinessException(ErrorMessages.INVALID_ROLE_ASSIGNMENT);
            }

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new NotFoundException(ErrorMessages.USER_NOT_FOUND_FOR_EMPLOYEE);
            }

            bool alreadyExists = await _context.Employees.AnyAsync(e => e.BusinessId == businessId && e.UserId == user.Id);
            if (alreadyExists)
            {
                throw new BusinessException(ErrorMessages.USER_ALREADY_EMPLOYEE);
            }

            Employee employee = new Employee
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                UserId = user.Id,
                CanManageOwnSchedule = request.CanManageOwnSchedule,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JobTitle = request.JobTitle,
                JobDescription = request.JobDescription,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            return MapToResponse(employee);
        }

        public async Task<IEnumerable<EmployeeResponseDto>> CreateEmployeesAsync(IEnumerable<CreateEmployeeRequest> requests)
        {
            Guid businessId = GetBusinessId();

            List<CreateEmployeeRequest> requestList = requests.ToList();
            int newCount = requestList.Count;
            int currentCount = await _context.Employees.CountAsync(e => e.BusinessId == businessId);

            Business? business = await _context.Businesses
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == businessId);

            if (business == null)
            {
                throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);
            }

            if (currentCount + newCount > business.EmployeeCapacity)
            {
                throw new BusinessException($"{ErrorMessages.BULK_CAPACITY_LIMIT_EXCEEDED} {business.EmployeeCapacity - currentCount}");
            }

            // Batch-load all matching users in a single query to avoid N+1
            List<string> requestedEmails = requestList.Select(r => r.Email).ToList();
            
            List<string> businessEmails = await _context.Businesses
                .Where(b => requestedEmails.Contains(b.Email))
                .Select(b => b.Email)
                .ToListAsync();

            if (businessEmails.Any())
            {
                throw new BusinessException(ErrorMessages.INVALID_ROLE_ASSIGNMENT);
            }

            Dictionary<string, User> userByEmail = await _context.Users
                .Where(u => requestedEmails.Contains(u.Email))
                .ToDictionaryAsync(u => u.Email, StringComparer.OrdinalIgnoreCase);

            List<Employee> employees = new List<Employee>();
            foreach (CreateEmployeeRequest req in requestList)
            {
                if (!userByEmail.TryGetValue(req.Email, out User? usr))
                {
                    continue; // Skip missing users in bulk
                }

                employees.Add(new Employee
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    UserId = usr.Id,
                    CanManageOwnSchedule = req.CanManageOwnSchedule,
                    FirstName = usr.FirstName,
                    LastName = usr.LastName,
                    JobTitle = req.JobTitle,
                    JobDescription = req.JobDescription,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();

            return employees.Select(MapToResponse).ToList();
        }

        public async Task<int> GetCountAsync()
        {
            Guid businessId = GetBusinessId();
            return await _context.Employees.CountAsync(e => e.BusinessId == businessId);
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync(int page, int pageSize)
        {
            Guid businessId = GetBusinessId();

            List<Employee> employees = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Branch)
                .AsNoTracking()
                .Where(e => e.BusinessId == businessId)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return employees.Select(MapToResponse).ToList();
        }

        public async Task AssignBranchAsync(Guid employeeId, Guid? branchId)
        {
            Guid businessId = GetBusinessId();

            Employee? employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.BusinessId == businessId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorMessages.EMPLOYEE_NOT_FOUND);
            }

            if (branchId.HasValue)
            {
                bool branchExists = await _context.Branches
                    .AnyAsync(b => b.Id == branchId.Value && b.BusinessId == businessId);

                if (!branchExists)
                {
                    throw new NotFoundException(ErrorMessages.BRANCH_NOT_FOUND);
                }
            }

            employee.BranchId = branchId;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetByBranchAsync(Guid branchId)
        {
            Guid businessId = GetBusinessId();

            // Şube kontrolü
            bool branchExists = await _context.Branches
                .AnyAsync(b => b.Id == branchId && b.BusinessId == businessId);

            if (!branchExists)
            {
                throw new NotFoundException(ErrorMessages.BRANCH_NOT_FOUND);
            }

            List<Employee> employees = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Branch)
                .AsNoTracking()
                .Where(e => e.BusinessId == businessId && e.BranchId == branchId)
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            return employees.Select(MapToResponse).ToList();
        }

        public async Task SetOfferingsAsync(Guid employeeId, List<Guid> offeringIds)
        {
            Guid businessId = GetBusinessId();

            // Employee belongs to business
            Employee? employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.BusinessId == businessId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorMessages.EMPLOYEE_NOT_FOUND);
            }

            // All offerings belong to business
            if (offeringIds.Any())
            {
                int validOfferingsCount = await _context.Offerings
                    .CountAsync(o => offeringIds.Contains(o.Id) && o.BusinessId == businessId);

                if (validOfferingsCount != offeringIds.Count)
                {
                    throw new NotFoundException(ErrorMessages.OFFERING_NOT_FOUND);
                }
            }

            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Delete existing assignments (not soft-delete for junction table usually, but we use BaseEntity)
                // We'll do a hard delete for junction if preferred, but plan said SetProperty is enough.
                // Actually, for junction tables in this architecture, we might want to just delete them.
                var existing = await _context.EmployeeOfferings
                    .Where(eo => eo.EmployeeId == employeeId)
                    .ToListAsync();
                
                _context.EmployeeOfferings.RemoveRange(existing);

                // Insert new assignments
                var newAssignments = offeringIds.Select(offeringId => new EmployeeOffering
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    EmployeeId = employeeId,
                    OfferingId = offeringId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.EmployeeOfferings.AddRangeAsync(newAssignments);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<EmployeeOfferingResponseDto>> GetOfferingsAsync(Guid employeeId)
        {
            Guid businessId = GetBusinessId();

            return await _context.EmployeeOfferings
                .AsNoTracking()
                .Include(eo => eo.Offering)
                .Where(eo => eo.EmployeeId == employeeId && eo.BusinessId == businessId)
                .Select(eo => new EmployeeOfferingResponseDto
                {
                    EmployeeId = eo.EmployeeId,
                    OfferingId = eo.OfferingId,
                    OfferingName = eo.Offering.Name,
                    DurationInMinutes = eo.Offering.DurationInMinutes
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetByOfferingAsync(Guid businessId, Guid offeringId)
        {
            // Offering check (Public access, so we pass businessId)
            bool offeringExists = await _context.Offerings.AnyAsync(o => o.Id == offeringId && o.BusinessId == businessId);
            if (!offeringExists)
            {
                throw new NotFoundException(ErrorMessages.OFFERING_NOT_FOUND);
            }

            // Find employees assigned to this offering
            var assignedEmployeeIds = await _context.EmployeeOfferings
                .Where(eo => eo.OfferingId == offeringId && eo.BusinessId == businessId)
                .Select(eo => eo.EmployeeId)
                .ToListAsync();

            // If no one is assigned, logic says "everyone can do it" (backward compat)
            if (!assignedEmployeeIds.Any())
            {
                return await _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Branch)
                    .AsNoTracking()
                    .Where(e => e.BusinessId == businessId)
                    .OrderBy(e => e.FirstName)
                    .Select(e => MapToResponse(e))
                    .ToListAsync();
            }

            return await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Branch)
                .AsNoTracking()
                .Where(e => assignedEmployeeIds.Contains(e.Id))
                .OrderBy(e => e.FirstName)
                .Select(e => MapToResponse(e))
                .ToListAsync();
        }

        private EmployeeResponseDto MapToResponse(Employee employee)
        {
            return new EmployeeResponseDto
            {
                EmployeeId = employee.Id,
                BusinessId = employee.BusinessId,
                UserId = employee.UserId,
                Email = employee.User?.Email,
                CanManageOwnSchedule = employee.CanManageOwnSchedule,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                JobTitle = employee.JobTitle,
                JobDescription = employee.JobDescription,
                BranchId = employee.BranchId,
                BranchName = employee.Branch?.Name,
                CreatedAt = employee.CreatedAt
            };
        }
    }
}
