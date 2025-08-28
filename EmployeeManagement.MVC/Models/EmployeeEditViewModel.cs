using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.MVC.Models
{
    public class EmployeeEditViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        // Department
        public int DepartmentId { get; set; }
        public IEnumerable<SelectListItem>? AvailableDepartments { get; set; }

        // Skills
        public List<int> SelectedSkillIds { get; set; } = new();
        public IEnumerable<SelectListItem>? AvailableSkills { get; set; }
    }
}
