using EmployeeManagement.Controllers;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EmployeeManagement.Tests
{
    [TestFixture]
    public class SkillsControllerTests
    {
        private const int SeedSkillId = 1;
        private const string SeedSkillName = "TestSkill";

        private AppDbContext _dbContext = null!;
        private SkillsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _dbContext = CreateDbContextWithSeed();
            _controller = new SkillsController(_dbContext);
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

            var skills = (result.Result as OkObjectResult)?.Value as List<SkillDto>;


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

            Assert.Multiple(() =>
            {
                Assert.That(result.Result, Is.Null); // success path
                Assert.That(result.Value, Is.Not.Null);
                Assert.That(result.Value!.Name, Is.EqualTo(SeedSkillName));
            });
        }

        [Test]
        public async Task GetSkill_NonExistingId_ReturnsNotFound()
        {
            var result = await _controller.GetSkill(999);

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateSkill_AddsSkillAndReturnsCreatedAt()
        {
            var newSkill = new  CreateSkillDto("NewSkill", "NewDesc" );

            var result = await _controller.CreateSkill(newSkill);

            Assert.Multiple(() =>
            {
                Assert.That(result.Result, Is.TypeOf<CreatedAtActionResult>());
                Assert.That(_dbContext.Skills.Count(), Is.EqualTo(2));
            });
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
    }
}
