namespace EmployeeManagement.API.Services
{
    public interface IAuditLogger
    {
        Task LogChangeAsync(string entityName, string entityId, string action, object? newValue, string performedBy, object? oldValue= null);

    }
}
