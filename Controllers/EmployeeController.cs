using EmployeeManagement.API.Services;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Controller for managing Employees and their Skills.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(AppDbContext db, ILogger<EmployeesController> logger) : ControllerBase
{
    private readonly ILogger<EmployeesController> _logger;

   
    /// <summary>
    /// Gets all employees with optional filtering.
    /// </summary>
    /// <param name="filter">Filter parameters to apply.</param>
    /// <returns>List of employees.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees([FromQuery] FilterCollection filter)
    {
        return await ActionWrapper.ExecuteAsync<IEnumerable<EmployeeDto>>(
        logger,
        async () =>
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
                ))
                .ToListAsync();

            return employees;
        },
        "Fetched {Count} employees"
    );
    }

    /// <summary>
    /// Gets an employee by Id.
    /// </summary>
    /// <param name="id">Employee Id.</param>
    /// <returns>Employee details.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await GetEmployeeById(id);

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

    private async Task<ActionResult<EmployeeDto>> GetEmployeeDto(int id)
    {
        var employee = await GetEmployeeById(id);

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
    /// <summary>
    /// Gets all departments.
    /// </summary>
    /// <returns>List of departments.</returns>
    [HttpGet("departments")]
    public async Task<ActionResult<List<Department>>> GetDepartments()
    {
        var departments = await db.Departments.AsNoTracking().ToListAsync();
        return Ok(departments);
    }

    /// <summary>
    /// Gets all skills assigned to an employee.
    /// </summary>
    /// <param name="id">Employee Id.</param>
    /// <returns>List of EmployeeSkills.</returns>
    [HttpGet("{id:int}/skills")]
    public Task<ActionResult<List<EmployeeSkill>>> GetEmployeeSkills(int id)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => GetEmployeeSkillsInternal(id),
            "Fetched skills for employee {EmployeeId}", id
        );
    }

    private async Task <List<EmployeeSkill>> GetEmployeeSkillsInternal(int id)
    {
        var employeeSkills = db.EmployeeSkills
            .Where(emp => emp.EmployeeId == id)
            .AsNoTracking()
            .ToListAsync();
        return await employeeSkills;
    }


    /// <summary>
    /// Creates a new employee.
    /// </summary>
    /// <param name="dto">Employee data.</param>
    /// <returns>Created employee.</returns>
    [HttpPost]
    public Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => CreateEmployeeInternal(dto),
            "Created employee {FirstName} {LastName}", dto.FirstName, dto.LastName
        );
    }

    /// <summary>
    /// Internal method to handle employee creation logic.
    /// Returns EmployeeDto.
    /// </summary>
    private async Task<IActionResult> CreateEmployeeInternal(CreateEmployeeDto dto)
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



    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    /// <param name="id">Employee Id.</param>
    /// <param name="dto">Updated employee data.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
    {
        var employee = await GetEmployeeById(id);
        if (employee == null) return NotFound();

        var department = await db.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return BadRequest(StringConstants.INVALID_DEPARTMENT);

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.HireDate = dto.HireDate;
        employee.Email = dto.Email;
        employee.DepartmentId = dto.DepartmentId;

        if (dto.Skills != null && dto.Skills.Any())
        {
            foreach (var skillName in dto.Skills)
            {
                var skill = await db.Skills.FirstOrDefaultAsync(s => s.Name == skillName)
                            ?? new Skill { Name = skillName };
                if (!employee.EmployeeSkills.Any(es => es.Skill.Name == skillName))
                {
                    employee.EmployeeSkills.Add(new EmployeeSkill
                    {
                        Employee = employee,
                        Skill = skill
                    });
                }
            }
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes an employee by Id.
    /// </summary>
    /// <param name="id">Employee Id.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        db.Employees.Remove(employee);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes multiple employees.
    /// </summary>
    /// <param name="ids">List of employee Ids.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteEmployees([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
            return BadRequest(StringConstants.NO_EMPLOYEE_ID);

        var employees = await db.Employees
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        if (!employees.Any())
            return NotFound(StringConstants.NO_MATCHING_EMPLOYEES);

        db.Employees.RemoveRange(employees);
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Adds a skill to an employee.
    /// </summary>
    [HttpPost("{id:int}/skills/{skillId:int}")]
    public async Task<IActionResult> AddSkill(int id, int skillId)
    {
        var employee = await db.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return NotFound(StringConstants.NO_MATCHING_EMPLOYEES);

        var skill = await db.Skills.FindAsync(skillId);
        if (skill == null) return BadRequest(StringConstants.NO_SKILL);

        if (employee.EmployeeSkills.Any(es => es.SkillId == skillId))
            return BadRequest($"{StringConstants.SKILL_IN_USE} '{skill.Name}'.");

        employee.EmployeeSkills.Add(new EmployeeSkill
        {
            EmployeeId = id,
            SkillId = skillId
        });

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Removes a skill from an employee.
    /// </summary>
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

    // Private helper to get employee with skills and department
    private async Task<Employee?> GetEmployeeById(int id) =>
        await db.Employees
           .Include(e => e.Department)
           .Include(e => e.EmployeeSkills)
               .ThenInclude(es => es.Skill)
           .FirstOrDefaultAsync(e => e.Id == id);
}
