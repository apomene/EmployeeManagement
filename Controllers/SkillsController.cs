using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Models;
using EmployeeManagement.Data;
using Microsoft.EntityFrameworkCore;


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

        db.Skills.Remove(existing);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Skill?> FindSkillAsync(int id) =>
        await db.Skills.FindAsync(id);
}

