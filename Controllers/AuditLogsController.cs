using EmployeeManagement.API.Services;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

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
        public async Task<ActionResult<PagedResult<AuditLogEntry>>> GetEmployeeLogs( string email,int pageNumber = 1, int pageSize = 50)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 50; // max 100 per page

            var filter = Builders<AuditLogEntry>.Filter.Eq(e => e.EntityName, "Employee") &
                         Builders<AuditLogEntry>.Filter.Eq(e => e.EntityId, email);

            var total = await _auditLogger.CountAsync(filter);
            var logs = await _auditLogger.GetLogsByEmployeeAsync(email, pageNumber, pageSize);

            var pagedResult = new PagedResult<AuditLogEntry>
            {
                Items = logs,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(pagedResult);
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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 50)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 50; // limit max page size

            var totalCount = await _auditLogger.CountAllLogsAsync();
            var logs = await _auditLogger.GetAllLogsAsync(pageNumber, pageSize);

            var pagedResult = new PagedResult<AuditLogEntry>
            {
                Items = logs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(pagedResult);
        }

    }

}
