using EmployeeManagement.Data;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(AppDbContext db) : ControllerBase
{    
    
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees([FromQuery] FilterCollection filter)
    {
        var query = db.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .AsNoTracking();

        query = filter.ApplyAll(query);

        var employees = await query
            .Select(e => new EmployeeDto(
                e.Id,
                e.FirstName,
                e.LastName,
                e.HireDate,
                e.Email,
                e.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
                e.DepartmentId
            )).ToListAsync();

        return Ok(employees);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await db.Employees
       .Include(e => e.Department)
       .Include(e => e.EmployeeSkills)
           .ThenInclude(es => es.Skill)
       .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return NotFound();
     
        var dto = new EmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.HireDate,
            employee.Email,
            employee.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
            employee.DepartmentId
        );

        return Ok(dto);
    }

    [HttpGet("departments")]
    public async Task<ActionResult<Department>> GetDepartments()
    {
        var departments = await db.Departments.AsNoTracking().ToListAsync();

        return Ok(departments);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto dto)
    {
        var department = await db.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return BadRequest(StringConstants.INVALID_DEPARTMENT);

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            HireDate = dto.HireDate,
            Email = dto.Email,
            DepartmentId = dto.DepartmentId


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
            employee.Email,
            employee.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
            employee.DepartmentId
        );

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.HireDate = dto.HireDate;
        employee.Email = dto.Email;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        db.Employees.Remove(employee);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteEmployees([FromBody] List<int> ids)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest(StringConstants.NO_EMPLOYEE_ID);

        var employees = await db.Employees
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        if (employees.Count == 0)
            return NotFound(StringConstants.NO_MATCHING_EMPLOYEES);

        db.Employees.RemoveRange(employees);
        await db.SaveChangesAsync();

        return NoContent();
    }


    [HttpPost("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> AddSkill(int id, int skillId)
    {
        var employee = await db.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound(StringConstants.NO_MATCHING_EMPLOYEES);

        var skill = await db.Skills.FindAsync(skillId);
        if (skill == null)
            return BadRequest(StringConstants.NO_SKILL);

        if (employee.EmployeeSkills.Any(es => es.SkillId == skillId))
        {
            return BadRequest($"{StringConstants.SKILL_IN_USE} '{skill.Name}'.");
        }

        employee.EmployeeSkills.Add(new EmployeeSkill
        {
            EmployeeId = employee.Id,
            SkillId = skillId
        });

        await db.SaveChangesAsync();

        return NoContent();
    }

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
