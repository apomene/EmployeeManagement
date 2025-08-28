using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.MVC.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        // Display fields
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }

        // For dropdowns / display
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        // Skills list
        public List<string> Skills { get; set; } = new();

        // For forms (like "Add Skill" or "Choose Department")
        public IEnumerable<SelectListItem>? AvailableDepartments { get; set; }
        public IEnumerable<SelectListItem>? AvailableSkills { get; set; }
    }

}
