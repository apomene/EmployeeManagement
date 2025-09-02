
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
        public async Task GetAll_ShouldReturnAllLogs()
        {
            

            await _auditLogger.LogChangeAsync("Employee", "emp1", "Create", _dto, "test-user");
            await _auditLogger.LogChangeAsync("Department", "emp2", "Update", _dto2, "test-user");

            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);

            var logs = result.Value as List<AuditLogEntry>;
            Assert.That(logs, Is.Not.Null);
            Assert.That(logs.Count, Is.EqualTo(2));
            Assert.That(logs.Any(l => l.EntityName == "Employee" && l.Action == "Create"), Is.True);
            Assert.That(logs[0].EntityId == "emp2", Is.True);
            Assert.That(logs[0].NewValue.Email == _dto2.Email, Is.True);
            Assert.That(logs[1].EntityId == "emp1", Is.True);
            Assert.That(logs[1].NewValue.Email == _dto.Email, Is.True);

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

    }

}


