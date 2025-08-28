using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace EmployeeManagement.MVC.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly HttpClient _http;

        public EmployeesController(IHttpClientFactory factory, IConfiguration configuration)
        {
            var apiName = configuration.GetValue<string>("ApiSettings:EmployeesApiName");
            _http = factory.CreateClient(apiName);
        }
        // GET: Employees
        public async Task<IActionResult> Index(string? sortBy, string? search)
        {
            var employees = await _http.GetFromJsonAsync<List<Employee>>(StringConstants.EMPLOYEES);

            if (!string.IsNullOrEmpty(search))
            {
                employees = employees?
                    .Where(e => e.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)
                             || e.LastName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            employees = sortBy switch
            {
                "lastname_asc" => employees?.OrderBy(e => e.LastName).ToList(),
                "lastname_desc" => employees?.OrderByDescending(e => e.LastName).ToList(),
                "hiredate_asc" => employees?.OrderBy(e => e.HireDate).ToList(),
                "hiredate_desc" => employees?.OrderByDescending(e => e.HireDate).ToList(),
                _ => employees
            };

            ViewData["CurrentSearch"] = search; 
            ViewData["CurrentSort"] = sortBy;

            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _http.GetFromJsonAsync<Employee>($"{StringConstants.EMPLOYEES}/{id}");
            var allSkills = await _http.GetFromJsonAsync<List<Skill>>(StringConstants.SKILLS);

            ViewBag.Skills = new SelectList(allSkills, "Id", "Name");
            return View(employee);
        }


        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            var departments = await _http.GetFromJsonAsync<List<Department>>($"{StringConstants.EMPLOYEES}/{StringConstants.DEPARTMENTS}");
            ViewData["Departments"] = new SelectList(departments, "Id", "Name");
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                // Log or inspect which fields are invalid
                foreach (var entry in ModelState)
                {
                    var key = entry.Key;
                    var errors = entry.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Property '{key}' is invalid: {error.ErrorMessage}");
                    }
                }

                var departments = await _http.GetFromJsonAsync<List<Department>>($"{StringConstants.EMPLOYEES}/{StringConstants.DEPARTMENTS}");
                ViewData["Departments"] = new SelectList(departments, "Id", "Name", employee.DepartmentId);
                return View(employee);
            }

            var response = await _http.PostAsJsonAsync(StringConstants.EMPLOYEES, employee);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", StringConstants.ERROR_CREATE_EMPLOYEE);
            return View(employee);
        }

     

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
           
            var employee = await _http.GetFromJsonAsync<Employee>($"{StringConstants.EMPLOYEES}/{id}");
            if (employee == null) return NotFound();

            
            var departments = await _http.GetFromJsonAsync<List<Department>>($"{StringConstants.EMPLOYEES}/{StringConstants.DEPARTMENTS}");
            ViewData["Departments"] = new SelectList(departments, "Id", "Name", employee.DepartmentId);

            return View(employee);

        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                ModelState.AddModelError("", StringConstants.ID_MISMATCH);
                return View(employee);
            }
            if (!ModelState.IsValid) return View(employee);

            var response = await _http.PutAsJsonAsync($"{StringConstants.EMPLOYEES}/{id}", employee);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", StringConstants.ERROR_UPDATE_EMPLOYEE);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _http.GetFromJsonAsync<Employee>($"{StringConstants.EMPLOYEES}/{id}");
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _http.DeleteAsync($"{StringConstants.EMPLOYEES}/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", StringConstants.ERROR_DELETE_EMPLOYEE);
            return RedirectToAction(nameof(Index));
        }
     
        [HttpPost]
        public async Task<IActionResult> AddSkill(int employeeId, int skillId)
        {
            var response = await _http.PostAsJsonAsync(
                $"{StringConstants.EMPLOYEES}/{employeeId}/{StringConstants.SKILLS}/{skillId}", new { skillId });

            return RedirectToAction("Details", new { id = employeeId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSkill(int employeeId, int skillId)
        {
            var response = await _http.DeleteAsync(
                $"{StringConstants.EMPLOYEES}/{employeeId}/{StringConstants.SKILLS}/{skillId}");

            return RedirectToAction("Details", new { id = employeeId });
        }


    }
}
