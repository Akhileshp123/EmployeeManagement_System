using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Interfaces;
using EmployeeManagementAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementAPI.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IAuthRepository authRepository, IConfiguration configuration)
    {
        _authRepository = authRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _authRepository.GetUserByEmailAsync(loginDto.Email);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        if (await _authRepository.EmailExistsAsync(registerDto.Email))
            throw new ArgumentException("Email is already registered.");

        if (await _authRepository.UsernameExistsAsync(registerDto.Username))
            throw new ArgumentException("Username is already taken.");

        // Validate role
        var allowedRoles = new[] { "Admin", "User" };
        var role = allowedRoles.Contains(registerDto.Role, StringComparer.OrdinalIgnoreCase)
            ? registerDto.Role
            : "User";

        var user = new User
        {
            Username = registerDto.Username.Trim(),
            Email = registerDto.Email.Trim().ToLower(),
            PasswordHash = HashPassword(registerDto.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        var userId = await _authRepository.CreateUserAsync(user);
        user.UserId = userId;

        return GenerateAuthResponse(user);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string HashPassword(string password)
    {
        // PBKDF2 with SHA-256, 100 000 iterations, 32-byte salt + 32-byte hash
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        // Store as Base64(salt):Base64(hash)
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private AuthResponseDto GenerateAuthResponse(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        var issuer = jwtSettings["Issuer"] ?? "EmployeeManagementAPI";
        var audience = jwtSettings["Audience"] ?? "EmployeeManagementAPI";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var mins) ? mins : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiry
        };
    }
}
