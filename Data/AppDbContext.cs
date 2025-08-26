using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;

namespace EmployeeManagement.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Skill>().Property(s => s.CreatedAt)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Seed a few skills for quick testing
        modelBuilder.Entity<Skill>().HasData(
            new Skill { Id = 1, Name = "C# Fundamentals", Description = "Core C# syntax, OOP, LINQ", CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new Skill { Id = 2, Name = "ASP.NET Core", Description = "Web APIs, MVC, Razor Pages", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Skill { Id = 3, Name = "SQL", Description = "Joins, indexing, query optimization", CreatedAt = DateTime.UtcNow.AddDays(-3) }
        );
    }
}
