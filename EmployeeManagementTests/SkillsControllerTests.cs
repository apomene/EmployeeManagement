using EmployeeManagement.Controllers;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;


namespace EmployeeManagement.Tests
{
    [TestFixture]
    public class SkillsControllerTests
    {
        private const int SeedSkillId = 1;
        private const string SeedSkillName = "TestSkill";

        private AppDbContext _dbContext = null!;
        private SkillsController _controller = null!;
        private readonly NullLogger<EmployeesController> _logger = NullLogger<EmployeesController>.Instance;

        [SetUp]
        public void Setup()
        {
            _dbContext = CreateDbContextWithSeed();
            _controller = new SkillsController(_dbContext, _logger);
        }

        [TearDown]
        public void TearDown() => _dbContext.Dispose();

        private static AppDbContext CreateDbContextWithSeed()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Skills.Add(new Skill
            {
                Id = SeedSkillId,
                Name = SeedSkillName,
                Description = "Testing",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            return context;
        }

        [Test]
        public async Task GetSkills_ReturnsListOfSkills()
        {
            var result = await _controller.GetSkills();

            var skills = result?.Value as List<SkillDto>;


            Assert.Multiple(() =>
            {
                Assert.That(skills, Is.Not.Null);
                Assert.That(skills!.Count(), Is.EqualTo(1));
                Assert.That(skills.First().Name, Is.EqualTo(SeedSkillName));
            });
        }

        [Test]
        public async Task GetSkill_ExistingId_ReturnsSkill()
        {
            var result = await _controller.GetSkill(SeedSkillId);

            var skill = (result as OkObjectResult)?.Value as SkillDto;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.InstanceOf<ObjectResult>());
                Assert.That(result, Is.Not.Null);
                Assert.That(skill.Name, Is.EqualTo(SeedSkillName));
            });
        }

        [Test]
        public async Task GetSkill_NonExistingId_ReturnsNotFound()
        {
            var result = await _controller.GetSkill(999);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task CreateSkill_AddsSkillAndReturnsCreatedAt()
        {
            var newSkill = new  CreateSkillDto("NewSkill", "NewDesc" );

            var result = await _controller.CreateSkill(newSkill);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
                Assert.That(_dbContext.Skills.Count(), Is.EqualTo(2));
            });
        }

        [Test]
        public async Task CreateSkill_DuplicateName_ReturnsNotAllowed()
        {
            var newSkill = new CreateSkillDto("NewSkill", "NewDesc");

            await _controller.CreateSkill(newSkill);
            var result = await _controller.CreateSkill(newSkill); // Attempt duplicate

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(400));
            Assert.That(objectResult.Value, Is.EqualTo(StringConstants.SKILL_EXISTS));

            Assert.That(_dbContext.Skills.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task UpdateSkill_ValidUpdate_ReturnsNoContentAndUpdatesDb()
        {
            var updated = new UpdateSkillDto(1,"UpdatedName", "UpdatedDesc" );

            var result = await _controller.UpdateSkill(SeedSkillId, updated);

            Assert.That(result, Is.TypeOf<NoContentResult>());
            var skill = await _dbContext.Skills.FindAsync(SeedSkillId);
            Assert.That(skill!.Name, Is.EqualTo("UpdatedName"));
        }

        [Test]
        public async Task UpdateSkill_IdMismatch_ReturnsBadRequest()
        {
       

            var updated = new UpdateSkillDto(2, "DoesntMatter", "DoesntMatter");

            var result = await _controller.UpdateSkill(SeedSkillId, updated);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateSkill_NonExisting_ReturnsNotFound()
        {

            var updated = new UpdateSkillDto(999, "Missing", "DoesntMatter");

            var result = await _controller.UpdateSkill(999, updated);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteSkill_Existing_RemovesFromDbAndReturnsNoContent()
        {
            var result = await _controller.DeleteSkill(SeedSkillId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<NoContentResult>());
                Assert.That(_dbContext.Skills.Any(s => s.Id == SeedSkillId), Is.False);
            });
        }

        [Test]
        public async Task DeleteSkill_NonExisting_ReturnsNotFound()
        {
            var result = await _controller.DeleteSkill(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteSkill_ShouldReturnBadRequest_WhenSkillAssignedToEmployee()
        {
            // Arrange
            var skill = new Skill { Id = 2, Name = "C#", Description = "Programming" };
            var employee = new Employee { Id = 3, FirstName = "John", LastName = "Doe",Email="doe@hotmail.com" };
            var employeeSkill = new EmployeeSkill { EmployeeId = 1, SkillId = 2, Employee = employee, Skill = skill };

            _dbContext.Skills.Add(skill);
            _dbContext.Employees.Add(employee);
            _dbContext.EmployeeSkills.Add(employeeSkill);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteSkill(skill.Id);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            
            var badRequest = result as BadRequestObjectResult;
            Assert.That(StringConstants.FAIL_DELETE_SKILLS == badRequest.Value);
        }

        [Test]
        public async Task ExportSkillsToCsv_ReturnsFile_WithCorrectCsvContent()
        {
                     
            // Act
            var result = await  _controller.ExportSkillsToCsv() as FileContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That("text/csv" == result.ContentType);

            var csvText = Encoding.UTF8.GetString(result.FileContents);
            Assert.That(csvText.Contains("Name"));
            Assert.That(csvText.Contains("Description"));
            Assert.That(csvText.Contains("CreatedAt"));

            Assert.That(csvText.Contains("TestSkill"));
        }

        [Test]
        public async Task ExportSkillsToCsv_NoSkills_ReturnsNotFound()
        {
            _dbContext.Skills.RemoveRange(_dbContext.Skills);
            await _dbContext.SaveChangesAsync();

            var result = await _controller.ExportSkillsToCsv() as FileContentResult;

            Assert.That(result, Is.Null);
        }
    }
}

