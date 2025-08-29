using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.MVC.Models
{
    public class EmployeeSkillViewModel
    {
        public int Id { get; set; }      // SkillId
        public string Name { get; set; } = string.Empty;

        public DateTime? AssignedAt { get; set; }

    }

    public class EmployeeViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        // Assigned skills
        public List<EmployeeSkillViewModel> Skills { get; set; } = new();

        // For Add Skill dropdown
        public IEnumerable<SelectListItem>? AvailableSkills { get; set; }

        public List<int> SelectedSkillIds { get; set; } = new();

        public Tuple<string, DateTime>? LatestSkillUpdate { get; set; } 
    }

}
