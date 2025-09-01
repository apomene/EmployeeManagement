using EmployeeManagement.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EmployeeManagement.API.Data
{

    public class AuditLogEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;

        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public string Action { get; set; } = default!;

        public EmployeeDto? NewValue { get; set; }
        public EmployeeDto? OldValue { get; set; }

        public string PerformedBy { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }

}
