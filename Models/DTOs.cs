using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{

        // Response DTO
        public record EmployeeDto(
            int Id,
            string FirstName,
            string LastName,
            DateTime HireDate,
            List<string> Skills);

        // Input DTOs
        public record CreateEmployeeDto(
            [Required] string FirstName,
            [Required] string LastName,
            DateTime HireDate,
            List<string>? Skills);

        public record UpdateEmployeeDto(
            [Required] string FirstName,
            [Required] string LastName,
            DateTime HireDate);

        public record AddSkillDto(
            [Required] string SkillName);

}
