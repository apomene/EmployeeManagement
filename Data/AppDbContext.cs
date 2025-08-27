using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;

namespace EmployeeManagement.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Employee> Employees { get; set; }

    public DbSet<EmployeeSkill> EmployeeSkills { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeSkill>()
            .HasKey(es => new { es.EmployeeId, es.SkillId });

        modelBuilder.Entity<Employee>()
       .HasOne(e => e.Department)
       .WithMany(d => d.Employees)
       .HasForeignKey(e => e.DepartmentId);

        modelBuilder.Entity<EmployeeSkill>()
            .HasOne(es => es.Employee)
            .WithMany(e => e.EmployeeSkills)
            .HasForeignKey(es => es.EmployeeId);

        modelBuilder.Entity<EmployeeSkill>()
            .HasOne(es => es.Skill)
            .WithMany(s => s.EmployeeSkills)
            .HasForeignKey(es => es.SkillId);

        modelBuilder.Entity<Skill>().Property(s => s.CreatedAt)
                    .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Seed initial data
        modelBuilder.Entity<Skill>().HasData(
            new Skill { Id = 1, Name = "C#", Description = "Programming in C#", CreatedAt = new DateTime(2025, 1, 1) },
            new Skill { Id = 2, Name = "SQL", Description = "Database querying and design", CreatedAt = new DateTime(2025, 1, 1) },
            new Skill { Id = 3, Name = "JavaScript", Description = "Frontend and backend scripting", CreatedAt = new DateTime(2025, 1, 1) },
            new Skill { Id = 4, Name = "Project Management", Description = "Agile and Scrum methodologies", CreatedAt = new DateTime(2025, 1, 1) },
            new Skill { Id = 5, Name = "Cloud Computing", Description = "Azure and AWS services", CreatedAt = new DateTime(2025, 1, 1) }
        );

    }
}
