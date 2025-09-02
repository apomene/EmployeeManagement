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
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(IAuditLogger auditLogger, ILogger<AuditLogsController> logger)
        {
            _logger = logger;
            _auditLogger = auditLogger;
        }

        [HttpGet("{email}")]
        public  Task<ActionResult<List<AuditLogEntry>>> GetEmployeeLogs(string email)
        {
            var result =  ActionWrapper.ExecuteAsync(
                _logger,
                () => GetEmployeeLogsInternal(email),
                StringConstants.LOGS_ERROR, email
                );
            return result;
        }

        private async Task<List<AuditLogEntry>> GetEmployeeLogsInternal(string email)
        {

            var logs = await _auditLogger.GetLogsByEmployeeAsync(email);
            try
            {
                if (logs == null || !logs.Any())
                    throw new KeyNotFoundException($"{StringConstants.NO_LOGS_FOUND}{email}'.");

            }
            catch (KeyNotFoundException kex)
            {
                _logger.LogWarning(kex.Message);
                return new List<AuditLogEntry>();
            }
          
            return logs;
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
