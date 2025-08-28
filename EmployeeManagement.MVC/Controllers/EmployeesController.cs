using EmployeeManagement.Models;
using EmployeeManagement.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using System.Reflection;
using System.Xml.Linq;

namespace EmployeeManagement.MVC.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly HttpClient _http;
        private readonly List<Department> _departments;

        public EmployeesController(IHttpClientFactory factory, IConfiguration configuration)
        {
            var apiName = configuration.GetValue<string>("ApiSettings:EmployeesApiName");
            _http = factory.CreateClient(apiName);
            _departments = GetDepartments();
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
            var dto = await _http.GetFromJsonAsync<EmployeeDto>($"{StringConstants.EMPLOYEES}/{id}");
            var allSkills = await _http.GetFromJsonAsync<List<Skill>>(StringConstants.SKILLS);


            var viewModel = GetViewModel(dto, allSkills);

            return View(viewModel);
        }



        // GET: Create
        public async Task<IActionResult> Create()
        {
           
            var skills = await _http.GetFromJsonAsync<List<Skill>>(StringConstants.SKILLS);

            var viewModel = new CreateEmployeeViewModel
            {
                Departments = new SelectList(_departments, "Id", "Name"),
                AvailableSkills = new SelectList(skills, "Id", "Name")
            };

            return View(viewModel);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel viewModel)
        {
            var skills = await _http.GetFromJsonAsync<List<Skill>>(StringConstants.SKILLS);
            if (!ModelState.IsValid)
            {
                // Reload dropdowns
                viewModel.Departments = new SelectList(_departments, "Id", "Name", viewModel.DepartmentId);
                viewModel.AvailableSkills = new SelectList(skills, "Id", "Name", viewModel.SelectedSkillId);
                return View(viewModel);
            }

            int skillIdToAssign;
            List<string> skillIsToAssign = new List<string>();

            if (viewModel.SelectedSkillId == -1 && !string.IsNullOrWhiteSpace(viewModel.NewSkillName))
            {
                // Create the new skill via API
                var skillResponse = await _http.PostAsJsonAsync(StringConstants.SKILLS,  new CreateSkillDto( viewModel.NewSkillName , viewModel.NewSkillDescription));
                if (!skillResponse.IsSuccessStatusCode)
                {
                    TempData["SkillError"] = "Could not create new skill.";
                    return RedirectToAction(nameof(Create));
                }

                var newSkill = await skillResponse.Content.ReadFromJsonAsync<Skill>();
                skillIdToAssign = newSkill.Id;
                skillIsToAssign.Add(newSkill.Name);
            }
            else
            {
                skillIdToAssign = viewModel.SelectedSkillId.Value;
                if (skillIdToAssign > 0)
                {
                    var selectedSkill = skills.FirstOrDefault(s => s.Id == skillIdToAssign);
                    if (selectedSkill != null)
                    {
                        skillIsToAssign.Add(selectedSkill.Name);
                    }
                }
            }

         
            // Map ViewModel to DTO or entity
            var dto = new EmployeeDto(
                viewModel.Id,
                viewModel.FirstName,
                viewModel.LastName,
                viewModel.HireDate,
                viewModel.Email,
                skillIsToAssign,
                viewModel.DepartmentId
            );

            var response = await _http.PostAsJsonAsync(StringConstants.EMPLOYEES, dto);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", StringConstants.ERROR_CREATE_EMPLOYEE);
                return View(viewModel);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
           
            var employee = await _http.GetFromJsonAsync<Employee>($"{StringConstants.EMPLOYEES}/{id}");
            if (employee == null) return NotFound();

            
            ViewData["Departments"] = new SelectList(_departments, "Id", "Name", employee.DepartmentId);

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

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            
            var employee = await _http.GetFromJsonAsync<EmployeeDto>($"{StringConstants.EMPLOYEES}/{id}");
            if (employee == null)
                return NotFound();

            return View(GetViewModel(employee));
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(EmployeeViewModel viewModel)
        {
            if (viewModel == null || viewModel.Id == 0)
                return BadRequest(StringConstants.INVALID_EMPLOYEE);

            var response = await _http.DeleteAsync($"{StringConstants.EMPLOYEES}/{viewModel.Id}");

            if (!response.IsSuccessStatusCode)
            {
                // Optional: you could add a TempData message to show the error in the UI
                ModelState.AddModelError("", StringConstants.NO_MATCHING_EMPLOYEES);
                return View(viewModel); // show the Delete page again
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> AddSkill(int employeeId, int skillId)
        {
            var response = await _http.PostAsJsonAsync(
                $"{StringConstants.EMPLOYEES}/{employeeId}/{StringConstants.SKILLS}/{skillId}", new { skillId });

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Details), new { id = employeeId });
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {

                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["SkillError"] = errorMessage;
            }
            else
            {
                TempData["SkillError"] = StringConstants.UNEXPTECTED_ERROR;
            }
               
            return RedirectToAction(nameof(Details), new { id = employeeId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSkill(int employeeId, int skillId)
        {
            var response = await _http.DeleteAsync(
                $"{StringConstants.EMPLOYEES}/{employeeId}/{StringConstants.SKILLS}/{skillId}");

            return RedirectToAction("Details", new { id = employeeId });
        }

        private EmployeeViewModel GetViewModel(EmployeeDto dto, List<Skill> allSkills)
        {
            var skillViewModel = new List<EmployeeSkillViewModel>();
            skillViewModel = allSkills.Where(s=>dto.Skills.Contains(s.Name))
                .Select(skill => new EmployeeSkillViewModel { Id = skill.Id, Name = skill.Name })
                .ToList();
            
            var viewModel = new EmployeeViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,
                DepartmentName = _departments.FirstOrDefault(d => d.Id == dto.DepartmentId)?.Name ?? string.Empty,  
                Skills = skillViewModel,
                AvailableSkills = new SelectList(allSkills, "Id", "Name")
            };
            return viewModel;
        }

        private EmployeeViewModel GetViewModel(EmployeeDto dto)
        {
           
            var viewModel = new EmployeeViewModel
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,

            };
            return viewModel;
        }

        private List<Department> GetDepartments()
        {
            var departments = _http.GetFromJsonAsync<List<Department>>($"{StringConstants.EMPLOYEES}/{StringConstants.DEPARTMENTS}").Result;
            return departments ?? new List<Department>();
        }


    }
}
