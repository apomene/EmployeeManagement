using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.MVC.Controllers
{
    public class EmployeeImportController : Controller
    {
        private readonly HttpClient _http;



        public EmployeeImportController(IHttpClientFactory factory, IConfiguration configuration)
        {
            var apiName = configuration.GetValue<string>("ApiSettings:EmployeesApiName");
            _http = factory.CreateClient(apiName);
        }

         [HttpPost]
        public async Task<IActionResult> Import(int batchSize = 50)
        {
            try
            {
                var result = await _http.PostAsync($"/import?batchSize={batchSize}", null);
                var importResult = await result.Content.ReadFromJsonAsync<ImportResult>();

                TempData["ImportResult"] = importResult?.Message ?? "No response from API";
                return RedirectToAction("Index", "Employees");
            }
            catch (Exception ex)
            {
                TempData["ImportResult"] = $"Error during import: {ex.Message}";
                return RedirectToAction("Index", "Employees");
            }
        }
    }


}
