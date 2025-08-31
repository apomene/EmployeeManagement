using EmployeeManagement.API.Services;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using NLog;
using ILogger = NLog.ILogger;
using JsonConvert = Newtonsoft.Json.JsonConvert;

public class AuditLogger : IAuditLogger
{
    private readonly IMongoCollection<BsonDocument> _auditCollection;
    private readonly ILogger _logger;

    public AuditLogger(IMongoDatabase mongoDb, ILogger<AuditLogger> logger)
    {
        _auditCollection = mongoDb.GetCollection<BsonDocument>("AuditLogs");
        _logger = LogManager.GetCurrentClassLogger();
    }

    public async Task LogChangeAsync(string entityName, string entityId, string action, object? newValue, string performedBy, object? oldValue)
    {
        try
        {

            var newValueDoc = newValue as BsonDocument ?? BsonDocument.Parse(JsonConvert.SerializeObject(newValue));


            var doc = new BsonDocument
                            {
                                { "EntityName", entityName },
                                { "EntityId", entityId },
                                { "Action", action },
                                { "PerformedBy", performedBy },
                                { "Timestamp", DateTime.UtcNow },
                                { "NewValue", newValueDoc }
                            };

            await _auditCollection.InsertOneAsync(doc);


            await _auditCollection.InsertOneAsync(doc);
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
    public Task LogChangeAsync(string entityName, string entityId, string action, object? newValue, string performedBy, object? oldValue)
    {
        // do nothing in tests
        return Task.CompletedTask;
    }
}


public class AuditLogEntry
{
    [BsonId]
    public ObjectId Id { get; set; }  
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string Action { get; set; } = default!;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
    public string PerformedBy { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}
