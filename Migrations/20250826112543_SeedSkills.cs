using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class SeedSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 26, 11, 25, 43, 33, DateTimeKind.Utc).AddTicks(8191), "Programming in C#", "C#" },
                    { 2, new DateTime(2025, 8, 26, 11, 25, 43, 33, DateTimeKind.Utc).AddTicks(8422), "Database querying and design", "SQL" },
                    { 3, new DateTime(2025, 8, 26, 11, 25, 43, 33, DateTimeKind.Utc).AddTicks(8424), "Frontend and backend scripting", "JavaScript" },
                    { 4, new DateTime(2025, 8, 26, 11, 25, 43, 33, DateTimeKind.Utc).AddTicks(8425), "Agile and Scrum methodologies", "Project Management" },
                    { 5, new DateTime(2025, 8, 26, 11, 25, 43, 33, DateTimeKind.Utc).AddTicks(8426), "Azure and AWS services", "Cloud Computing" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Skills",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
