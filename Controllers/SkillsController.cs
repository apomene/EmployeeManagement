using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EmployeeManagement.Controllers;

/// <summary>
/// Controller for managing skills in the system.
/// Provides endpoints for CRUD operations and CSV export.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SkillsController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Retrieves all skills.
    /// </summary>
    /// <returns>A list of <see cref="SkillDto"/> objects.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkills()
    {
        var skills = await db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkillDto(s.Id, s.Name, s.Description, s.CreatedAt))
            .ToListAsync();

        return Ok(skills);
    }

    /// <summary>
    /// Retrieves a specific skill by ID.
    /// </summary>
    /// <param name="id">The ID of the skill to retrieve.</param>
    /// <returns>The <see cref="SkillDto"/> of the requested skill, or 404 if not found.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SkillDto>> GetSkill(int id)
    {
        var skill = await FindSkillAsync(id);
        if (skill is null) return NotFound();

        return new SkillDto(skill.Id, skill.Name, skill.Description, skill.CreatedAt);
    }

    /// <summary>
    /// Creates a new skill.
    /// </summary>
    /// <param name="dto">The skill data transfer object containing Name and Description.</param>
    /// <returns>The created <see cref="SkillDto"/> with 201 status code, or 405 if a skill with the same name exists.</returns>
    [HttpPost]
    public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] CreateSkillDto dto)
    {
        var existing = await db.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name.ToLower() == dto.Name.ToLower());

        if (existing != null)
            return StatusCode(StatusCodes.Status405MethodNotAllowed, StringConstants.SKILL_EXISTS);

        var skill = new Skill
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var resultDto = new SkillDto(skill.Id, skill.Name, skill.Description, skill.CreatedAt);
        return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, resultDto);
    }

    /// <summary>
    /// Updates an existing skill.
    /// </summary>
    /// <param name="id">The ID of the skill to update.</param>
    /// <param name="dto">The updated skill data.</param>
    /// <returns>NoContent if successful, 400 if ID mismatch, or 404 if skill not found.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateSkillDto dto)
    {
        if (id != dto.Id) return BadRequest(StringConstants.ID_MISMATCH);

        var existing = await FindSkillAsync(id);
        if (existing is null) return NotFound();

        existing.Name = dto.Name;
        existing.Description = dto.Description;

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a skill by ID.
    /// Will return 400 if the skill is assigned to any employees.
    /// </summary>
    /// <param name="id">The ID of the skill to delete.</param>
    /// <returns>NoContent if deleted, 404 if not found, 400 if deletion is blocked.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var existing = await FindSkillAsync(id);
        if (existing is null) return NotFound();

        if (db.EmployeeSkills.Where(x => x.SkillId == id).Any())
            return BadRequest(StringConstants.FAIL_DELETE_SKILLS);

        try
        {
            db.Skills.Remove(existing);
            await db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            // DB restrict constraint blocks deletion
            return BadRequest(StringConstants.FAIL_DELETE_SKILLS);
        }
    }

    /// <summary>
    /// Exports all skills as a CSV file.
    /// </summary>
    /// <returns>A CSV file containing all skill properties (excluding EmployeeSkills).</returns>
    [HttpGet("/export")]
    public async Task<IActionResult> ExportSkillsToCsv()
    {
        var skills = await db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkillDto(s.Id, s.Name, s.Description, s.CreatedAt))
            .ToListAsync();

        if (!skills.Any())
        {
            return NotFound(StringConstants.NO_SKILLS);
        }

        var csvBuilder = new StringBuilder();
        var properties = typeof(SkillDto).GetProperties();

        csvBuilder.AppendLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

        foreach (var skill in skills)
        {
            var values = properties.Select(p =>
            {
                var val = p.GetValue(skill)?.ToString() ?? "";
                val = val.Replace("\"", "\"\"");
                return $"\"{val}\"";
            });

            csvBuilder.AppendLine(string.Join(",", values));
        }

        var fileName = $"skills_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

        return File(csvBytes, "text/csv", fileName);
    }

    /// <summary>
    /// Finds a skill entity by ID.
    /// </summary>
    /// <param name="id">The ID of the skill.</param>
    /// <returns>The <see cref="Skill"/> entity or null if not found.</returns>
    private async Task<Skill?> FindSkillAsync(int id) =>
        await db.Skills.FindAsync(id);
}
