using AppointmentSystem.Api.Modules.BranchModule.DTOs;
using AppointmentSystem.Api.Modules.BranchModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppointmentSystem.Api.Modules.BranchModule.Services
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public BranchService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private Guid GetBusinessId()
        {
            if (_currentUserService.Role != Roles.Business.ToString() || !_currentUserService.TenantId.HasValue)
            {
                throw new BusinessException(ErrorMessages.UNAUTHORIZED_ACCESS);
            }
            return _currentUserService.TenantId.Value;
        }

        public async Task<BranchResponseDto> CreateAsync(CreateBranchRequest request)
        {
            Guid businessId = GetBusinessId();

            // Şube sayısı kontrolü
            int branchCount = await _context.Branches.CountAsync(b => b.BusinessId == businessId);
            if (branchCount >= BusinessRules.MAX_BRANCHES_DEFAULT)
            {
                throw new BusinessException($"{ErrorMessages.BRANCH_LIMIT_EXCEEDED} {BusinessRules.MAX_BRANCHES_DEFAULT}");
            }

            // İlk şube ise otomatik olarak ana şube yap
            bool isMain = branchCount == 0 || request.IsMain;

            Branch branch = new Branch
            {
                BusinessId = businessId,
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                District = request.District,
                PhoneNumber = request.PhoneNumber,
                IsMain = isMain,
                IsActive = true
            };

            // Eğer yeni şube ana şube olarak işaretlendiyse, diğerlerini ana şube olmaktan çıkar
            if (isMain && branchCount > 0)
            {
                using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.Branches
                        .Where(b => b.BusinessId == businessId && b.IsMain)
                        .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsMain, false));

                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();
            }

            return MapToResponse(branch);
        }

        public async Task<List<BranchResponseDto>> GetAllAsync()
        {
            Guid businessId = GetBusinessId();
            List<Branch> branches = await _context.Branches
                .Where(b => b.BusinessId == businessId)
                .OrderByDescending(b => b.IsMain)
                .ThenBy(b => b.Name)
                .ToListAsync();

            return branches.Select(MapToResponse).ToList();
        }

        public async Task<BranchResponseDto> GetByIdAsync(Guid branchId)
        {
            Guid businessId = GetBusinessId();
            Branch? branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId && b.BusinessId == businessId);

            if (branch == null)
            {
                throw new NotFoundException(ErrorMessages.BRANCH_NOT_FOUND);
            }

            return MapToResponse(branch);
        }

        public async Task UpdateAsync(Guid branchId, UpdateBranchRequest request)
        {
            Guid businessId = GetBusinessId();
            Branch? branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId && b.BusinessId == businessId);

            if (branch == null)
            {
                throw new NotFoundException(ErrorMessages.BRANCH_NOT_FOUND);
            }

            branch.Name = request.Name;
            branch.Address = request.Address;
            branch.City = request.City;
            branch.District = request.District;
            branch.PhoneNumber = request.PhoneNumber;
            branch.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid branchId)
        {
            Guid businessId = GetBusinessId();
            Branch? branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branchId && b.BusinessId == businessId);

            if (branch == null)
            {
                throw new NotFoundException(ErrorMessages.BRANCH_NOT_FOUND);
            }

            // Ana şube silinemez kısıtlaması (başka şubeler varsa)
            if (branch.IsMain)
            {
                bool hasOtherBranches = await _context.Branches
                    .AnyAsync(b => b.BusinessId == businessId && b.Id != branchId);
                
                if (hasOtherBranches)
                {
                    throw new BusinessException(ErrorMessages.MAIN_BRANCH_DELETE_ERROR);
                }
            }

            branch.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task SetMainBranchAsync(Guid branchId)
        {
            Guid businessId = GetBusinessId();
            
            // Hedef şube var mı kontrolü
            bool exists = await _context.Branches
                .AnyAsync(b => b.Id == branchId && b.BusinessId == businessId);
            
            if (!exists)
            {
                throw new NotFoundException(ErrorMessages.BRANCH_NOT_FOUND);
            }

            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Mevcut ana şubeleri pasife çek
                await _context.Branches
                    .Where(b => b.BusinessId == businessId && b.IsMain)
                    .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsMain, false));

                // Yeni ana şubeyi ata
                await _context.Branches
                    .Where(b => b.Id == branchId)
                    .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsMain, true));

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private BranchResponseDto MapToResponse(Branch branch)
        {
            return new BranchResponseDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                City = branch.City,
                District = branch.District,
                PhoneNumber = branch.PhoneNumber,
                IsMain = branch.IsMain,
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt
            };
        }
    }
}
