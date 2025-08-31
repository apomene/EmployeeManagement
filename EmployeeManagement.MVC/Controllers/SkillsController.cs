using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace EmployeeManagement.Controllers
{
    public class SkillsController : Controller
    {
        private readonly HttpClient _http;

        public SkillsController(IHttpClientFactory factory, IConfiguration configuration)
        {
            var apiName = configuration.GetValue<string>("ApiSettings:EmployeesApiName");
            _http = factory.CreateClient(apiName);
        }

        // Page A: List skills
        public async Task<IActionResult> Index()
        {
            var skills = await _http.GetFromJsonAsync<IEnumerable<Skill>>(StringConstants.SKILLS);
            return View(skills);
        }

        // Page B: Details
        public async Task<IActionResult> Details(int id)
        {
            var skill = await _http.GetFromJsonAsync<Skill>($"{StringConstants.SKILLS}/{id}");
            if (skill == null) return NotFound();
            return View(skill);
        }

        // Page C: Create
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Skill skill)
        {
            var response = await _http.PostAsJsonAsync(StringConstants.SKILLS, skill);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                errorMessage = string.IsNullOrWhiteSpace(errorMessage)
                    ? StringConstants.ERROR_CREATE_SKILL
                    : errorMessage;

                ModelState.AddModelError("", errorMessage);              
            }
            
            return View(skill);
        }

        // Edit
        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _http.GetFromJsonAsync<Skill>($"{StringConstants.SKILLS}/{id}");
            if (skill == null) return NotFound();
            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Skill skill)
        {
            var response = await _http.PutAsJsonAsync($"{StringConstants.SKILLS}/{id}", skill);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", StringConstants.ERROR_UPDATE_SKILL);
            return View(skill);
        }

        // Delete
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _http.GetFromJsonAsync<Skill>($"{StringConstants.SKILLS}/{id}");
            if (skill == null) return NotFound();

            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _http.DeleteAsync($"{StringConstants.SKILLS}/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(errorMessage)
                    ? StringConstants.FAIL_DELETE_SKILLS
                    : errorMessage;

                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["ErrorMessage"] = StringConstants.ERROR_DELETE_SKILL;
            return RedirectToAction(nameof(Delete), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ExportSkills()
        {            
            var response = await _http.GetAsync("/export");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = StringConstants.FAIL_EXPORT_SKILLS;
                return RedirectToAction("Index");
            }

            // Read file bytes and content disposition
            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "text/csv";
            var fileName = $"skills_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(contentBytes, contentType, fileName);
        }
    }
}
