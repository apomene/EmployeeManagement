

namespace EmployeeManagement.Models
{
    public class StringConstants
    {
        // Skills
        public const string SKILLS = "skills";
        public const string SKILL_EXISTS = "Skill with the same name already exists";
        public const string ID_MISMATCH = "ID mismatch";
        public const string ERROR_CREATE_SKILL = "Error creating skill";
        public const string ERROR_UPDATE_SKILL = "Error updating skill";
        public const string ERROR_DELETE_SKILL = "Error deleting skill";
        public const string NO_SKILL = "No Skill with this Id";
        public const string SKILL_IN_USE = "Employee already has the skill";
        public const string UNEXPTECTED_ERROR = "An unexpected error occurred while adding the skill.";
        public const string FAIL_DELETE_SKILLS = "Cannot delete skill because it is assigned to one or more employees.";
        public const string NO_SKILLS = "No Skills Found";
        public const string FAIL_EXPORT_SKILLS = "Failed to export skills.";
        public const string CANNNOT_CREATE_SKILL = "Could not create new skill.";

        // Employees
        public const string EMPLOYEES = "employees";
        public const string ERROR_CREATE_EMPLOYEE = "Error creating employee";
        public const string ERROR_UPDATE_EMPLOYEE = "Error updating employee";
        public const string ERROR_DELETE_EMPLOYEE = "Error deleting employee";
        public const string NO_EMPLOYEE_ID = "No employee IDs provided.";
        public const string NO_MATCHING_EMPLOYEES = "No matching employees found.";
        public const string INVALID_DEPARTMENT = "Invalid DepartmentId";
        public const string INVALID_EMPLOYEE = "Invalid employee Id";
        public const string DEPARTMENTS = "departments";
        public const string FAIL_DELETE_EMPLOYEES = "Failed to delete employees.";
        public const string OK_DELETE_EMPLOYEES = "Employees deleted successfully."; 
        public const string NO_EMPLOYEES = "No Employees Found";

        // Log Messages
        public const string ERROR_500 = "An error occurred while executing API action: {error}";

        //Employees Controller
        public const string LOG_EMPLOYEE_CREATED = "Created employee {FirstName} {LastName}";
        public const string LOG_EMPLOYEE_UPDATED = "Updated employee {EmployeeId}";
        public const string LOG_EMPLOYEE_DELETED = "Deleted employee {EmployeeId}";
        public const string LOG_EMPLOYEES_DELETED = "Deleted multiple employees";
        public const string LOG_SKILL_ADDED = "Added skill {SkillId} to employee {EmployeeId}";
        public const string LOG_SKILL_REMOVED = "Removed skill {SkillId} from employee {EmployeeId}";
        public const string LOG_DEPARTMENTS_FETCHED = "Fetched departments";
        public const string LOG_EMPLOYEE_SKILLS_FETCHED = "Fetched skills for employee {EmployeeId}";
        public const string LOG_EMPLOYEE_FETCHED = "Fetched employee with ID {EmployeeId}";
        public const string LOG_EMPLOYEES_FETCHED = "Fetched employees";

        //Skills Controller
        public const string LOG_SKILLS_FETCHED = "Fetched all skills";
        public const string LOG_SKILL_FETCHED = "Fetched skill {SkillId}";
        public const string LOG_SKILL_CREATED = "Created skill {SkillName} with ID {SkillId}";
        public const string LOG_SKILL_UPDATED = "Updated skill {SkillId}";
        public const string LOG_SKILL_DELETED = "Deleted skill {SkillId}";
        public const string LOG_SKILLS_EXPORTED = "Exported skills to CSV";
       

        //Audit Logs
        public const string AUDIT_CREATE = "Create";
        public const string AUDIT_UPDATE =  "Update";
        public const string AUDIT_DELETE =  "Delete";
        public const string AUDIT_SKILL_ADD = "ADDED_SKILL";
        public const string AUDIT_SKILL_REMOVE = "REMOVED_SKILL";
        public const string NO_LOGS_FOUND = "No audit logs found for employee with e-mail: ";
        public const string AUDIT_LOGS = "auditlogs";
        public const string LOGS_ERROR = "Error retrieving audit logs for employee with e-mail: {email}"; 
        public const string AUDIT_USER = "System";


    }
}
