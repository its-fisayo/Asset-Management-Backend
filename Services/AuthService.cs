using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AssetManagementBackend.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> Signup(SignupRequestDto dto)
    {
        var fullName = dto.FullName.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        var staffId = dto.StaffId.Trim();

        if(string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(staffId))
        {
            throw new BadRequestException(
                "Full name, email and Staff ID are required.");
        }

        var emailExists = await _context.Users.AnyAsync(user => user.Email == email);

        if (emailExists)
        {
            throw new ConflictException(
                "An account with this email already exists.");
        }

        var staffIdExists = await _context.Users.AnyAsync(user => user.StaffId == staffId);

        if (staffIdExists)
        {
            throw new ConflictException(
                "An account with this staff ID already exists.");
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            StaffId = staffId,
            Role = "User",
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        //Automatically sign in the newly registered user.
        return GenerateToken(user);
    }

    public async Task<LoginResponseDto?> Login(
        LoginRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == email);

        if(user == null || !user.IsActive)
        {
            return null;
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if(passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if(passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.Password);

            await _context.SaveChangesAsync();
        }

        return GenerateToken(user);
    }

    private LoginResponseDto GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"]
           ?? throw new InvalidOperationException(
               "JWT signing key is not configured.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        var expiryMinutes = _configuration.GetValue<int>(
            "Jwt:ExpiryMinutes");

        if (expiryMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT expiry must be greater than zero.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("StaffId", user.StaffId)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
            );

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new LoginResponseDto
        {
            AccessToken = tokenString,
            TokenType = "Bearer",
            ExpiresAt = expiresAt,
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            StaffId = user.StaffId,
            Role = user.Role
        };
    }
}