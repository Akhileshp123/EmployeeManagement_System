using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Interfaces;
using EmployeeManagementAPI.Models;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Text;

namespace EmployeeManagementAPI.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public EmployeeRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static Employee MapEmployee(DbDataReader reader) => new Employee
    {
        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        LastName = reader.GetString(reader.GetOrdinal("LastName")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
        HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
        DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
        DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName"))
    };

    public async Task<IEnumerable<Employee>> GetEmployeesAsync()
    {
        const string sql = @"SELECT e.EmployeeId, e.FirstName, e.LastName, e.Email, e.Phone,
                                    e.Salary, e.HireDate, e.DepartmentId, d.DepartmentName
                             FROM Employee e
                             JOIN Department d ON e.DepartmentId = d.DepartmentId
                             ORDER BY e.EmployeeId";

        var employees = new List<Employee>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            employees.Add(MapEmployee(reader));
        }

        return employees;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
    {
        const string sql = @"SELECT e.EmployeeId, e.FirstName, e.LastName, e.Email, e.Phone,
                                    e.Salary, e.HireDate, e.DepartmentId, d.DepartmentName
                             FROM Employee e
                             JOIN Department d ON e.DepartmentId = d.DepartmentId
                             WHERE e.EmployeeId = @EmployeeId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapEmployee(reader);
    }

    public async Task<int> CreateEmployeeAsync(Employee employee)
    {
        const string sql = @"INSERT INTO Employee
                             (FirstName, LastName, Email, Phone, Salary, HireDate, DepartmentId)
                             VALUES
                             (@FirstName, @LastName, @Email, @Phone, @Salary, @HireDate, @DepartmentId)";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@LastName", employee.LastName);
        command.Parameters.AddWithValue("@Email", employee.Email);
        command.Parameters.AddWithValue("@Phone", employee.Phone ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Salary", employee.Salary);
        command.Parameters.AddWithValue("@HireDate", employee.HireDate.Date);
        command.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId);

        await command.ExecuteNonQueryAsync();
        return (int)command.LastInsertedId;
    }

    public async Task<bool> UpdateEmployeeAsync(Employee employee)
    {
        const string sql = @"UPDATE Employee SET
                             FirstName = @FirstName,
                             LastName = @LastName,
                             Email = @Email,
                             Phone = @Phone,
                             Salary = @Salary,
                             DepartmentId = @DepartmentId
                             WHERE EmployeeId = @EmployeeId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        command.Parameters.AddWithValue("@LastName", employee.LastName);
        command.Parameters.AddWithValue("@Email", employee.Email);
        command.Parameters.AddWithValue("@Phone", employee.Phone ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Salary", employee.Salary);
        command.Parameters.AddWithValue("@DepartmentId", employee.DepartmentId);
        command.Parameters.AddWithValue("@EmployeeId", employee.EmployeeId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId)
    {
        const string sql = "DELETE FROM Employee WHERE EmployeeId = @EmployeeId";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@EmployeeId", employeeId);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeEmployeeId = null)
    {
        const string sql = "SELECT COUNT(1) FROM Employee WHERE Email = @Email" +
                           " AND (@ExcludeEmployeeId IS NULL OR EmployeeId <> @ExcludeEmployeeId)";

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@ExcludeEmployeeId", excludeEmployeeId.HasValue ? excludeEmployeeId.Value : (object)DBNull.Value);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<IEnumerable<Employee>> SearchEmployeesAsync(
        string? search, int? departmentId, string? sortBy, bool ascending, int page, int pageSize)
    {
        var sqlBuilder = new StringBuilder(@"
            SELECT e.EmployeeId, e.FirstName, e.LastName, e.Email, e.Phone,
                   e.Salary, e.HireDate, e.DepartmentId, d.DepartmentName
            FROM Employee e
            JOIN Department d ON e.DepartmentId = d.DepartmentId
            WHERE 1=1");

        if (!string.IsNullOrWhiteSpace(search))
        {
            sqlBuilder.Append(" AND (e.FirstName LIKE @Search OR e.LastName LIKE @Search OR e.Email LIKE @Search)");
        }

        if (departmentId.HasValue)
        {
            sqlBuilder.Append(" AND e.DepartmentId = @DepartmentId");
        }

        // Safe whitelist for sort columns
        var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FirstName", "LastName", "Email", "Salary", "HireDate", "DepartmentName"
        };

        var orderColumn = allowedSortColumns.Contains(sortBy ?? "") ? sortBy! : "e.EmployeeId";
        if (orderColumn == "DepartmentName")
            orderColumn = "d.DepartmentName";
        else if (!orderColumn.StartsWith("e.") && !orderColumn.StartsWith("d."))
            orderColumn = "e." + orderColumn;

        sqlBuilder.Append($" ORDER BY {orderColumn} {(ascending ? "ASC" : "DESC")}");
        sqlBuilder.Append(" LIMIT @PageSize OFFSET @Offset");

        var employees = new List<Employee>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sqlBuilder.ToString(), connection);

        if (!string.IsNullOrWhiteSpace(search))
            command.Parameters.AddWithValue("@Search", $"%{search}%");

        if (departmentId.HasValue)
            command.Parameters.AddWithValue("@DepartmentId", departmentId.Value);

        command.Parameters.AddWithValue("@PageSize", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            employees.Add(MapEmployee(reader));
        }

        return employees;
    }
}
