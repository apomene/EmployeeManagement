using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.MVC.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly HttpClient _http;

        public AuditLogsController(IHttpClientFactory factory, IConfiguration configuration)
        {
            var apiName = configuration.GetValue<string>("ApiSettings:EmployeesApiName");
            _http = factory.CreateClient(apiName);
        }

        public async Task<IActionResult> Employee(string email)
        {
            var logs = await _http.GetFromJsonAsync<List<AuditLogEntry>>($"{ StringConstants.AUDIT_LOGS}/{email}");

            if (logs == null || !logs.Any())
            {
                ViewBag.Email = email;
                return View("NoLogs");
            }

            return View(logs);
        }
    }
}
