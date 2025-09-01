using EmployeeManagement.API.Data;
using EmployeeManagement.Models;

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
    }

}
