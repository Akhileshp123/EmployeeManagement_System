using EmployeeManagementAPI.DTOs;

namespace EmployeeManagementAPI.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync();
    Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int departmentId);
    Task<int> CreateDepartmentAsync(CreateDepartmentDto dto);
    Task<bool> UpdateDepartmentAsync(int departmentId, UpdateDepartmentDto dto);
    Task<bool> DeleteDepartmentAsync(int departmentId);
}
