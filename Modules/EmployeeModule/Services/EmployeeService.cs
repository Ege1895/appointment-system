using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.EmployeeModule.DTOs;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

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
            string? userIdString = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid businessId))
            {
                throw new BusinessException(ErrorMessages.BUSINESS_CONTEXT_NOT_FOUND);
            }
            return businessId;
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
                .AsNoTracking()
                .Where(e => e.BusinessId == businessId)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return employees.Select(MapToResponse).ToList();
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
                CreatedAt = employee.CreatedAt
            };
        }
    }
}
