using EmployeeManagement.Models;

namespace EmployeeManagement.API.Services
{
    public class Helpers
    {
        // helper: safely maps Employee to EmployeeDto
        public static EmployeeDto ToEmployeeDto(Employee employee)
        {
            return new EmployeeDto(
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.HireDate,
                employee.Email,
                employee.EmployeeSkills?
                    .Select(es => es.Skill?.Name ?? string.Empty)  // Safe navigation
                    .Where(name => !string.IsNullOrEmpty(name))    // Drop null/empty skill names
                    .ToList()
                    ?? new List<string>(),
                employee.DepartmentId
            );
        }
      
    }
}
