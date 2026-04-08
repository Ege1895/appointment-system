using System.Collections.Generic;
using System.Threading.Tasks;
using AppointmentSystem.Api.Modules.EmployeeModule.DTOs;

namespace AppointmentSystem.Api.Modules.EmployeeModule.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> CreateEmployeesAsync(IEnumerable<CreateEmployeeRequest> requests);
        
        Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<IEnumerable<EmployeeResponseDto>> GetAllAsync(int page, int pageSize);
        Task<int> GetCountAsync();

        /// <summary>
        /// Çalışanı bir şubeye atar veya atamasını kaldırır (branchId = null).
        /// </summary>
        Task AssignBranchAsync(Guid employeeId, Guid? branchId);

        /// <summary>
        /// Belirli bir şubeye atanmış çalışanları listeler.
        /// </summary>
        Task<IEnumerable<EmployeeResponseDto>> GetByBranchAsync(Guid branchId);

        Task SetOfferingsAsync(Guid employeeId, List<Guid> offeringIds);
        Task<List<EmployeeOfferingResponseDto>> GetOfferingsAsync(Guid employeeId);
        Task<IEnumerable<EmployeeResponseDto>> GetByOfferingAsync(Guid businessId, Guid offeringId);
    }
}
