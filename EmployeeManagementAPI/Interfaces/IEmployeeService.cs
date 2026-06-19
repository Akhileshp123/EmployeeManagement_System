using EmployeeManagementAPI.DTOs;

namespace EmployeeManagementAPI.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync();
    Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int employeeId);
    Task<int> CreateEmployeeAsync(CreateEmployeeDto employee);
    Task<bool> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto employee);
    Task<bool> DeleteEmployeeAsync(int employeeId);
    Task<IEnumerable<EmployeeResponseDto>> SearchEmployeesAsync(string? search, int? departmentId, string? sortBy, bool ascending, int page, int pageSize);
}
