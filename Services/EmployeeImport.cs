using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    public interface IEmployeeImportService
    {
        Task<ImportResult> ImportFromExternalSystemAsync(int batchSize = 50, bool isManual = false);
    }

    public class EmployeeImportService : IEmployeeImportService
    {
        private readonly HttpClient _http;
        private readonly AppDbContext _db;

        public EmployeeImportService(HttpClient http, AppDbContext db)
        {
            _http = http;
            _db = db;
        }

        public async Task<ImportResult> ImportFromExternalSystemAsync(int batchSize = 50, bool isManual = false)
        {
            var result = new ImportResult();

            var externalEmployees = await _http.GetFromJsonAsync<List<ExternalEmployeeDto>>(
                $"/api/external/employees?limit={batchSize}"
            );

            if (externalEmployees == null || !externalEmployees.Any())
            {
                result.Message = StringConstants.NO_EMPLOYEES;
                return result;
            }

            foreach (var ext in externalEmployees)
            {
                try
                {
                    if (await _db.Employees.AnyAsync(e => e.Email == ext.Email))
                    {
                        result.Skipped++;
                        continue;
                    }

                    var employee = new Employee
                    {
                        FirstName = ext.FirstName,
                        LastName = ext.LastName,
                        Email = ext.Email,
                        HireDate = ext.HireDate,
                        DepartmentId = ext.DepartmentId
                    };

                    _db.Employees.Add(employee);
                    result.Imported++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add(ex.Message);
                }
            }

            await _db.SaveChangesAsync();
            return result;
        }
    } 
}
