using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using EmployeeManagement.Models;

namespace EmployeeManagement.Controllers
{
    public class SkillsController : Controller
    {
        private readonly HttpClient _http;

        public SkillsController(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("EmployeesAPI");
        }

        // Page A: List skills
        public async Task<IActionResult> Index()
        {
            var skills = await _http.GetFromJsonAsync<IEnumerable<Skill>>("skills");
            return View(skills);
        }

        // Page B: Details
        public async Task<IActionResult> Details(int id)
        {
            var skill = await _http.GetFromJsonAsync<Skill>($"skills/{id}");
            if (skill == null) return NotFound();
            return View(skill);
        }

        // Page C: Create
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Skill skill)
        {
            var response = await _http.PostAsJsonAsync("skills", skill);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error creating skill");
            return View(skill);
        }

        // Edit
        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _http.GetFromJsonAsync<Skill>($"skills/{id}");
            if (skill == null) return NotFound();
            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Skill skill)
        {
            var response = await _http.PutAsJsonAsync($"skills/{id}", skill);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error updating skill");
            return View(skill);
        }

        // Delete
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _http.GetFromJsonAsync<Skill>($"skills/{id}");
            if (skill == null) return NotFound();
            return View(skill);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _http.DeleteAsync($"skills/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error deleting skill");
            return RedirectToAction(nameof(Index));
        }
    }
}
