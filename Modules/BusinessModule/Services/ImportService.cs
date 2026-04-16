using System.Globalization;
using AppointmentSystem.Api.Modules.BusinessModule.DTOs;
using AppointmentSystem.Api.Modules.BusinessModule.Models;
using AppointmentSystem.Api.Modules.EmployeeModule.Models;
using AppointmentSystem.Api.Modules.UserModule.Models;
using AppointmentSystem.Api.Shared.Authentication;
using AppointmentSystem.Api.Shared.Constants;
using AppointmentSystem.Api.Shared.Database;
using AppointmentSystem.Api.Shared.Exceptions;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Api.Modules.BusinessModule.Services
{
    public class ImportService : IImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        private const int BATCH_SIZE = 100;

        public ImportService(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private Guid GetBusinessId()
        {
            return _currentUserService.TenantId ?? throw new BusinessException(ErrorMessages.BUSINESS_CONTEXT_NOT_FOUND);
        }

        public async Task<ImportResultDto> ImportEmployeesAsync(IFormFile file)
        {
            Guid businessId = GetBusinessId();
            List<EmployeeImportRow> rows = ParseFile<EmployeeImportRow>(file);

            if (rows.Count > BusinessRules.MAX_IMPORT_ROWS)
            {
                throw new BusinessException(ErrorMessages.IMPORT_TOO_MANY_ROWS);
            }

            ImportResultDto result = new ImportResultDto { TotalRows = rows.Count };
            
            // Kapasite kontrolü
            Business? business = await _context.Businesses.AsNoTracking().FirstOrDefaultAsync(b => b.Id == businessId);
            if (business == null) throw new NotFoundException(ErrorMessages.BUSINESS_NOT_FOUND);

            int currentEmployeeCount = await _context.Employees.CountAsync(e => e.BusinessId == businessId);
            
            // Batch işleme
            for (int i = 0; i < rows.Count; i += BATCH_SIZE)
            {
                List<EmployeeImportRow> batch = rows.Skip(i).Take(BATCH_SIZE).ToList();
                List<string> emails = batch.Select(r => r.Email).ToList();

                // Mevcut kullanıcıları ve çalışanları bir kerede çek
                Dictionary<string, User> users = await _context.Users
                    .Where(u => emails.Contains(u.Email))
                    .ToDictionaryAsync(u => u.Email, u => u, StringComparer.OrdinalIgnoreCase);

                List<string> existingEmployeeEmails = await _context.Employees
                    .Where(e => e.BusinessId == businessId && e.User != null && emails.Contains(e.User.Email))
                    .Select(e => e.User!.Email)
                    .ToListAsync();

                foreach (EmployeeImportRow row in batch)
                {
                    int currentRowNumber = i + batch.IndexOf(row) + 2; // +1 header, +1 index

                    // 1. Kullanıcı var mı?
                    if (!users.TryGetValue(row.Email, out User? user))
                    {
                        result.Errors.Add(new ImportRowErrorDto 
                        { 
                            RowNumber = currentRowNumber, 
                            Email = row.Email, 
                            Reason = ErrorMessages.IMPORT_USER_NOT_FOUND 
                        });
                        result.FailedCount++;
                        continue;
                    }

                    // 2. Zaten çalışan mı?
                    if (existingEmployeeEmails.Contains(row.Email, StringComparer.OrdinalIgnoreCase))
                    {
                        result.Errors.Add(new ImportRowErrorDto 
                        { 
                            RowNumber = currentRowNumber, 
                            Email = row.Email, 
                            Reason = ErrorMessages.IMPORT_ALREADY_EXISTS 
                        });
                        result.FailedCount++;
                        continue;
                    }

                    // 3. Kapasite doldu mu?
                    if (currentEmployeeCount + result.SuccessCount >= business.EmployeeCapacity)
                    {
                        result.Errors.Add(new ImportRowErrorDto 
                        { 
                            RowNumber = currentRowNumber, 
                            Email = row.Email, 
                            Reason = $"{ErrorMessages.CAPACITY_LIMIT_EXCEEDED} {business.EmployeeCapacity}" 
                        });
                        result.FailedCount++;
                        continue;
                    }

                    // Ekle
                    Employee employee = new Employee
                    {
                        Id = Guid.NewGuid(),
                        BusinessId = businessId,
                        UserId = user.Id,
                        FirstName = row.FirstName ?? user.FirstName,
                        LastName = row.LastName ?? user.LastName,
                        JobTitle = row.JobTitle ?? string.Empty,
                        JobDescription = row.JobDescription ?? string.Empty,
                        CanManageOwnSchedule = row.CanManageOwnSchedule,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Employees.AddAsync(employee);
                    result.SuccessCount++;
                }

                await _context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<ImportResultDto> ImportCustomersAsync(IFormFile file)
        {
            Guid businessId = GetBusinessId();
            List<CustomerImportRow> rows = ParseFile<CustomerImportRow>(file);

            if (rows.Count > BusinessRules.MAX_IMPORT_ROWS)
            {
                throw new BusinessException(ErrorMessages.IMPORT_TOO_MANY_ROWS);
            }

            ImportResultDto result = new ImportResultDto { TotalRows = rows.Count };

            for (int i = 0; i < rows.Count; i += BATCH_SIZE)
            {
                List<CustomerImportRow> batch = rows.Skip(i).Take(BATCH_SIZE).ToList();
                List<string> emails = batch.Select(r => r.Email).ToList();

                // Mevcut kullanıcıları çek
                Dictionary<string, User> users = await _context.Users
                    .AsNoTracking()
                    .Where(u => emails.Contains(u.Email))
                    .ToDictionaryAsync(u => u.Email, u => u, StringComparer.OrdinalIgnoreCase);

                // Bu işletme ile zaten ilişkisi olanları çek (Randevu veya KnownCustomer)
                List<string> relationshipEmails = await _context.Appointments
                    .Where(a => a.BusinessId == businessId && emails.Contains(a.User.Email))
                    .Select(a => a.User.Email)
                    .Union(_context.BusinessKnownCustomers
                        .Where(c => c.BusinessId == businessId && emails.Contains(c.User.Email))
                        .Select(c => c.User.Email))
                    .ToListAsync();
                
                // Davetiyesi olanları çek
                List<string> existingInvitationEmails = await _context.PendingCustomerInvitations
                    .Where(inv => inv.BusinessId == businessId && emails.Contains(inv.Email))
                    .Select(inv => inv.Email)
                    .ToListAsync();

                foreach (CustomerImportRow row in batch)
                {
                    int currentRowNumber = i + batch.IndexOf(row) + 2;

                    // 1. Kullanıcı sistemde var mı?
                    if (users.TryGetValue(row.Email, out User? user))
                    {
                        // Zaten bir ilişkisi var mı?
                        if (relationshipEmails.Contains(row.Email, StringComparer.OrdinalIgnoreCase))
                        {
                            result.SuccessCount++; // Zaten bilindiği için başarılı sayılabilir veya atlanabilir.
                            continue;
                        }

                        // İlişki kur
                        BusinessKnownCustomer knownCustomer = new BusinessKnownCustomer
                        {
                            Id = Guid.NewGuid(),
                            BusinessId = businessId,
                            UserId = user.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.BusinessKnownCustomers.AddAsync(knownCustomer);
                    }
                    else
                    {
                        // Kullanıcı yok, davetiye oluştur (zaten yoksa)
                        if (existingInvitationEmails.Contains(row.Email, StringComparer.OrdinalIgnoreCase))
                        {
                            result.SuccessCount++;
                            continue;
                        }

                        PendingCustomerInvitation invitation = new PendingCustomerInvitation
                        {
                            Id = Guid.NewGuid(),
                            BusinessId = businessId,
                            Email = row.Email,
                            FirstName = row.FirstName,
                            LastName = row.LastName,
                            PhoneNumber = row.PhoneNumber,
                            InvitedAt = DateTime.UtcNow
                        };
                        await _context.PendingCustomerInvitations.AddAsync(invitation);
                    }

                    result.SuccessCount++;
                }

                await _context.SaveChangesAsync();
            }

            return result;
        }

        private List<T> ParseFile<T>(IFormFile file) where T : class
        {
            string extension = Path.GetExtension(file.FileName).ToLower();
            using Stream stream = file.OpenReadStream();

            if (extension == ".csv")
            {
                using StreamReader reader = new StreamReader(stream);
                CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };
                using CsvReader csv = new CsvReader(reader, config);
                return csv.GetRecords<T>().ToList();
            }
            else if (extension == ".xlsx")
            {
                List<T> list = new List<T>();
                using XLWorkbook workbook = new XLWorkbook(stream);
                IXLWorksheet worksheet = workbook.Worksheet(1);
                IXLRange range = worksheet.RangeUsed();
                
                // İlk satır header
                IXLRangeRow headerRow = range.Row(1);
                Dictionary<string, int> columns = new Dictionary<string, int>();
                for (int i = 1; i <= headerRow.CellCount(); i++)
                {
                    string colName = headerRow.Cell(i).GetString();
                    if (!string.IsNullOrEmpty(colName)) columns[colName] = i;
                }

                // Verileri oku
                for (int i = 2; i <= range.RowCount(); i++)
                {
                    IXLRangeRow row = range.Row(i);
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in typeof(T).GetProperties())
                    {
                        if (columns.TryGetValue(prop.Name, out int colIndex))
                        {
                            string val = row.Cell(colIndex).GetString();
                            if (prop.PropertyType == typeof(bool))
                            {
                                prop.SetValue(obj, val?.ToLower() == "true" || val == "1");
                            }
                            else
                            {
                                prop.SetValue(obj, val);
                            }
                        }
                    }
                    list.Add(obj);
                }
                return list;
            }

            throw new BusinessException(ErrorMessages.IMPORT_INVALID_FORMAT);
        }
    }
}
