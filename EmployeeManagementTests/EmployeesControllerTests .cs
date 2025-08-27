
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Tests
{
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

        private async Task<AppDbContext> SeedTestData()
        {

            _dbContext.Employees.AddRange(
                new Employee
                {
                    Id = 1,
                    FirstName = "Alice",
                    LastName = "Smith",
                    HireDate = new DateTime(2023, 1, 1),
                    Email = "alice@test.com",
                    EmployeeSkills = new List<EmployeeSkill>
                    {
                    new EmployeeSkill { Skill = new Skill { Name = "C#" } }
                    }
                },
                new Employee
                {
                    Id = 2,
                    FirstName = "Bob",
                    LastName = "Johnson",
                    HireDate = new DateTime(2024, 5, 5),
                    Email = "bob@test.com",
                    EmployeeSkills = new List<EmployeeSkill>
                    {
                    new EmployeeSkill { Skill = new Skill { Name = "SQL" } }
                    }
                },
                new Employee
                {
                    Id = 3,
                    FirstName = "Charlie",
                    LastName = "Brown",
                    HireDate = new DateTime(2025, 5, 5),
                    Email = "charlie@test.com",
                    EmployeeSkills = new List<EmployeeSkill>
                    {
                    new EmployeeSkill { Skill = new Skill { Name = "Java" } }
                    }
                }
            );


            await _dbContext.SaveChangesAsync();
            return _dbContext;
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
        [TestCase("FirstName", "John", "HireDate", "desc", 0, "")]
        [TestCase("FirstName", "Alice", "HireDate", "desc", 1, "Alice")]
        [TestCase("FirstName", "Alice", "LastName", "asc", 1, "Alice")]
        [TestCase("FirstName", "Alice", "FirstName", "asc", 1, "Alice")]
        [TestCase("LastName", "Johnson", "HireDate", "desc", 1, "Bob")]
        [TestCase("LastName", "Johnson", "LastName", "desc", 1, "Bob")]
        [TestCase("LastName", "Johnson", "LastName", "asc", 1, "Bob")]
        public async Task GetEmployees_WithSearchAndOrdering_ReturnsFilteredOrderedList(string searchField, string searchTerm, string orderBy, string direction, int count, string exptectedName)
        {

            var db = await SeedTestData();
            var controller = new EmployeesController(db);

            var filter = new FilterCollection
            {
                SearchField = searchField,
                SearchTerm = searchTerm,
                OrderBy = orderBy,
                Direction = direction
            };

            var result = await controller.GetEmployees(filter);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var employees = (result.Result as OkObjectResult)?.Value as IEnumerable<EmployeeDto>;

            var list = employees.ToList();
            Assert.That(list!.Count(), Is.EqualTo(count));
            if (count > 0)
                Assert.That(list[0].FirstName, Is.EqualTo(exptectedName));

        }

        [Test]
        public async Task GetEmployees_WithoutFilter_ReturnsAll()
        {
            // Arrange
            var db = await SeedTestData();
            var controller = new EmployeesController(db);

            var filter = new FilterCollection(); // no search/order

            // Act
            var result = await controller.GetEmployees(filter);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

            var employees = (result.Result as OkObjectResult)?.Value as IEnumerable<EmployeeDto>;

            var list = employees.ToList();
            Assert.That(list!.Count(), Is.EqualTo(3));
        }

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

            var dto = new UpdateEmployeeDto("EveUpdated", "JonesUpdated", emp.Email, DateTime.UtcNow);
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

        [Test]
        public async Task DeleteEmployees_WithValidIds_RemovesEmployeesAndReturnsNoContent()
        {

            var db = await SeedTestData();
            var controller = new EmployeesController(db);
            var idsToDelete = new List<int> { 1, 2 };


            var result = await controller.DeleteEmployees(idsToDelete);


            Assert.That(result, Is.TypeOf<NoContentResult>());

            var remaining = await db.Employees.ToListAsync();
            Assert.That(remaining.Count, Is.EqualTo(1));
            Assert.That(remaining[0].Id, Is.EqualTo(3));
        }

        [Test]
        public async Task DeleteEmployees_WithEmptyList_ReturnsBadRequest()
        {

            var db = await SeedTestData();
            var controller = new EmployeesController(db);


            var result = await controller.DeleteEmployees(new List<int>());


            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest!.Value, Is.EqualTo(StringConstants.NO_EMPLOYEE_ID));
        }

        [Test]
        public async Task DeleteEmployees_WithNonExistingIds_ReturnsNotFound()
        {
            var db = await SeedTestData();
            var controller = new EmployeesController(db);

            var result = await controller.DeleteEmployees(new List<int> { 99, 100 });

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFound = result as NotFoundObjectResult;
            Assert.That(notFound!.Value, Is.EqualTo(StringConstants.NO_MATCHING_EMPLOYEES));

            var remaining = await db.Employees.ToListAsync();
            Assert.That(remaining.Count, Is.EqualTo(3));
        }
    }


}
