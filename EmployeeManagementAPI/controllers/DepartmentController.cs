using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments([FromQuery] string? name)
    {
        var departments = string.IsNullOrWhiteSpace(name)
            ? await _departmentService.GetAllDepartmentsAsync()
            : await _departmentService.SearchDepartmentsAsync(name);

        return Ok(departments);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchDepartments([FromQuery] string name)
    {
        var departments = await _departmentService.SearchDepartmentsAsync(name);
        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDepartmentById(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        return department is null ? NotFound() : Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto department)
    {
        try
        {
            var id = await _departmentService.CreateDepartmentAsync(department);
            var created = await _departmentService.GetDepartmentByIdAsync(id);
            return CreatedAtAction(nameof(GetDepartmentById), new { id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto department)
    {
        try
        {
            var updated = await _departmentService.UpdateDepartmentAsync(id, department);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        try
        {
            var deleted = await _departmentService.DeleteDepartmentAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}
