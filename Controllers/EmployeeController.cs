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

            var employees = await query.Select(e => Helpers.ToEmployeeDto(e)) .ToListAsync();

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

        var dto = Helpers.ToEmployeeDto(employee!);

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
    public async Task<IActionResult> CreateEmployee(EmployeeDto dto)
    {
        var result =  await ActionWrapper.ExecuteAsync(
            logger,
            () => CreateEmployeeInternal(dto),
            StringConstants.LOG_EMPLOYEE_CREATED, dto.FirstName, dto.LastName);
      
        LogAuditIfSuccessful<IActionResult>(result, StringConstants.AUDIT_CREATE, dto);
        
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

        await AssignSkillsToEmployee(employee, dto.Skills);

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var resultDto = Helpers.ToEmployeeDto(employee!);

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, resultDto);
    }



    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    /// <param name="id">Employee Id.</param>
    /// <param name="dto">Updated employee data.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, EmployeeDto dto)
    {
        var result = await ActionWrapper.ExecuteAsync(
            logger,
            () => UpdateEmployeeInternal(id, dto),
            StringConstants.LOG_EMPLOYEE_UPDATED, id
        );

        LogAuditIfSuccessful<IActionResult>(result, StringConstants.AUDIT_UPDATE, dto);
   
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

        await AssignSkillsToEmployee(employee, dto.Skills);
    
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
        var employeeDto = Helpers.ToEmployeeDto(employee!);

        var result =  NoContent();
        LogAuditIfSuccessful<IActionResult>(result, StringConstants.AUDIT_DELETE, employeeDto);
        return result;
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

        if (employees == null) return NotFound();

        if (!employees.Any())
            return NotFound(StringConstants.NO_MATCHING_EMPLOYEES);

        db.Employees.RemoveRange(employees);
        await db.SaveChangesAsync();
        var result = NoContent();
        foreach (var emp in employees)
        {
            var employeeDto = Helpers.ToEmployeeDto(emp);
            LogAuditIfSuccessful<IActionResult>(result, StringConstants.AUDIT_DELETE, employeeDto);
        }
        return result;
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

        var employeeDto = Helpers.ToEmployeeDto(employee!);

        var result = NoContent();
        LogAuditIfSuccessful<IActionResult>(result, StringConstants.AUDIT_SKILL_ADD, employeeDto);
        return result;
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
        var employee = await db.Employees
          .Include(e => e.EmployeeSkills)
          .ThenInclude(es => es.Skill)
          .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return NotFound();
        var employeeSkill = await db.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == id && es.SkillId == skillId);

        if (employeeSkill == null) return NotFound();

        db.EmployeeSkills.Remove(employeeSkill);
        await db.SaveChangesAsync();  

        var employeeDto = Helpers.ToEmployeeDto(employee);
        var result = NoContent();
        LogAuditIfSuccessful<IActionResult>(result, StringConstants.AUDIT_SKILL_REMOVE, employeeDto);
        return result;
    }

    /// <summary>
    /// Gets employees filtered by skills.
    /// </summary>
    /// <param name="skillIds">List of skill IDs to filter by. Empty list = all employees.</param>
    /// <returns>List of employees with department and skills.</returns>
    [HttpGet("filter-by-skills")]
    public Task<ActionResult<List<EmployeeDto>>> FilterBySkills([FromQuery] List<int> skillIds)
    {
        var skills = skillIds ??= new List<int>();
        var skillIdsMessage = (skillIds != null && skillIds.Any())
            ? string.Join(",", skillIds)
            : "ALL";

        return ActionWrapper.ExecuteAsync(
            logger,
            () => FilterBySkillsInternal(skills),
            $"{StringConstants.LOG_EMPLOYEES_FETCHED} with skill_IDs:{skillIdsMessage}"
        );
    }


    private async Task<List<EmployeeDto>> FilterBySkillsInternal(List<int> skillIds)
    {
        var filter = new FilterCollection(db);
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
   

    // Private helper to safely assign/update employee skills
    private async Task AssignSkillsToEmployee(Employee employee, IEnumerable<string>? skills)
    {
        if (skills == null || !skills.Any())
            return;

        // Ensure EmployeeSkills is initialized
        employee.EmployeeSkills ??= new List<EmployeeSkill>();

        foreach (var skillName in skills.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            // Try to find existing skill by name (case-insensitive match)
            var skill = await db.Skills
                .FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower())
                ?? new Skill { Name = skillName };

            // Skip if already assigned (case-insensitive match)
            var hasSkill = employee.EmployeeSkills
                .Any(es => es.Skill != null && es.Skill.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (!hasSkill)
            {
                employee.EmployeeSkills.Add(new EmployeeSkill
                {
                    Employee = employee,
                    Skill = skill
                });
            }
        }
    }

    // Private helper to log changes safely after a successful action
    private void LogAuditIfSuccessful<T>(
        IActionResult result,
        string action,
        EmployeeDto dto)
    {
        // Only log if result indicates success
        if (result is ObjectResult objResult && objResult.StatusCode is >= 200 and < 300 ||
            result is NoContentResult)
        {
            _ = auditLogger.LogChangeAsync(
                entityName: "Employee",
                entityId: dto.Email, // Using Email as unique Id of the audit log
                action: action,
                newValue: dto,
                performedBy: "system" // TODO: replace with actual user/scheduler info once authentication is added
            );
        }
    }


}
