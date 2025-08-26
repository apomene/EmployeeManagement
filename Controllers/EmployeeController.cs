using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Employee>> GetEmployees(
        string? orderBy = null, string? direction = "asc", string? search = null)
    {
        return NotFound();
    }

    [HttpGet("{id}")]
    public ActionResult<Employee> GetEmployee(int id)
    {
        return NotFound();
    }

    [HttpPost]
    public ActionResult<Employee> CreateEmployee(Employee employee)
    {
        return NotFound();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateEmployee(int id, Employee employee)
    {
        return NotFound();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteEmployee(int id)
    {
        return NotFound();
    }

    [HttpPost("{id}/skills")]
    public IActionResult AddSkill(int id, Skill skill)
    {
        return NotFound();
    }

    [HttpDelete("{id}/skills/{skillId}")]
    public IActionResult RemoveSkill(int id, int skillId)
    {
        return NotFound();
    }
}
