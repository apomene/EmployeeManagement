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
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkills() =>
        await db.Skills.AsNoTracking().OrderBy(s => s.Name).ToListAsync();


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Skill>> GetSkill(int id)
    {
        var skill = await db.Skills.FindAsync(id);
        return skill is null ? NotFound() : skill;
    }


    [HttpPost]
    public async Task<ActionResult<Skill>> CreateSkill([FromBody] Skill skill)
    {
        skill.Id = 0;
        skill.CreatedAt = DateTime.UtcNow;
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] Skill updated)
    {
        if (id != updated.Id) return BadRequest("ID mismatch");
        var existing = await db.Skills.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Name = updated.Name;
        existing.Description = updated.Description;
        await db.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {

        var skill = await db.Skills.FindAsync(id);
        if (skill is null) return NotFound();
        db.Skills.Remove(skill);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
