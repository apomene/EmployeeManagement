using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;


namespace EmployeeManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController(AppDbContext db) : ControllerBase
{    
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SkillDto>> GetSkill(int id)
    {
        var skill = await FindSkillAsync(id);
        if (skill is null) return NotFound();

        return new SkillDto(skill.Id, skill.Name, skill.Description, skill.CreatedAt);
    }

    [HttpPost]
    public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] CreateSkillDto dto)
    {
        // Check if skill with same name exists
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var existing = await FindSkillAsync(id);
        if (existing is null) return NotFound();

        if (db.EmployeeSkills.Where(x=>x.SkillId ==id).Any())
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
           
        csvBuilder.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        
        foreach (var skill in skills)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(skill, null);
                return value is DateTime dt ? dt.ToString("o") : value?.ToString();
            });
            csvBuilder.AppendLine(string.Join(",", values));
        }

        var fileName = $"skills_export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());

        return File(csvBytes, "text/csv", fileName);
    }

    private async Task<Skill?> FindSkillAsync(int id) =>
        await db.Skills.FindAsync(id);
}

