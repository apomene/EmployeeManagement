namespace EmployeeManagement.Services
{
    using EmployeeManagement.Data;
    using EmployeeManagement.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class FilterCollection(AppDbContext db)
    {
        public string? OrderBy { get; set; }
        public string Direction { get; set; } = "asc";
        public string? SearchTerm { get; set; }
        public string? SearchField { get; set; }   // which field to search on (optional)

        public IQueryable<T> ApplyOrdering<T>(IQueryable<T> query)
        {
            if (string.IsNullOrEmpty(OrderBy))
                return query;

            var property = typeof(T).GetProperty(OrderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                throw new ArgumentException($"Property '{OrderBy}' does not exist on type '{typeof(T).Name}'");

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            string methodName = Direction.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

            var result = typeof(Queryable).GetMethods()
                .Single(m => m.Name == methodName
                            && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.PropertyType)
                .Invoke(null, new object[] { query, orderByExpression });

            return (IQueryable<T>)result!;
        }

        public IQueryable<T> ApplyFiltering<T>(IQueryable<T> query)
        {
            if (string.IsNullOrEmpty(SearchTerm) || string.IsNullOrEmpty(SearchField))
                return query;

            var property = typeof(T).GetProperty(SearchField,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                throw new ArgumentException($"Property '{SearchField}' does not exist on type '{typeof(T).Name}'");

            if (property.PropertyType != typeof(string))
                throw new ArgumentException($"Filtering only supported on string properties for now (field: {SearchField})");

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);

            var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null));
            var contains = Expression.Call(propertyAccess,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(SearchTerm, typeof(string)));

            var predicate = Expression.AndAlso(notNull, contains);
            var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);

            return query.Where(lambda);
        }

        public IQueryable<T> ApplyAll<T>(IQueryable<T> query)
        {
            query = ApplyFiltering(query);
            query = ApplyOrdering(query);
            return query;
        }

        public async Task<List<Employee>> GetEmployeesBySkillsAsync(List<int> skillIds)
        {
            var query = db.Employees.AsQueryable();


            if (skillIds != null && skillIds.Any())
            {
                query = query.Where(e => e.EmployeeSkills.Any(es => skillIds.Contains(es.SkillId)));
            }

            var result= await query
                .Include(e => e.EmployeeSkills)
                .ThenInclude(es => es.Skill)
                .Include(e => e.Department)
                .ToListAsync();
            return result;
        }


    }
}
