using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToEmployeeEmail_Safe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create a new table with the same columns + unique constraint on Email
            migrationBuilder.CreateTable(
                name: "Employees_temp",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                              .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    HireDate = table.Column<DateTime>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false)
                    // add other columns
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees_temp", x => x.Id);
                });

            // 2. Add unique index on Email
            migrationBuilder.CreateIndex(
                name: "IX_Employees_temp_Email",
                table: "Employees_temp",
                column: "Email",
                unique: true);

            // 3. Copy data from old table
            migrationBuilder.Sql("INSERT INTO Employees_temp (Id, FirstName, LastName, Email, HireDate, DepartmentId) SELECT Id, FirstName, LastName, Email, HireDate, DepartmentId FROM Employees;");

            // 4. Drop old table
            migrationBuilder.DropTable("Employees");

            // 5. Rename new table
            migrationBuilder.RenameTable("Employees_temp", newName: "Employees");
        }
    
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
