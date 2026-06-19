using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Interfaces;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int departmentId);
    Task<int> CreateDepartmentAsync(Department department);
    Task<bool> UpdateDepartmentAsync(Department department);
    Task<bool> DeleteDepartmentAsync(int departmentId);
    Task<bool> ExistsByNameAsync(string departmentName, int? excludeDepartmentId = null);
    Task<bool> ExistsByIdAsync(int departmentId);
    Task<int> GetEmployeeCountAsync(int departmentId);
}
