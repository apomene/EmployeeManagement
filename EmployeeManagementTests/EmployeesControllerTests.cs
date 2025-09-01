
using EmployeeManagement.API.Services;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Mongo2Go;
using MongoDB.Driver;
using System.Diagnostics;

namespace EmployeeManagement.Tests
{
    [TestFixture]
    public class EmployeesControllerTests
    {
        private EmployeesController _controller;
        private AppDbContext _dbContext;
        private MongoDbRunner _mongoRunner;
        private IAuditLogger _auditLogger;
        private readonly NullLogger<EmployeesController> _logger = NullLogger<EmployeesController>.Instance;

        [SetUp]
        public void Setup()
        {
            // Start temporary MongoDB
            _mongoRunner = MongoDbRunner.Start();

            var client = new MongoClient(_mongoRunner.ConnectionString);
            var database = client.GetDatabase("EmployeeAuditTestDb");

            // use real AuditLogger
            _auditLogger = new AuditLogger(database, NullLogger<AuditLogger>.Instance);

            _dbContext = CreateInMemoryDbContext();
            _controller = new EmployeesController(_dbContext, _logger, _auditLogger);
        }



        [TearDown]
        public void TearDown() 
        {
            _dbContext.Dispose();
            _mongoRunner?.Dispose();
        }
        

        private static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            return new AppDbContext(options);
        }

