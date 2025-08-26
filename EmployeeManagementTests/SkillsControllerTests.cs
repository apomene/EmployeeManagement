using EmployeeManagement.Controllers;
using EmployeeManagement.Data;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementTests
{
    public class SkillsControllerTests
    {
        private AppDbContext _dbContext = null!;
        private SkillsController _controller = null!;
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetSkills_ReturnsListOfSkills()
        {
            var result = await _controller.GetSkills();

            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(1));
            Assert.That(result.Value.First().Name, Is.EqualTo("TestSkill"));
        }

        [Test]
        public async Task GetSkill_ExistingId_ReturnsSkill()
        {
            var result = await _controller.GetSkill(1);

            Assert.That(result.Result, Is.Null); // ActionResult<Skill> has Value, not IActionResult
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Name, Is.EqualTo("TestSkill"));
        }

        [Test]
        public async Task GetSkill_NonExistingId_ReturnsNotFound()
        {
            var result = await _controller.GetSkill(999);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateSkill_AddsSkillAndReturnsCreatedAt()
        {
            var newSkill = new Skill { Name = "NewSkill", Description = "NewDesc" };

            var result = await _controller.CreateSkill(newSkill);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            Assert.That(_dbContext.Skills.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task UpdateSkill_ValidUpdate_ReturnsNoContent()
        {
            var updated = new Skill { Id = 1, Name = "UpdatedName", Description = "UpdatedDesc" };

            var result = await _controller.UpdateSkill(1, updated);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var skill = await _dbContext.Skills.FindAsync(1);
            Assert.That(skill!.Name, Is.EqualTo("UpdatedName"));
        }

        [Test]
        public async Task UpdateSkill_IdMismatch_ReturnsBadRequest()
        {
            var updated = new Skill { Id = 2, Name = "DoesntMatter" };

            var result = await _controller.UpdateSkill(1, updated);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateSkill_NonExisting_ReturnsNotFound()
        {
            var updated = new Skill { Id = 999, Name = "Missing" };

            var result = await _controller.UpdateSkill(999, updated);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteSkill_Existing_ReturnsNoContent()
        {
            var result = await _controller.DeleteSkill(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            Assert.That(_dbContext.Skills.Any(s => s.Id == 1), Is.False);
        }

        [Test]
        public async Task DeleteSkill_NonExisting_ReturnsNotFound()
        {
            var result = await _controller.DeleteSkill(999);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}

