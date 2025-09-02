
using EmployeeManagement.API.Controllers;
using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;


namespace EmployeeManagement.Tests
{
  
    [TestFixture]
    public class AuditLogsControllerTests
    {
        private MongoDbRunner _mongoRunner;
        private AuditLogger _auditLogger;
        private AuditLogsController _controller;
        private EmployeeDto _dto;
        private EmployeeDto _dto2;
        private readonly NullLogger<AuditLogsController> _logger = NullLogger<AuditLogsController>.Instance;


        [SetUp]
        public void Setup()
        {           
            // Start temporary MongoDB
            _mongoRunner = MongoDbRunner.Start();

            var client = new MongoClient(_mongoRunner.ConnectionString);
            var database = client.GetDatabase("HistoryDataTest");

            // use real AuditLogger
            _auditLogger = new AuditLogger(database, NullLogger<AuditLogger>.Instance);
            _controller = new AuditLogsController(_auditLogger,_logger);

            SetUpData();
        }

        private void SetUpData()
        {
            // Arrange
             _dto = new EmployeeDto(
                 1,
                "John",
                "Doe",
                DateTime.UtcNow,
                "john.doe@yahoo.com",
                new List<string> { "C#" },
                1
            );

             _dto2 = new EmployeeDto(
                 2,
                "Apo",
                "Mene",
                DateTime.UtcNow,
                "apo.mene@yahoo.com",
                new List<string> { "C#" },
                1
            );
        }

        [TearDown]
        public void Teardown()
        {
            _mongoRunner.Dispose();
        }


        [Test]
        [TestCase(1, 1, 1)] 
        [TestCase(1, 2, 2)] 
        [TestCase(2, 1, 1)]
        public async Task GetAll_ShouldReturnPagedLogs(int pageNumber, int pageSize, int expectedCount)
        {

            // Add some initial logs
            _dto = new EmployeeDto(1, "John", "Doe", DateTime.UtcNow, "john@example.com", new List<string> { "C#" }, 1);
            _dto2 = new EmployeeDto(2, "Jane", "Smith", DateTime.UtcNow, "jane@example.com", new List<string> { "Java" }, 2);

            await _auditLogger.LogChangeAsync("Employee", "emp1", "Create", _dto, "test-user");
            await _auditLogger.LogChangeAsync("Department", "emp2", "Update", _dto2, "test-user");

            _controller = new AuditLogsController(_auditLogger,_logger);
            // Act
            var result = await _controller.GetAll(pageNumber, pageSize) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);

            var pagedResult = result.Value as PagedResult<AuditLogEntry>;
            Assert.That(pagedResult, Is.Not.Null);

            var logs = pagedResult!.Items;
            Assert.That(logs.Count, Is.EqualTo(expectedCount));

            // Optional: verify descending order of timestamps
            if (logs.Count > 1)
                Assert.That(logs[0].Timestamp >= logs[1].Timestamp);

            // Check pagination metadata
            Assert.That(pagedResult.PageNumber, Is.EqualTo(pageNumber));
            Assert.That(pagedResult.PageSize, Is.EqualTo(pageSize));
            Assert.That(pagedResult.TotalCount, Is.EqualTo(2)); // total logs in system
        }

        [Test]
        public async Task GetLogsByEmployeeAsync_ShouldReturnOnlyEmployeeLogs()
        {

            await _auditLogger.LogChangeAsync("Employee", "emp1", "Create", _dto, "test-user");
            await _auditLogger.LogChangeAsync("Employee", _dto2.Email, "Create", _dto2, "test-user");
            await _auditLogger.LogChangeAsync("Employee", _dto2.Email, "Update", _dto2, "test-user");

            // Act
            var result = await _auditLogger.GetLogsByEmployeeAsync("apo.mene@yahoo.com");

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(l => l.EntityName == "Employee"), Is.True);
            Assert.That(result.First().Action, Is.EqualTo("Update")); // Should be sorted descending by Timestamp
            Assert.That(result.Last().Action, Is.EqualTo("Create"));
        }

        [Test]
        public async Task GetLogsByEmployeeAsync_ShouldReturnEmptyList_WhenNoLogsFound()
        {
            // Arrange
            var nonExistingEmail = "doesnotexist@example.com";

            // Act
            var result = await _auditLogger.GetLogsByEmployeeAsync(nonExistingEmail);

            // Assert
            Assert.That(result, Is.Not.Null, "Expected an empty list, but got null.");
            Assert.That(result, Is.Empty, "Expected no logs for a non-existing employee.");
        }


    }

}


