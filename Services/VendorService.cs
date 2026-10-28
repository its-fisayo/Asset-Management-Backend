using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Mappings;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class VendorService
{
    private readonly ApplicationDbContext _context;

    public VendorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task <PaginatedResponseDto<VendorResponseDto>> GetVendors(
        int page = 1,
        int pageSize = 16,
        string? search = null)
    {
        if(page < 1)
        {
            throw new BadRequestException(
                "Page must be greater than zero.");
        }
        if(pageSize < 1 || pageSize > 100)
        {
            throw new BadRequestException(
                "Page size must be between 1 and 100");
        }

        var query = _context.Vendors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(vendor =>
            vendor.Name.Contains(searchTerm) ||
            vendor.PrimaryContact.Contains(searchTerm) ||
            vendor.Email.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var vendors = await query
            .OrderBy(vendor => vendor.Name)
            .Skip((page -1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var vendorDtos = vendors
            .Select(vendor => vendor.ToResponseDto())
            .ToList();

        return new PaginatedResponseDto<VendorResponseDto>
        {
            Data = vendorDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<VendorResponseDto> GetVendor(Guid id)
    {
        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(vendor => vendor.Id == id);

        if(vendor == null)
        {
            throw new NotFoundException("Vendor was not found.");
        }

        return vendor.ToResponseDto();
    }

    public async Task<VendorResponseDto> CreateVendor(CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            Name = dto.Name,
            PrimaryContact = dto.PrimaryContact,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email
        };

        _context.Vendors.Add(vendor);

        await _context.SaveChangesAsync();

        return vendor.ToResponseDto();
    }

    public async Task<VendorResponseDto> UpdateVendor(
        Guid id,
        UpdateVendorDto dto)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if(vendor == null)
        {
            throw new NotFoundException("Vendor was not found.");
        }

        vendor.Name = dto.Name;
        vendor.PrimaryContact = dto.PrimaryContact;
        vendor.PhoneNumber = dto.PhoneNumber;
        vendor.Email = dto.Email;
        vendor.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return vendor.ToResponseDto();
    }

    public async Task DeleteVendor(Guid id)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if(vendor == null)
        {
            throw new NotFoundException("Vendor was not found.");
        }

        _context.Vendors.Remove(vendor);

        await _context.SaveChangesAsync();
    }
}