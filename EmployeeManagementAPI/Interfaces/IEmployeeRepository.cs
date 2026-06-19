using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int employeeId);
    Task<int> CreateEmployeeAsync(Employee employee);
    Task<bool> UpdateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(int employeeId);
    Task<bool> ExistsByEmailAsync(string email, int? excludeEmployeeId = null);
    Task<IEnumerable<Employee>> SearchEmployeesAsync(string? search, int? departmentId, string? sortBy, bool ascending, int page, int pageSize);
}