        private async Task<AppDbContext> SeedTestData()
        {
            var department = new Department { Id = 1, Name = "IT" };
            var skill1 = new Skill { Id = 23, Name = "Angular" };
            var skill2 = new Skill { Id = 32, Name = "MongoDB" };

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
                },
                new Employee
                {
                    Id = 4,
                    FirstName = "Apo",
                    LastName = "Mene",
                    Email = "apo@example.com",
                    HireDate = DateTime.UtcNow,
                    Department = department,
                    DepartmentId = department.Id,
                    EmployeeSkills = new List<EmployeeSkill> { new EmployeeSkill { EmployeeId = 4, Skill = skill1 } }
                }                             
            );

           
            _dbContext.Departments.Add(department);
            _dbContext.Skills.AddRange(skill1, skill2);

            await _dbContext.SaveChangesAsync();
            return _dbContext;
        }

        private void SeedEmployeeSkills()
        {
            
            var skill1 = new Skill { Id = 23, Name = "Angular" };
            var skill2 = new Skill { Id = 32, Name = "MongoDB" };
            var employee = new Employee 
              { Id = 17, 
                FirstName = "Bill",
                LastName = "Mene",
                Email = "apo@example.com",
                HireDate = DateTime.UtcNow,
            };
            _dbContext.Employees.Add(employee);


            _dbContext.Skills.AddRange(skill1, skill2);

            var empSkills = new List<EmployeeSkill>
        {
            new EmployeeSkill { EmployeeId = 17, SkillId = 23, AssignedAt = System.DateTime.UtcNow },
            new EmployeeSkill { EmployeeId = 17, SkillId = 32, AssignedAt = System.DateTime.UtcNow }
        };
            _dbContext.EmployeeSkills.AddRange(empSkills);
            _dbContext.SaveChanges();
        }

        private Employee CreateEmployee(string firstName, string lastName, Department department = null)
        {
            return new Employee
            {
                FirstName = firstName,
                LastName = lastName,
                HireDate = DateTime.UtcNow,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                Department = department,
                DepartmentId = department?.Id ?? 1
            };
        }

        private Department CrteateDepartment(int Id, string Name)
        {
            return new Department
            {
                Id = Id,
                Name = Name,
                Description = $"{Name} Department"
            };
        }
                

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
            var controller = new EmployeesController(db, _logger, _auditLogger);

            var filter = new FilterCollection
            {
                SearchField = searchField,
                SearchTerm = searchTerm,
                OrderBy = orderBy,
                Direction = direction
            };

            var result = await controller.GetEmployees(filter);

            var employees = result.Value as IEnumerable<EmployeeDto>;

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
            var controller = new EmployeesController(db, _logger, _auditLogger);

            var filter = new FilterCollection(); // no search/order

            // Act
            var result = await controller.GetEmployees(filter);


            var employees = result.Value as IEnumerable<EmployeeDto>;

            var list = employees.ToList();
            Assert.That(list!.Count(), Is.EqualTo(4));
        }

        [Test]
        public async Task CreateEmployee_AddsEmployee()
        {
            _dbContext.Departments.AddRange(new Department
            {
                Id = 1,
                Name = "IT",
                Description = "The Information Technology department"
            });

            var dto = new CreateEmployeeDto(
                "John",
                "Doe",
                DateTime.UtcNow,
                "john.doe@yahoo.com",
                new List<string> { "C#" },
                1
            );

            var result = await _controller.CreateEmployee(dto);
            Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
            var created = _dbContext.Employees.FirstOrDefault(id => id.Id == 1);

            Assert.That(created, Is.Not.Null);
            Assert.That(created!.FirstName, Is.EqualTo("John"));
        }

        [Test]
        public async Task CreateEmployee_InvalidDepartment_ReturnsBadRequest()
        {
            var dto = new CreateEmployeeDto(
                "John",
                "Doe",
                DateTime.UtcNow,
                "john.doe@yahoo.com",
                new List<string> { "C#" },
                999 // Non-existing department ID
            );

            var result = await _controller.CreateEmployee(dto);
           

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest!.Value, Is.EqualTo(StringConstants.INVALID_DEPARTMENT));
        }


        [Test]
        public async Task GetEmployee_ReturnsSingleEmployee()
        {
            var department = CrteateDepartment(1, "HR");
            var emp = CreateEmployee("Charlie", "Day", department);
            
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            // Act
            var actionResult = await _controller.GetEmployee(emp.Id);

            // Assert
            Assert.That(actionResult, Is.InstanceOf<OkObjectResult>());

            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var employee = okResult!.Value as EmployeeDto;
            Assert.That(employee, Is.Not.Null);
            Assert.That(employee!.FirstName, Is.EqualTo("Charlie"));
        }

        [Test]
        public async Task UpdateEmployee_InvalidDepartment_ReturnsBadRequest()
        {
            var department = CrteateDepartment(1, "HR");
            var emp = CreateEmployee("Eve", "Jones", department);
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var dto = new UpdateEmployeeDto("EveUpdated", "JonesUpdated", emp.Email, DateTime.UtcNow, new List<string>(), 999);
            var result = await _controller.UpdateEmployee(emp.Id, dto);
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            
        }

        [Test]
        public async Task UpdateEmployee_ChangesData()
        {
            var department = CrteateDepartment(1, "HR");
            var emp = CreateEmployee("Eve", "Jones", department);
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var dto = new UpdateEmployeeDto("EveUpdated", "JonesUpdated", emp.Email, DateTime.UtcNow,new List<string>(),department.Id);
            var result = await _controller.UpdateEmployee(emp.Id, dto);

            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var updated = await _dbContext.Employees.FindAsync(emp.Id);
            Assert.That(updated!.FirstName, Is.EqualTo("EveUpdated"));
        }

        [Test]
        public async Task UpdateEmployee_WithExistingSkill_DoesNotDuplicate()
        {
            // Arrange
            var db = await SeedTestData();
            var controller = new EmployeesController(db, _logger, _auditLogger);
            var dto = new UpdateEmployeeDto("John", "Doe", "john@example.com", DateTime.UtcNow, new List<string> { "Angular", "MongoDB" },1);

            var result = await controller.UpdateEmployee(4, dto);

            var employee = await db.Employees
                .Include(e => e.EmployeeSkills)
                .ThenInclude(es => es.Skill)
                .FirstAsync(e => e.Id == 4);

            Assert.That(employee.EmployeeSkills.Count, Is.EqualTo(2));
            Assert.That(employee.EmployeeSkills.Any(es => es.Skill.Name == "Angular"), Is.True);
            Assert.That(employee.EmployeeSkills.Any(es => es.Skill.Name == "MongoDB"), Is.True);
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
        public async Task AddSkill_WithNoEmployee_NotFOund()
        {
            var emp = CreateEmployee("Mark", "Spencer"); //does not exist in DB
           

            var dto = new AddSkillDto("Java");
            _dbContext.Skills.Add(new Skill { Id = 3, Name = "Java" });
            var result = await _controller.AddSkill(emp.Id, 3);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());

            var badRequest = result as NotFoundObjectResult;
            Assert.That(badRequest!.Value, Is.EqualTo(StringConstants.NO_MATCHING_EMPLOYEES));
        }

        [Test]
        public async Task AddSkill_AssignsSkillToEmployee()
        {
            var emp = CreateEmployee("Mark", "Spencer");
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var dto = new AddSkillDto("Java");
            _dbContext.Skills.Add(new Skill { Id = 3, Name = "Java" });
            var result = await _controller.AddSkill(emp.Id, 3);

            Assert.That(result, Is.InstanceOf<NoContentResult>());

            var updated = await _dbContext.Employees
                .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
                .FirstAsync();

            Assert.That(updated.EmployeeSkills.Any(es => es.Skill.Name == "Java"), Is.True);
        }

        [Test]
        public async Task AddSkill_WithInvalidId_ReturnBadRequest()
        {
            var emp = CreateEmployee("Mark", "Spencer");
            _dbContext.Employees.Add(emp);
            await _dbContext.SaveChangesAsync();

            var dto = new AddSkillDto("Java");
            _dbContext.Skills.Add(new Skill { Id = 3, Name = "Java" });
            var result = await _controller.AddSkill(emp.Id, 2); //Invalid ID

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest!.Value, Is.EqualTo(StringConstants.NO_SKILL));
        }

        [Test]
        public async Task AddSkill_SkillAlreadyAssigned_ReturnsBadRequest()
        {
            var db = await SeedTestData();
            var dto = new AddSkillDto("Java");
            db.Skills.Add(new Skill { Id = 1, Name = "Java" });
            var controller = new EmployeesController(db, _logger, _auditLogger);

            var employee = await db.Employees
                .Include(e => e.EmployeeSkills)
                .FirstAsync();
            employee.EmployeeSkills.Add(new EmployeeSkill { EmployeeId = employee.Id, SkillId = 1 });
            await db.SaveChangesAsync();

            var result = await controller.AddSkill(employee.Id, 1);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest!.Value.ToString().Contains(StringConstants.SKILL_IN_USE));
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
            var controller = new EmployeesController(db, _logger, _auditLogger);
            var idsToDelete = new List<int> { 1, 2 };


            var result = await controller.DeleteEmployees(idsToDelete);


            Assert.That(result, Is.TypeOf<NoContentResult>());

            var remaining = await db.Employees.ToListAsync();
            Assert.That(remaining.Count, Is.EqualTo(2));
            Assert.That(remaining[0].Id, Is.EqualTo(3));
        }

        [Test]
        public async Task DeleteEmployees_WithEmptyList_ReturnsBadRequest()
        {

            var db = await SeedTestData();
            var controller = new EmployeesController(db, _logger, _auditLogger);


            var result = await controller.DeleteEmployees(new List<int>());


            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest!.Value, Is.EqualTo(StringConstants.NO_EMPLOYEE_ID));
        }

        [Test]
        public async Task DeleteEmployees_WithNonExistingIds_ReturnsNotFound()
        {
            var db = await SeedTestData();
            var controller = new EmployeesController(db, _logger, _auditLogger);

            var result = await controller.DeleteEmployees(new List<int> { 99, 100 });

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFound = result as NotFoundObjectResult;
            Assert.That(notFound!.Value, Is.EqualTo(StringConstants.NO_MATCHING_EMPLOYEES));

            var remaining = await db.Employees.ToListAsync();
            Assert.That(remaining.Count, Is.EqualTo(4));
        }

        [Test]
        public async Task GetEmployeeSkills_ReturnsOk_WithEmployeeSkills()
        {
            SeedEmployeeSkills();
            var result = await _controller.GetEmployeeSkills(17);

            var skills = result.Value as List<EmployeeSkill>;

            Assert.That(skills, Is.Not.Null);
            Assert.That(skills.Count, Is.EqualTo(2));

            Assert.That(skills.Any(s => s.SkillId == 23));
            Assert.That(skills.Any(s => s.SkillId == 32));
        }

        [Test]
        public async Task GetEmployeeSkills_ReturnsOk_WithEmptyList_WhenNoSkills()
        {
            // Act
            var result = await _controller.GetEmployeeSkills(999); // Non-existing employee

            // Assert
            var skills = result.Value as List<EmployeeSkill>;

            Assert.That(skills, Is.Not.Null);
            Assert.That(skills.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CreateEmployee_Should_Write_AuditLog()
        {
            // Arrange: create department
            _dbContext.Departments.Add(new Department
            {
                Id = 1,
                Name = "IT",
                Description = "The Information Technology department"
            });
            await _dbContext.SaveChangesAsync();

            var dto = new CreateEmployeeDto(
                "John",
                "Doe",
                DateTime.UtcNow,
                "john.doe@yahoo.com",
                new List<string> { "C#" },
                1
            );

            // Act: call API (fire-and-forget logging)
            var result = await _controller.CreateEmployee(dto);

            // Assert: wait for audit log to appear in MongoDB
            var client = new MongoClient(_mongoRunner.ConnectionString);
            var db = client.GetDatabase("EmployeeAuditTestDb");
            var collection = db.GetCollection<AuditLogEntry>("AuditLogs");

            AuditLogEntry? log = null;
            var timeout = TimeSpan.FromSeconds(5); // maximum wait
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed < timeout)
            {
                log = await collection.Find(FilterDefinition<AuditLogEntry>.Empty)
                                      .FirstOrDefaultAsync();
                if (log != null)
                    break;

                await Task.Delay(50); // small delay to avoid busy-wait
            }

            Assert.That(log, Is.Not.Null, "Audit log was not written in time.");
            Assert.That(log!.Action, Is.EqualTo("Create"));
            Assert.That(log.EntityName, Is.EqualTo("Employee"));
            Assert.That(log.EntityId, Is.EqualTo(dto.Email));
        }


    }

}
