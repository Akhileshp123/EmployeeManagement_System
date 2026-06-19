using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Interfaces;
using EmployeeManagementAPI.Models;
using MySql.Data.MySqlClient;

namespace EmployeeManagementAPI.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DepartmentRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Department>> GetDepartmentsAsync()
    {
        const string sql = "SELECT DepartmentId, DepartmentName, Description FROM Department";
        var departments = new List<Department>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            departments.Add(new Department
            {
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
            });
        }

        return departments;
    }

    public async Task<IEnumerable<Department>> SearchDepartmentsAsync(string name)
    {
        const string sql = @"SELECT DepartmentId, DepartmentName, Description
                             FROM Department
                             WHERE DepartmentName LIKE @Name
                             ORDER BY DepartmentName";
        var departments = new List<Department>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", $"%{name}%");

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            departments.Add(new Department
            {
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
            });
        }

        return departments;
    }

    public async Task<Department?> GetDepartmentByIdAsync(int departmentId)
    {
        const string sql = "SELECT DepartmentId, DepartmentName, Description FROM Department WHERE DepartmentId = @DepartmentId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DepartmentId", departmentId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Department
        {
            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
            DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
        };
    }

    public async Task<int> CreateDepartmentAsync(Department department)
    {
        const string sql = "INSERT INTO Department (DepartmentName, Description) VALUES (@DepartmentName, @Description);";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DepartmentName", department.DepartmentName);
        command.Parameters.AddWithValue("@Description", department.Description ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
        return (int)command.LastInsertedId;
    }

    public async Task<bool> UpdateDepartmentAsync(Department department)
    {
        const string sql = "UPDATE Department SET DepartmentName = @DepartmentName, Description = @Description WHERE DepartmentId = @DepartmentId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DepartmentName", department.DepartmentName);
        command.Parameters.AddWithValue("@Description", department.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DepartmentId", department.DepartmentId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteDepartmentAsync(int departmentId)
    {
        const string sql = "DELETE FROM Department WHERE DepartmentId = @DepartmentId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DepartmentId", departmentId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsByNameAsync(string departmentName, int? excludeDepartmentId = null)
    {
        const string sql = "SELECT COUNT(1) FROM Department WHERE DepartmentName = @DepartmentName" +
                           " AND (@ExcludeDepartmentId IS NULL OR DepartmentId <> @ExcludeDepartmentId)";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DepartmentName", departmentName);
        command.Parameters.AddWithValue("@ExcludeDepartmentId", excludeDepartmentId.HasValue ? excludeDepartmentId.Value : (object)DBNull.Value);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<bool> ExistsByIdAsync(int departmentId)
    {
        const string sql = "SELECT COUNT(1) FROM Department WHERE DepartmentId = @DepartmentId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DepartmentId", departmentId);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<int> GetEmployeeCountAsync(int departmentId)
    {
        const string sql = "SELECT COUNT(1) FROM Employee WHERE DepartmentId = @DepartmentId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DepartmentId", departmentId);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
