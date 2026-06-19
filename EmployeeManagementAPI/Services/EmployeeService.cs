using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Interfaces;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public EmployeeService(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetEmployeesAsync();
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        return employee is null ? null : MapToDto(employee);
    }

    public async Task<int> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        ValidateEmployee(dto.FirstName, dto.LastName, dto.Email, dto.Salary);

        if (await _employeeRepository.ExistsByEmailAsync(dto.Email))
            throw new ArgumentException("Email must be unique.");

        if (!await _departmentRepository.ExistsByIdAsync(dto.DepartmentId))
            throw new ArgumentException("Department does not exist.");

        var entity = new Employee
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            Salary = dto.Salary,
            HireDate = DateTime.UtcNow,
            DepartmentId = dto.DepartmentId
        };

        return await _employeeRepository.CreateEmployeeAsync(entity);
    }

    public async Task<bool> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto dto)
    {
        ValidateEmployee(dto.FirstName, dto.LastName, dto.Email, dto.Salary);

        var existing = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        if (existing is null) return false;

        if (await _employeeRepository.ExistsByEmailAsync(dto.Email, employeeId))
            throw new ArgumentException("Email must be unique.");

        if (!await _departmentRepository.ExistsByIdAsync(dto.DepartmentId))
            throw new ArgumentException("Department does not exist.");

        existing.FirstName = dto.FirstName.Trim();
        existing.LastName = dto.LastName.Trim();
        existing.Email = dto.Email.Trim().ToLower();
        existing.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        existing.Salary = dto.Salary;
        existing.DepartmentId = dto.DepartmentId;

        return await _employeeRepository.UpdateEmployeeAsync(existing);
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId)
    {
        var existing = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        if (existing is null) return false;

        return await _employeeRepository.DeleteEmployeeAsync(employeeId);
    }

    public async Task<IEnumerable<EmployeeResponseDto>> SearchEmployeesAsync(
        string? name,
        string? email,
        int? departmentId,
        decimal? minSalary,
        decimal? maxSalary,
        string? sortBy,
        bool ascending,
        int page,
        int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        if (minSalary.HasValue && minSalary.Value < 0)
            throw new ArgumentException("Minimum salary cannot be negative.");

        if (maxSalary.HasValue && maxSalary.Value < 0)
            throw new ArgumentException("Maximum salary cannot be negative.");

        if (minSalary.HasValue && maxSalary.HasValue && minSalary.Value > maxSalary.Value)
            throw new ArgumentException("Minimum salary cannot be greater than maximum salary.");

        if (departmentId.HasValue && !await _departmentRepository.ExistsByIdAsync(departmentId.Value))
            throw new ArgumentException("Department does not exist.");

        var normalizedSort = NormalizeSortColumn(sortBy);
        var employees = await _employeeRepository.SearchEmployeesAsync(
            name,
            email,
            departmentId,
            minSalary,
            maxSalary,
            normalizedSort,
            ascending,
            page,
            pageSize);

        return employees.Select(MapToDto);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static EmployeeResponseDto MapToDto(Employee e) => new EmployeeResponseDto
    {
        EmployeeId = e.EmployeeId,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        Salary = e.Salary,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.DepartmentName ?? string.Empty
    };

    private static void ValidateEmployee(string firstName, string lastName, string email, decimal salary)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        if (salary <= 0)
            throw new ArgumentException("Salary must be greater than zero.");
    }

    private static string? NormalizeSortColumn(string? sortBy) => sortBy?.Trim().ToLowerInvariant() switch
    {
        "name" => "FirstName",
        "firstname" => "FirstName",
        "lastname" => "LastName",
        "last_name" => "LastName",
        "email" => "Email",
        "salary" => "Salary",
        "hiredate" => "HireDate",
        "hire_date" => "HireDate",
        "department" => "DepartmentName",
        "departmentname" => "DepartmentName",
        _ => sortBy
    };
}
