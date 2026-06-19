using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Interfaces;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IEnumerable<DepartmentResponseDto>> GetAllDepartmentsAsync()
    {
        var departments = await _departmentRepository.GetDepartmentsAsync();
        var result = new List<DepartmentResponseDto>();

        foreach (var dept in departments)
        {
            var count = await _departmentRepository.GetEmployeeCountAsync(dept.DepartmentId);
            result.Add(new DepartmentResponseDto
            {
                DepartmentId = dept.DepartmentId,
                DepartmentName = dept.DepartmentName,
                Description = dept.Description,
                EmployeeCount = count
            });
        }

        return result;
    }

    public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int departmentId)
    {
        var dept = await _departmentRepository.GetDepartmentByIdAsync(departmentId);
        if (dept is null) return null;

        var count = await _departmentRepository.GetEmployeeCountAsync(departmentId);
        return new DepartmentResponseDto
        {
            DepartmentId = dept.DepartmentId,
            DepartmentName = dept.DepartmentName,
            Description = dept.Description,
            EmployeeCount = count
        };
    }

    public async Task<int> CreateDepartmentAsync(CreateDepartmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DepartmentName))
            throw new ArgumentException("Department name is required.");

        if (await _departmentRepository.ExistsByNameAsync(dto.DepartmentName))
            throw new ArgumentException("Department name must be unique.");

        var department = new Department
        {
            DepartmentName = dto.DepartmentName.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
        };

        return await _departmentRepository.CreateDepartmentAsync(department);
    }

    public async Task<bool> UpdateDepartmentAsync(int departmentId, UpdateDepartmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DepartmentName))
            throw new ArgumentException("Department name is required.");

        var existing = await _departmentRepository.GetDepartmentByIdAsync(departmentId);
        if (existing is null) return false;

        if (await _departmentRepository.ExistsByNameAsync(dto.DepartmentName, departmentId))
            throw new ArgumentException("Department name must be unique.");

        existing.DepartmentName = dto.DepartmentName.Trim();
        existing.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        return await _departmentRepository.UpdateDepartmentAsync(existing);
    }

    public async Task<bool> DeleteDepartmentAsync(int departmentId)
    {
        var exists = await _departmentRepository.ExistsByIdAsync(departmentId);
        if (!exists) return false;

        var employeeCount = await _departmentRepository.GetEmployeeCountAsync(departmentId);
        if (employeeCount > 0)
            throw new InvalidOperationException($"Cannot delete department. It has {employeeCount} employee(s) assigned.");

        return await _departmentRepository.DeleteDepartmentAsync(departmentId);
    }
}
