namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();

    }
}
