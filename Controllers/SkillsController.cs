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
        return NotFound();
    }


    [HttpPost]
    public async Task<ActionResult<Skill>> CreateSkill([FromBody] Skill skill)
    {
        return NotFound();
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] Skill updated)
    {
        return NotFound();
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
       
        return NoContent();
    }
}
