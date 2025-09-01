using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AuditLogger _auditLogger;

        public AuditLogsController(AuditLogger auditLogger)
        {
            _auditLogger = auditLogger;
        }

        [HttpGet("employee/{email}")]
        public async Task<ActionResult<List<BsonDocument>>> GetEmployeeLogs(string email)
        {
            var logs = await _auditLogger.GetLogsByEmployeeAsync(email);

            if (logs == null || !logs.Any())
                return NotFound($"No audit logs found for employee with e-mail'{email}'.");

            return Ok(logs);
        }


        // GET: api/auditlogs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _auditLogger.GetAllLogsAsync();
            return Ok(logs);
        }
    }

}
