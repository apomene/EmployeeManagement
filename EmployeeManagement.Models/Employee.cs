
namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }   
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        public int DepartmentId { get; set; }    
        public Department? Department { get; set; } = null!; // Navigation property

        public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();

    }
}
