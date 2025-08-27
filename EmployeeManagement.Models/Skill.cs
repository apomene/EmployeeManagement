using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models;

public class Skill
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public required string Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
}
