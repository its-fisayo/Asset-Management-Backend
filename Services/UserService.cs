using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Returns a paginated list of users with optional search
    public async Task<PaginatedResponseDto<UserResponseDto>> GetUsers(
        int page,
        int pageSize,
        string? search)
    {
        if (page < 1)
        {
            throw new BadRequestException(
                "Page must be greater than zero.");
        }

        if(pageSize < 1 || pageSize > 100)
        {
            throw new BadRequestException(
                "Page size must be between 1 and 100.");
        }

        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(user =>
            user.FullName.Contains(searchTerm) ||
            user.Email.Contains(searchTerm) ||
            user.StaffId.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(
            (double)totalCount / pageSize);
        var users = await query.
            OrderBy(user => user.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var userDtos = users
            .Select(ToResponseDto)
            .ToList();

        return new PaginatedResponseDto<UserResponseDto>
        {
            Data = userDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    // Returns one user's details.
    public async Task<UserResponseDto> GetUser(Guid id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == id);

        if (user == null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        return ToResponseDto(user);
    }

    // Update basic details without changing role or password.
    public async Task<UserResponseDto> UpdateUser(
        Guid id,
        UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if(user == null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        var fullName = dto.FullName.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        var staffId = dto.StaffId.Trim();

        if(string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(staffId))
        {
            throw new BadRequestException(
                "Full name, email and staff ID are required.");
        }

        var emailExists = await _context.Users.AnyAsync(
            existingUser => existingUser.Email == email &&
            existingUser.Id != id);

        if (emailExists)
        {
            throw new ConflictException(
                "Another account already uses this email.");
        }

        var staffIdExists = await _context.Users.AnyAsync(
            existingUser => existingUser.StaffId == staffId &&
            existingUser.Id != id);

        if (staffIdExists)
        {
            throw new ConflictException(
                "Another account already uses this Staff ID.");
        }

        user.FullName = fullName;
        user.Email = email;
        user.StaffId = staffId;

        await _context.SaveChangesAsync();

        return ToResponseDto(user);
    }

    // Activates or deactivated an account.
    public async Task<UserResponseDto> UpdateUserStatus(
        Guid id,
        bool isActive)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        user.IsActive = isActive;

        await _context.SaveChangesAsync();

        return ToResponseDto(user);
    }

    // Changes a user's role. Only the roles currently supported
    // by this application's authorization setup are accepted.
    public async Task<UserResponseDto> UpdateUserRole(
        Guid id,
        string role)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            throw new NotFoundException(
                "User was not found.");
        }

        var normalisedRole = role.Trim();

        if(!string.Equals(
            normalisedRole,
            "Admin",
            StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                normalisedRole,
                "User",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                "Role must be aither Admin or User.");
        }

        user.Role = normalisedRole.Equals(
            "Admin",
            StringComparison.OrdinalIgnoreCase)
            ? "Admin"
            : "User";

        await _context.SaveChangesAsync();

        return ToResponseDto(user);
    }

    // Maps the database entity to a safe API response.
    private static UserResponseDto ToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            StaffId = user.StaffId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}