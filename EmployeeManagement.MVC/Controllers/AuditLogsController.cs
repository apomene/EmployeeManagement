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

        public  IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Employee(string email)
        {
            var logs = await _http.GetFromJsonAsync<List<AuditLogEntry>>($"{StringConstants.AUDIT_LOGS}/{email}");
            ViewBag.Email = email;
            
            if (logs == null || !logs.Any())
            {
                
                return View("NoLogs");
            }

            return View(logs);
        }
    }
}
