using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.DTOs;

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public string? Phone { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than zero.")]
    public decimal Salary { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Department is required.")]
    public int DepartmentId { get; set; }
}
