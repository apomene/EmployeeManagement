using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.MVC.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly HttpClient _http;
        private  int _pageSize;

        public AuditLogsController(IHttpClientFactory factory, IConfiguration configuration)
        {
            var apiName = configuration.GetValue<string>("ApiSettings:EmployeesApiName");
            _http = factory.CreateClient(apiName);
            _pageSize = configuration.GetValue<int>("pageSize", 50);
        }

        public  IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Employee(string email, int pageNumber = 1)
        {
            var result = await _http.GetFromJsonAsync<PagedResult<AuditLogEntry>>(
                $"{StringConstants.AUDIT_LOGS}/{email}?pageNumber={pageNumber}&pageSize={_pageSize}");

            if (result == null || !result.Items.Any())
            {
                ViewBag.Email = email;
                return View("NoLogs");
            }

            ViewBag.Email = email;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = _pageSize;
            ViewBag.TotalPages = result.TotalPages;

            return View(result.Items);
        }

    }
}
