using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{


    public record EmployeeDto(
        int Id,
        string FirstName,
        string LastName,
        DateTime HireDate,
        string Email,
        List<string> Skills,
        int DepartmentId);

    public record CreateEmployeeDto(
        [Required] string FirstName,
        [Required] string LastName,
        DateTime HireDate,
        string Email,
        List<string>? Skills,
        int DepartmentId);

    public record UpdateEmployeeDto(
        [Required] string FirstName,
        [Required] string LastName,
        string Email,
        DateTime HireDate);

    public record AddSkillDto(
        [Required] string SkillName);


    public record SkillDto(int Id, string Name, string? Description, DateTime CreatedAt);


    public record CreateSkillDto(
        [Required, StringLength(120)] string Name,
        [StringLength(1000)] string? Description);


    public record UpdateSkillDto(
        [Required] int Id,
        [Required, StringLength(120)] string Name,
        [StringLength(1000)] string? Description);

}
