using EmployeeManagement.API.Services;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EmployeeManagement.API.Controllers
{
    /// <summary>
    /// Controller for managing and retrieving audit log entries related to Employees.
    /// Provides endpoints to fetch logs for a specific employee or all audit logs with pagination support.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<AuditLogsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogsController"/> class.
        /// </summary>
        /// <param name="auditLogger">Service for accessing audit logs.</param>
        /// <param name="logger">Logger instance for diagnostic purposes.</param>

        public AuditLogsController(IAuditLogger auditLogger, ILogger<AuditLogsController> logger)
        {
            _logger = logger;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// Retrieves paged audit logs for a specific employee identified by email.
        /// </summary>
        /// <param name="email">The employee's email address.</param>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of logs per page (default 50, max 100).</param>
        /// <returns>A paged result containing the employee's audit log entries.</returns>
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

        /// <summary>
        /// Retrieves paged audit logs for all entities.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of logs per page (default 50, max 500).</param>
        /// <returns>A paged result containing all audit log entries.</returns>
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
