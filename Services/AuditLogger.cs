
using EmployeeManagement.API.Services;
using EmployeeManagement.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using NLog;
using ILogger = NLog.ILogger;
using JsonConvert = Newtonsoft.Json.JsonConvert;

public class AuditLogger : IAuditLogger
{
    private readonly IMongoCollection<AuditLogEntry> _auditCollection;
    private readonly ILogger _logger;
    public AuditLogger(IMongoDatabase mongoDb, ILogger<AuditLogger> logger)
    {
        _auditCollection = mongoDb.GetCollection<AuditLogEntry>("AuditLogs");
        _logger = LogManager.GetCurrentClassLogger();
    }

    public async Task LogChangeAsync(
        string entityName,
        string entityId,
        string action,
        EmployeeDto? newValue,
        string performedBy,
        EmployeeDto? oldValue = null)
    {
        try
        {
            var logEntry = new AuditLogEntry
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                NewValue = newValue,
                OldValue = oldValue,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow
            };

            await _auditCollection.InsertOneAsync(logEntry);
            _logger.Info("Audit log written for {EntityName}:{EntityId} - {Action}",
                         entityName, entityId, action);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write audit log for {EntityName}:{EntityId}",
                          entityName, entityId);
        }
    }

    public async Task<List<AuditLogEntry>> GetLogsByEmployeeAsync(string employeeId)
    {
        var filter = Builders<AuditLogEntry>.Filter.Eq(e => e.EntityName, "Employee") &
                     Builders<AuditLogEntry>.Filter.Eq(e => e.EntityId, employeeId);

        return await _auditCollection.Find(filter)
                                     .SortByDescending(e => e.Timestamp)
                                     .ToListAsync();
    }

    public async Task<List<AuditLogEntry>> GetLogsByEmployeeAsync(
    string employeeId, int pageNumber, int pageSize)
    {
        var filter = Builders<AuditLogEntry>.Filter.Eq(e => e.EntityName, "Employee") &
                     Builders<AuditLogEntry>.Filter.Eq(e => e.EntityId, employeeId);

        return await _auditCollection.Find(filter)
                                     .SortByDescending(e => e.Timestamp)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Limit(pageSize)
                                     .ToListAsync();
    }

    public async Task<int> CountAsync(FilterDefinition<AuditLogEntry> filter)
    {
        return (int)await _auditCollection.CountDocumentsAsync(filter);
    }


    public async Task<List<AuditLogEntry>> GetAllLogsAsync()
    {
        return await _auditCollection.Find(_ => true)
                                     .SortByDescending(e => e.Timestamp)
                                     .ToListAsync();
    }

    public async Task<List<AuditLogEntry>> GetAllLogsAsync(int pageNumber, int pageSize)
    {
        return await _auditCollection
            .Find(_ => true)
            .SortByDescending(e => e.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAllLogsAsync()
    {
        return (int)await _auditCollection.CountDocumentsAsync(_ => true);
    }

}

