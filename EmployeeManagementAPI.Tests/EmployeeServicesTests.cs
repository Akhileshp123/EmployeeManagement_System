using EmployeeManagementAPI.Interfaces;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Services;
using Moq;
using NUnit.Framework;
using EmployeeManagementAPI.DTOs;

namespace EmployeeManagementAPI.Tests;

public class EmployeeServiceTests
{
    private Mock<IEmployeeRepository> _employeeRepositoryMock;
    private Mock<IDepartmentRepository> _departmentRepositoryMock;
    private EmployeeService _employeeService;

    [SetUp]
    public void Setup()
    {
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _departmentRepositoryMock = new Mock<IDepartmentRepository>();

        _employeeService = new EmployeeService(
            _employeeRepositoryMock.Object,
            _departmentRepositoryMock.Object);
    }

    [Test]
    public async Task GetEmployeeByIdAsync_ReturnsNull_WhenEmployeeDoesNotExist()
    {
        // Arrange
        _employeeRepositoryMock
            .Setup(x => x.GetEmployeeByIdAsync(999))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await _employeeService.GetEmployeeByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }
    [Test]
    public async Task GetEmployeeByIdAsync_ReturnsEmployee_WhenEmployeeExists()
    {
        // Arrange
        var employee = new Employee
        {
            EmployeeId = 1,
            FirstName = "Akhilesh",
            LastName = "P",
            Email = "akhil@gmail.com",
            Salary = 50000,
            DepartmentId = 1,
            DepartmentName = "IT"
        };

        _employeeRepositoryMock
            .Setup(x => x.GetEmployeeByIdAsync(1))
            .ReturnsAsync(employee);

        // Act
        var result = await _employeeService.GetEmployeeByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EmployeeId, Is.EqualTo(1));
        Assert.That(result.FirstName, Is.EqualTo("Akhilesh"));
        Assert.That(result.Email, Is.EqualTo("akhil@gmail.com"));
    }
    [Test]
public void CreateEmployeeAsync_ThrowsException_WhenEmailAlreadyExists()
{
    // Arrange
    var dto = new CreateEmployeeDto
    {
        FirstName = "Akhilesh",
        LastName = "P",
        Email = "akhil@gmail.com",
        Phone = "9876543210",
        Salary = 50000,
        DepartmentId = 1
    };

    _employeeRepositoryMock
        .Setup(x => x.ExistsByEmailAsync(dto.Email, null))
        .ReturnsAsync(true);

    // Act & Assert
    Assert.ThrowsAsync<ArgumentException>(
        async () => await _employeeService.CreateEmployeeAsync(dto));
}
}