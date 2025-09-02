using EmployeeManagement.Models;
using MongoDB.Driver;

namespace EmployeeManagement.API.Services
{

    /// <summary>
    /// For testing purposes, a no-op audit logger that does nothing.
    /// </summary>
    public class FakeAuditLogger : IAuditLogger
    {

        public async Task LogChangeAsync(
           string entityName,
           string entityId,
           string action,
           EmployeeDto? newValue,
           string performedBy,
           EmployeeDto? oldValue = null)
        {
            // do nothing in tests
            await Task.CompletedTask;
        }

        public async Task<List<AuditLogEntry>> GetLogsByEmployeeAsync(string employeeId)
        {
            return await Task.Run(() => new List<AuditLogEntry>());
        }

        public async Task<List<AuditLogEntry>> GetAllLogsAsync()
        {
            return await Task.Run(() => new List<AuditLogEntry>());
        }

        public async Task<int> CountAsync(FilterDefinition<AuditLogEntry> filter)
        {
            return await Task.Run(() => 0);
        }

        public async Task<List<AuditLogEntry>> GetLogsByEmployeeAsync(string employeeId, int pageNumber, int pageSize)
        {
            return await Task.Run(() => new List<AuditLogEntry>());
        }

        public async Task<List<AuditLogEntry>> GetAllLogsAsync(int pageNumber, int pageSize)
        {
            return await Task.Run(() => new List<AuditLogEntry>());
        }

        public async Task<int> CountAllLogsAsync()
        {
            return await Task.Run(() => 0);
        }
    }
}
