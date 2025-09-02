using EmployeeManagement.Data;
using EmployeeManagement.Models;
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
public class SkillsController(AppDbContext db, ILogger<EmployeesController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves all skills.
    /// </summary>
    /// <returns>A list of <see cref="SkillDto"/> objects.</returns>
    [HttpGet]
    public Task<ActionResult<IEnumerable<SkillDto>>> GetSkills()
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            GetSkillsInternal,
            StringConstants.LOG_SKILLS_FETCHED
        );
    }

    private async Task<IEnumerable<SkillDto>> GetSkillsInternal()
    {
        return await db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkillDto(s.Id, s.Name, s.Description, s.CreatedAt))
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific skill by ID.
    /// </summary>
    /// <param name="id">The ID of the skill to retrieve.</param>
    /// <returns>The <see cref="SkillDto"/> of the requested skill, or 404 if not found.</returns>
    [HttpGet("{id:int}")]
    public Task<IActionResult> GetSkill(int id)
    {
        return ActionWrapper.ExecuteAsync(logger, () => GetSkillInternal(id), StringConstants.LOG_SKILL_FETCHED, id);
    }

    private async Task<IActionResult> GetSkillInternal(int id)
    {
        var skill = await FindSkillAsync(id);
        if (skill == null) return NotFound(StringConstants.NO_SKILL);

        var dto = new SkillDto(skill.Id, skill.Name, skill.Description, skill.CreatedAt);
        return Ok(dto);
    }


    /// <summary>
    /// Creates a new skill.
    /// </summary>
    /// <param name="dto">The skill data transfer object containing Name and Description.</param>
    /// <returns>The created <see cref="SkillDto"/> with 201 status code, or 405 if a skill with the same name exists.</returns>
   // POST: api/skills
    [HttpPost]
    public Task<IActionResult> CreateSkill([FromBody] CreateSkillDto dto)
    {
        var result =  ActionWrapper.ExecuteAsync(
            logger,
            () => CreateSkillInternal(dto),
            StringConstants.LOG_SKILL_CREATED, dto.Name
        );
        return result;
    }

    private async Task<IActionResult> CreateSkillInternal(CreateSkillDto dto)
    {
        var existing = await db.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name.ToLower() == dto.Name.ToLower());

        if (existing != null)
             return BadRequest(StringConstants.SKILL_EXISTS);

        var skill = new Skill
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        return Created();
    }


    /// <summary>
    /// Updates an existing skill.
    /// </summary>
    /// <param name="id">The ID of the skill to update.</param>
    /// <param name="dto">The updated skill data.</param>
    /// <returns>NoContent if successful, 400 if ID mismatch, or 404 if skill not found.</returns>
    [HttpPut("{id:int}")]
    public Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateSkillDto dto)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => UpdateSkillInternal(id, dto),
            StringConstants.LOG_SKILL_UPDATED, id
        );
    }

    private async Task<IActionResult> UpdateSkillInternal(int id, UpdateSkillDto dto)
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
    public Task<IActionResult> DeleteSkill(int id)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => DeleteSkillInternal(id),
            StringConstants.LOG_SKILL_DELETED, id
        );
    }

    private async Task<IActionResult> DeleteSkillInternal(int id)
    {
        var existing = await FindSkillAsync(id);
        if (existing is null) return NotFound();

        if (db.EmployeeSkills.Any(x => x.SkillId == id))
            return BadRequest(StringConstants.FAIL_DELETE_SKILLS);

        try
        {
            db.Skills.Remove(existing);
            await db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return BadRequest(StringConstants.FAIL_DELETE_SKILLS);
        }
    }

    /// <summary>
    /// Exports all skills as a CSV file.
    /// </summary>
    /// <returns>A CSV file containing all skill properties (excluding EmployeeSkills).</returns>
    [HttpGet("/export")]
    public Task<IActionResult> ExportSkillsToCsv()
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            ExportSkillsToCsvInternal,
            StringConstants.LOG_SKILLS_EXPORTED
        );
    }

    private async Task<IActionResult> ExportSkillsToCsvInternal()
    {
        var skills = await db.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkillDto(s.Id, s.Name, s.Description, s.CreatedAt))
            .ToListAsync();

        if (!skills.Any()) return NotFound(StringConstants.NO_SKILLS);

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
