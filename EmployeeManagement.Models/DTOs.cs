using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    /// <summary>
    /// Represents an employee with full details including ID, personal info, skills, and department.
    /// </summary>
    /// <param name="Id">Unique identifier of the employee.</param>
    /// <param name="FirstName">First name of the employee.</param>
    /// <param name="LastName">Last name of the employee.</param>
    /// <param name="HireDate">Date the employee was hired.</param>
    /// <param name="Email">Email address of the employee.</param>
    /// <param name="Skills">List of skills associated with the employee.</param>
    /// <param name="DepartmentId">Identifier of the department the employee belongs to.</param>
    public record EmployeeDto(
        int Id,
        string FirstName,
        string LastName,
        DateTime HireDate,
        string Email,
        List<string> Skills,
        int DepartmentId);

    /// <summary>
    /// DTO for creating a new employee. 
    /// Requires first and last name, and optionally accepts email and skills.
    /// </summary>
    /// <param name="FirstName">First name of the employee (required).</param>
    /// <param name="LastName">Last name of the employee (required).</param>
    /// <param name="HireDate">Date the employee was hired.</param>
    /// <param name="Email">Email address of the employee.</param>
    /// <param name="Skills">Optional list of skills.</param>
    /// <param name="DepartmentId">Identifier of the department the employee belongs to.</param>
    public record CreateEmployeeDto(
        [Required] string FirstName,
        [Required] string LastName,
        DateTime HireDate,
        string Email,
        List<string>? Skills,
        int DepartmentId);

    /// <summary>
    /// DTO for updating an existing employee.
    /// Requires first and last name, allows updating email, hire date, skills, and department.
    /// </summary>
    /// <param name="FirstName">First name of the employee (required).</param>
    /// <param name="LastName">Last name of the employee (required).</param>
    /// <param name="Email">Updated email address of the employee.</param>
    /// <param name="HireDate">Updated hire date of the employee.</param>
    /// <param name="Skills">Updated list of skills.</param>
    /// <param name="DepartmentId">Updated department identifier.</param>
    public record UpdateEmployeeDto(
        [Required] string FirstName,
        [Required] string LastName,
        string Email,
        DateTime HireDate,
        List<string>? Skills,
        int DepartmentId);

    /// <summary>
    /// DTO for adding a new skill to an employee.
    /// Requires a skill name.
    /// </summary>
    /// <param name="SkillName">Name of the skill to add (required).</param>
    public record AddSkillDto(
        [Required] string SkillName);

    /// <summary>
    /// Represents a skill with metadata including ID, name, description, and creation date.
    /// </summary>
    /// <param name="Id">Unique identifier of the skill.</param>
    /// <param name="Name">Name of the skill.</param>
    /// <param name="Description">Optional description of the skill.</param>
    /// <param name="CreatedAt">Date when the skill was created.</param>
    public record SkillDto(
        int Id,
        string Name,
        string? Description,
        DateTime CreatedAt);

    /// <summary>
    /// DTO for creating a new skill.
    /// Requires a name (max 120 characters) and optional description (max 1000 characters).
    /// </summary>
    /// <param name="Name">Name of the skill (required, max 120 characters).</param>
    /// <param name="Description">Optional description of the skill (max 1000 characters).</param>
    public record CreateSkillDto(
        [Required, StringLength(120)] string Name,
        [StringLength(1000)] string? Description);

    /// <summary>
    /// DTO for updating an existing skill.
    /// Requires an ID and name, allows updating description.
    /// </summary>
    /// <param name="Id">Unique identifier of the skill (required).</param>
    /// <param name="Name">Updated name of the skill (required, max 120 characters).</param>
    /// <param name="Description">Updated description of the skill (optional, max 1000 characters).</param>
    public record UpdateSkillDto(
        [Required] int Id,
        [Required, StringLength(120)] string Name,
        [StringLength(1000)] string? Description);
}
