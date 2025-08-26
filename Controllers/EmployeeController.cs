using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(AppDbContext db) : ControllerBase
{
    // GET api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(
        string? orderBy = null, string? direction = "asc", string? search = null)
    {
        var query = db.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.FirstName.Contains(search) || e.LastName.Contains(search));
        }

        query = orderBy?.ToLower() switch
        {
            "surname" => direction == "desc" ? query.OrderByDescending(e => e.LastName) : query.OrderBy(e => e.LastName),
            "hiredate" => direction == "desc" ? query.OrderByDescending(e => e.HireDate) : query.OrderBy(e => e.HireDate),
            _ => query.OrderBy(e => e.Id)
        };

        var employees = await query
            .Select(e => new EmployeeDto(
                e.Id,
                e.FirstName,
                e.LastName,
                e.HireDate,
                e.EmployeeSkills.Select(es => es.Skill.Name).ToList()
            )).ToListAsync();

        return Ok(employees);
    }

    // GET api/employees/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var e = await db.Employees
            .Include(emp => emp.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(emp => emp.Id == id);

        if (e == null) return NotFound();

        var dto = new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.HireDate,
            e.EmployeeSkills.Select(es => es.Skill.Name).ToList()
        );

        return Ok(dto);
    }

    // POST api/employees
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            HireDate = dto.HireDate
        };

        if (dto.Skills != null && dto.Skills.Any())
        {
            foreach (var skillName in dto.Skills)
            {
                var skill = await db.Skills.FirstOrDefaultAsync(s => s.Name == skillName)
                            ?? new Skill { Name = skillName };
                employee.EmployeeSkills.Add(new EmployeeSkill
                {
                    Employee = employee,
                    Skill = skill
                });
            }
        }

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var resultDto = new EmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.HireDate,
            employee.EmployeeSkills.Select(es => es.Skill.Name).ToList()
        );

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, resultDto);
    }

    // PUT api/employees/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.HireDate = dto.HireDate;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/employees/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        db.Employees.Remove(employee);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // POST api/employees/{id}/skills
    [HttpPost("{id:int}/skills")]
    public async Task<IActionResult> AddSkill(int id, AddSkillDto dto)
    {
        var employee = await db.Employees
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return NotFound();

        var skill = await db.Skills.FirstOrDefaultAsync(s => s.Name == dto.SkillName)
                    ?? new Skill { Name = dto.SkillName };

        if (!employee.EmployeeSkills.Any(es => es.Skill.Name == dto.SkillName))
        {
            employee.EmployeeSkills.Add(new EmployeeSkill
            {
                Employee = employee,
                Skill = skill
            });
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    // DELETE api/employees/{id}/skills/{skillId}
    [HttpDelete("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> RemoveSkill(int id, int skillId)
    {
        var employeeSkill = await db.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == id && es.SkillId == skillId);

        if (employeeSkill == null) return NotFound();

        db.EmployeeSkills.Remove(employeeSkill);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
