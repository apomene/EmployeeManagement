using EmployeeManagement.API.Services;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Controller for managing Employees and their Skills.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(AppDbContext db, ILogger<EmployeesController> logger, IAuditLogger auditLogger) : ControllerBase
{

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
       StringConstants.LOG_EMPLOYEES_FETCHED
    );
    }

    /// <summary>
    /// Gets an employee by Id.
    /// </summary>
    /// <param name="id">Employee Id.</param>
    /// <returns>Employee details.</returns>
    [HttpGet("{id:int}")]
    public Task<IActionResult> GetEmployee(int id)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => GetEmployeeInternal(id),
            StringConstants.LOG_EMPLOYEE_FETCHED, id
        );
    }

    private async Task<IActionResult> GetEmployeeInternal(int id)
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
    public Task<ActionResult<IEnumerable<Department>>> GetDepartments()
    {
        return ActionWrapper.ExecuteAsync<IEnumerable<Department>>(
           logger,
           async () =>
           {
               var departments = await db.Departments.AsNoTracking().ToListAsync();
               return departments;
           },
           StringConstants.LOG_DEPARTMENTS_FETCHED
       );

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
            StringConstants.LOG_EMPLOYEE_SKILLS_FETCHED, id
        );
    }

    private async Task<List<EmployeeSkill>> GetEmployeeSkillsInternal(int id)
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
    public Task<IActionResult> CreateEmployee(EmployeeDto dto)
    {
        var result = ActionWrapper.ExecuteAsync(
            logger,
            () => CreateEmployeeInternal(dto),
            StringConstants.LOG_EMPLOYEE_CREATED, dto.FirstName, dto.LastName
        );
        if (result.IsCompleted && result.Result is ObjectResult objResult)
        {
            if (objResult.StatusCode > 200 && objResult.StatusCode < 300)
            {
                _ = auditLogger.LogChangeAsync(
                   entityName: "Employee",
                   entityId: dto.Email, // Using Email as unique Id of the audit log
                   action: "Create",
                   newValue: dto,
                   performedBy: "system"  // TO DO: Replace with actual user/scheduler info if we implement authentication
               );
            }
        }
        return result;
    }

    /// <summary>
    /// Internal method to handle employee creation logic.
    /// Returns EmployeeDto.
    /// </summary>
    private async Task<IActionResult> CreateEmployeeInternal(EmployeeDto dto)
    {
        var department = await db.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return BadRequest(StringConstants.INVALID_DEPARTMENT);

        if (await db.Employees.AnyAsync(e => e.Email == dto.Email))
        {
            return BadRequest($"An employee with email '{dto.Email}' already exists.");
        }

        var employee = new Employee
        {
            Id = dto.Id,
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
    public Task<IActionResult> UpdateEmployee(int id, EmployeeDto dto)
    {
        var result = ActionWrapper.ExecuteAsync(
            logger,
            () => UpdateEmployeeInternal(id, dto),
            StringConstants.LOG_EMPLOYEE_UPDATED, id
        );
        if (result.IsCompleted && result.Result is NoContentResult objResult)
        {
            if (objResult.StatusCode == 204)
            {
                _ = auditLogger.LogChangeAsync(
                   entityName: "Employee",
                   entityId: dto.Email, // Using Email as unique Id of the audit log
                   action: "Update",
                   newValue: dto,
                   performedBy: "system"  // TO DO: Replace with actual user/scheduler info if we implement authentication
               );
            }
        }
        return result;
    }

    private async Task<IActionResult> UpdateEmployeeInternal(int id, EmployeeDto dto)
    {
        var employee = await GetEmployeeById(id);
        if (employee == null) return NotFound();

        var department = await db.Departments.FindAsync(dto.DepartmentId);
        if (department == null) return BadRequest(StringConstants.INVALID_DEPARTMENT);

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
        var result = ActionWrapper.ExecuteAsync(
            logger,
            () => DeleteEmployeeInternal(id),
            StringConstants.LOG_EMPLOYEE_DELETED, id
        );      
        return await result;
    }

    private async Task<IActionResult> DeleteEmployeeInternal(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        db.Employees.Remove(employee);
        await db.SaveChangesAsync();
        var employeeDto = new EmployeeDto(  id,
                                            employee!.FirstName,
                                            employee.LastName,
                                            employee.HireDate,
                                            employee.Email,
                                            employee.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
                                            employee.DepartmentId
                                          );
        _ = auditLogger.LogChangeAsync(
           entityName: "Employee",
           entityId: employeeDto.Email, // Using Email as unique Id of the audit log
           action: "Delete",
           newValue: employeeDto,
           performedBy: "system"); // TO DO: Replace with actual user/scheduler info if we implement authentication
        return NoContent();
    }


    /// <summary>
    /// Deletes multiple employees.
    /// </summary>
    /// <param name="ids">List of employee Ids.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete]
    public Task<IActionResult> DeleteEmployees([FromBody] List<int> ids)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => DeleteEmployeesInternal(ids),
            StringConstants.LOG_EMPLOYEES_DELETED
        );
    }

    private async Task<IActionResult> DeleteEmployeesInternal(List<int> ids)
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
    public Task<IActionResult> AddSkill(int id, int skillId)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => AddSkillInternal(id, skillId),
            StringConstants.LOG_SKILL_ADDED, skillId, id
        );
    }

    private async Task<IActionResult> AddSkillInternal(int id, int skillId)
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

        var employeeDto = new EmployeeDto(id,
                                            employee!.FirstName,
                                            employee.LastName,
                                            employee.HireDate,
                                            employee.Email,
                                            employee.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
                                            employee.DepartmentId
                                          );
        _ = auditLogger.LogChangeAsync(
           entityName: "Employee",
           entityId: employeeDto.Email, // Using Email as unique Id of the audit log
           action: "ADDED_SKILL",
           newValue: employeeDto,
           performedBy: "system"); // TO DO: Replace with actual user/scheduler info if we implement authentication
        return NoContent();
    }


    /// <summary>
    /// Removes a skill from an employee.
    /// </summary>
    [HttpDelete("{id:int}/skills/{skillId:int}")]
    public Task<IActionResult> RemoveSkill(int id, int skillId)
    {
        return ActionWrapper.ExecuteAsync(
            logger,
            () => RemoveSkillInternal(id, skillId),
            StringConstants.LOG_SKILL_REMOVED, skillId, id
        );
    }

    private async Task<IActionResult> RemoveSkillInternal(int id, int skillId)
    {
        var employeeSkill = await db.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == id && es.SkillId == skillId);

        if (employeeSkill == null) return NotFound();

        db.EmployeeSkills.Remove(employeeSkill);
        await db.SaveChangesAsync();

        var employee = await db.Employees
           .Include(e => e.EmployeeSkills)
           .ThenInclude(es => es.Skill)
           .FirstOrDefaultAsync(e => e.Id == id);
        var employeeDto = new EmployeeDto(id,
                                           employee!.FirstName,
                                           employee.LastName,
                                           employee.HireDate,
                                           employee.Email,
                                           employee.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
                                           employee.DepartmentId
                                         );
        _ = auditLogger.LogChangeAsync(
           entityName: "Employee",
           entityId: employeeDto.Email, // Using Email as unique Id of the audit log
           action: "REMOVED_SKILL",
           newValue: employeeDto,
           performedBy: "system"); // TO DO: Replace with actual user/scheduler info if we implement authentication

        return NoContent();
    }

    public Task<ActionResult<List<Employee>>> FilterBySkills([FromQuery] List<int> skillIds, FilterCollection filter)
    {
        return  ActionWrapper.ExecuteAsync(
             logger,
             () => FilterBySkillsInternal(skillIds,filter),
             StringConstants.LOG_EMPLOYEE_SKILLS_FETCHED, filter
         );
    }


    private async Task<List<Employee>> FilterBySkillsInternal(List<int> skillIds, FilterCollection filter)
    {
        var result = await filter.GetEmployeesBySkillsAsync(skillIds);
        return result;
    }


    // Private helper to get employee with skills and department
    private async Task<Employee?> GetEmployeeById(int id) =>
        await db.Employees
           .Include(e => e.Department)
           .Include(e => e.EmployeeSkills)
               .ThenInclude(es => es.Skill)
           .FirstOrDefaultAsync(e => e.Id == id);
}
