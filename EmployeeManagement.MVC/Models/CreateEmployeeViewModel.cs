using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.MVC.Models
{
    public class CreateEmployeeViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Now;
        public int DepartmentId { get; set; }


        public int? SelectedSkillId { get; set; }
        public List<int>? SelectedSkillIds { get; set; }
        public string? NewSkillName { get; set; }
        public string? NewSkillDescription { get; set; }

        public SelectList? AvailableSkills { get; set; }
        public SelectList? Departments { get; set; }
    }

}
