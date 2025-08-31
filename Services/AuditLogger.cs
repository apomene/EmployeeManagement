using EmployeeManagement.API.Services;
using MongoDB.Driver;
using NLog;
using ILogger = NLog.ILogger;

public class AuditLogger : IAuditLogger
{
    private readonly IMongoCollection<AuditLogEntry> _auditCollection;
    private readonly ILogger _logger;

    public AuditLogger(IMongoDatabase mongoDb, ILogger<AuditLogger> logger)
    {
        _auditCollection = mongoDb.GetCollection<AuditLogEntry>("AuditLogs");
        _logger = LogManager.GetCurrentClassLogger();
    }

    public async Task LogChangeAsync(string entityName, string entityId, string action, object? newValue, string performedBy, object? oldValue)
    {
        var entry = new AuditLogEntry
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            PerformedBy = performedBy,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _auditCollection.InsertOneAsync(entry);
            _logger.Info("Audit log written for {EntityName}:{EntityId} - {Action}", entityName, entityId, action);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write audit log for {EntityName}:{EntityId}", entityName, entityId);
        }
    }
}

/// <summary>
/// For testing purposes, a no-op audit logger that does nothing.
/// </summary>
public class FakeAuditLogger : IAuditLogger 
{
    public Task  LogChangeAsync(string entityName, string entityId, string action, object? newValue, string performedBy, object? oldValue)
    {
        // do nothing in tests
        return Task.CompletedTask;
    }
}


public class AuditLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string Action { get; set; } = default!;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public string PerformedBy { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}
