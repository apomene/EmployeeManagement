
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.Data;
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
    public class AuditLogsControllerIntegrationTests
    {
        private MongoDbRunner _mongoRunner;
        private IMongoDatabase _db;
        private AuditLogger _auditLogger;
        private AuditLogsController _controller;

        [SetUp]
        public void Setup()
        {
            

            // Start temporary MongoDB
            _mongoRunner = MongoDbRunner.Start();

            var client = new MongoClient(_mongoRunner.ConnectionString);
            var database = client.GetDatabase("HistoryDataTest");

            // use real AuditLogger
            _auditLogger = new AuditLogger(database, NullLogger<AuditLogger>.Instance);
            _controller = new AuditLogsController(_auditLogger);

        }

        [TearDown]
        public void Teardown()
        {
            _mongoRunner.Dispose();
        }


        [Test]
        public async Task GetAll_ShouldReturnAllLogs()
        {
            // Arrange
            var dto = new EmployeeDto(
                 1,
                "John",
                "Doe",
                DateTime.UtcNow,
                "john.doe@yahoo.com",
                new List<string> { "C#" },
                1
            );

            // Arrange
            var dto2 = new EmployeeDto(
                 2,
                "Apo",
                "Mene",
                DateTime.UtcNow,
                "apo.mene@yahoo.com",
                new List<string> { "C#" },
                1
            );

            await _auditLogger.LogChangeAsync("Employee", "emp1", "Create", dto, "test-user");
            await _auditLogger.LogChangeAsync("Department", "dep1", "Update", dto2, "test-user");

            // Act
            var result = await _controller.GetAll() as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);

            var logs = result.Value as List<BsonDocument>;
            Assert.That(logs, Is.Not.Null);
            Assert.That(logs.Count, Is.EqualTo(2));
        }

    }

}


