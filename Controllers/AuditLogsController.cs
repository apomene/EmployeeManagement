using EmployeeManagement.API.Services;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogger _auditLogger;

        public AuditLogsController(IAuditLogger auditLogger)
        {
            _auditLogger = auditLogger;
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<List<AuditLogEntry>>> GetEmployeeLogs(string email)
        {
            var logs = await _auditLogger.GetLogsByEmployeeAsync(email);

            if (logs == null || !logs.Any())
                return NotFound($"{StringConstants.NO_LOGS_FOUND}{email}'.");

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
