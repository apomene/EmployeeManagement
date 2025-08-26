
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


    [TestFixture]
    public class EmployeesControllerTests
    {
        private EmployeesController _controller;
        private AppDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            _dbContext = CreateInMemoryDbContext();
            _controller = new EmployeesController(_dbContext);
        }

        [TearDown]
        public void TearDown() => _dbContext.Dispose();

        private static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            return new AppDbContext(options);
        }

        private Employee CreateEmployee(string firstName, string lastName)
            => new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                HireDate = DateTime.UtcNow,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com"
            };

        [Test]
        public async Task CreateEmployee_AddsEmployee()
        {
            var dto = new CreateEmployeeDto(
                "John",
                "Doe",
                DateTime.UtcNow,
                "john.doe@yahoo.com",
                new List<string> { "C#" }
            );

            var result = await _controller.CreateEmployee(dto);
            var created = (result.Result as CreatedAtActionResult)?.Value as EmployeeDto;

            Assert.That(created, Is.Not.Null);
            Assert.That(created!.FirstName, Is.EqualTo("John"));
            Assert.That(created.Skills.Single(), Is.EqualTo("C#"));
        }

        [Test]
        public async Task GetEmployees_ReturnsEmployees()
        {
            _dbContext.Employees.Add(CreateEmployee("Alice", "Smith"));
            _dbContext.Employees.Add(CreateEmployee("Bob", "Brown"));
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetEmployees();
            var employees = (result.Result as OkObjectResult)?.Value as IEnumerable<EmployeeDto>;

            Assert.That(employees, Is.Not.Null);
            Assert.That(employees!.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetEmployee_ReturnsSingleEmployee()
        {
            var emp = CreateEmployee("Charlie", "Day");
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.GetEmployee(emp.Id);
            var employee = (result.Result as OkObjectResult)?.Value as EmployeeDto;

            Assert.That(employee, Is.Not.Null);
            Assert.That(employee!.FirstName, Is.EqualTo("Charlie"));
        }

        [Test]
        public async Task UpdateEmployee_ChangesData()
        {
            var emp = CreateEmployee("Eve", "Jones");
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var dto = new UpdateEmployeeDto("EveUpdated", "JonesUpdated" ,emp.Email,DateTime.UtcNow);
            var result = await _controller.UpdateEmployee(emp.Id, dto);

            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var updated = await _dbContext.Employees.FindAsync(emp.Id);
            Assert.That(updated!.FirstName, Is.EqualTo("EveUpdated"));
        }

        [Test]
        public async Task DeleteEmployee_RemovesEmployee()
        {
            var emp = CreateEmployee("Sam", "Fisher");
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.DeleteEmployee(emp.Id);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            Assert.That(!_dbContext.Employees.Any());
        }

        [Test]
        public async Task AddSkill_AssignsSkillToEmployee()
        {
            var emp = CreateEmployee("Mark", "Spencer");
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var dto = new AddSkillDto("Java");
            var result = await _controller.AddSkill(emp.Id, dto);

            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var updated = await _dbContext.Employees
                .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
                .FirstAsync();

            Assert.That(updated.EmployeeSkills.Any(es => es.Skill.Name == "Java"), Is.True);
        }

        [Test]
        public async Task RemoveSkill_DeletesSkillFromEmployee()
        {
            var skill = new Skill { Name = "Python" };
            var emp = CreateEmployee("Lucy", "Lawless");
            emp.EmployeeSkills.Add(new EmployeeSkill { Skill = skill });
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.RemoveSkill(emp.Id, skill.Id);

            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var updated = await _dbContext.Employees
                .Include(e => e.EmployeeSkills)
                .FirstAsync();

            Assert.That(!updated.EmployeeSkills.Any());
        }
    }

