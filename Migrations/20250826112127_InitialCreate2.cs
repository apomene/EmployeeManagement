using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 19, 11, 18, 39, 438, DateTimeKind.Utc).AddTicks(548), "Core C# syntax, OOP, LINQ", "C# Fundamentals" },
                    { 2, new DateTime(2025, 8, 21, 11, 18, 39, 438, DateTimeKind.Utc).AddTicks(893), "Web APIs, MVC, Razor Pages", "ASP.NET Core" },
                    { 3, new DateTime(2025, 8, 23, 11, 18, 39, 438, DateTimeKind.Utc).AddTicks(899), "Joins, indexing, query optimization", "SQL" }
                });
        }
    }
}
