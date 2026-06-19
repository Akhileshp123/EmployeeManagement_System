using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? sort,
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var employees = string.IsNullOrWhiteSpace(sort)
            ? await _employeeService.GetAllEmployeesAsync()
            : await _employeeService.SearchEmployeesAsync(null, null, null, null, null, sort, ascending, page, pageSize);

        return Ok(employees);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchEmployees(
        [FromQuery] string? name,
        [FromQuery] string? email,
        [FromQuery] string? sort,
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employees = await _employeeService.SearchEmployeesAsync(name, email, null, null, null, sort, ascending, page, pageSize);
            return Ok(employees);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpGet("department/{id:int}")]
    public async Task<IActionResult> GetEmployeesByDepartment(
        int id,
        [FromQuery] string? sort,
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employees = await _employeeService.SearchEmployeesAsync(null, null, id, null, null, sort, ascending, page, pageSize);
            return Ok(employees);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpGet("filter")]
    public async Task<IActionResult> FilterEmployees(
        [FromQuery] int? departmentId,
        [FromQuery] decimal? minSalary,
        [FromQuery] decimal? maxSalary,
        [FromQuery] string? sort,
        [FromQuery] bool ascending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var employees = await _employeeService.SearchEmployeesAsync(null, null, departmentId, minSalary, maxSalary, sort, ascending, page, pageSize);
            return Ok(employees);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto employee)
    {
        try
        {
            var id = await _employeeService.CreateEmployeeAsync(employee);
            var created = await _employeeService.GetEmployeeByIdAsync(id);
            return CreatedAtAction(nameof(GetEmployeeById), new { id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto employee)
    {
        try
        {
            var updated = await _employeeService.UpdateEmployeeAsync(id, employee);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var deleted = await _employeeService.DeleteEmployeeAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
