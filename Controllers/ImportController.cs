using EmployeeManagement.API.Services;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IEmployeeImportService _importService;

        public ImportController(IEmployeeImportService importService)
        {
            _importService = importService;
        }

        /// <summary>
        /// Trigger employee import from external system.
        /// </summary>
        /// <param name="batchSize">Optional batch size limit for import.</param>
        [HttpPost]
        public async Task<ActionResult<ImportResult>> ImportEmployees([FromQuery] int batchSize = 50)
        {
            var result = await _importService.ImportFromExternalSystemAsync(batchSize);
            return Ok(result);
        }
    }

}
