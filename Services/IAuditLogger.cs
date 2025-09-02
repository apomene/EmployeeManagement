using EmployeeManagement.Models;
using MongoDB.Driver;

namespace EmployeeManagement.API.Services
{
    public interface IAuditLogger
    {
        Task LogChangeAsync(
            string entityName,
            string entityId,
            string action,
            EmployeeDto? newValue,
            string performedBy,
            EmployeeDto? oldValue = null);

        Task<List<AuditLogEntry>> GetAllLogsAsync();

        Task<List<AuditLogEntry>> GetLogsByEmployeeAsync(string employeeId);

        Task<List<AuditLogEntry>> GetLogsByEmployeeAsync(string employeeId, int pageNumber, int pageSize);
        Task<List<AuditLogEntry>> GetAllLogsAsync(int pageNumber, int pageSize);
        Task<int> CountAllLogsAsync();

        Task<int> CountAsync(FilterDefinition<AuditLogEntry> filter);
    }

}
