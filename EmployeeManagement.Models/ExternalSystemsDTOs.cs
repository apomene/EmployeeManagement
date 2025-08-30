

namespace EmployeeManagement.Models
{
    public class ExternalEmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public int DepartmentId { get; set; }
    }

    public class ImportResult
    {
        public int Imported { get; set; } = 0;
        public int Skipped { get; set; } = 0;
        public int Failed { get; set; } = 0;
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

}
