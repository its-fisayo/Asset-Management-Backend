using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Mappings;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class ProductImageService
{
    // Provides access to the database and the application's file system.
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    // Provides access to the database and the application's file system.
    public ProductImageService(
        ApplicationDbContext context,
        IWebHostEnvironment environment
        )
    {
        _context = context;
        _environment = environment;
    }

    public async Task<ProductImageResponseDto?> UploadImage(
        Guid productId,
        IFormFile file
        )
    {
        // Find the product the uploaded image should belong to.
        var product = await _context.Products
            .FindAsync( productId );

        // Stop the upload if the product does not exist.
        if ( product == null )
        {
            return null;
        }

        // Reject an empty file.
        if ( file.Length == 0 )
        {
            throw new BadRequestException("The uploaded file is empty.");
        }

        // Set the maximum permitted image size to 5 MB.
        const long maxFileSize = 5 * 1024 * 1024;

        // Reject files that exceed the size limit.
        if ( file.Length > maxFileSize )
        {
            throw new BadRequestException("The file size cannot exceed 5MB.");
        }

        // Define the image content types accepted by the application.
        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        // Reject files whose declared content type is not supported.
        if (!allowedTypes.Contains(file.ContentType))
        {
            throw new BadRequestException("Only JPEG, PNG and WebP images are allowed.");
        }

        // Get the web root directory, normally the wwwroot folder.
        // Use the application's content root as a fallback if necessary.
        var webRootPath = _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot"
            );

        // Define the folder where uploaded images will be stored.
        var uploadsFolder = Path.Combine(
            webRootPath,
            "uploads"
         );

        // Create the uploads folder if it does not already exist.
        Directory.CreateDirectory(uploadsFolder);

        // Extract the file extension from the original filename.
        var extension = Path.GetExtension(file.FileName);

        // Generate a unique filename to reduce the risk of overwriting files.
        var generatedFileName = $"{Guid.NewGuid()}{extension}";

        // Combine the uploads directory and generated filename
        // to obtain the full physical path of the file.
        var filePath = Path.Combine(
            uploadsFolder,
            generatedFileName
        );

        // Create the destination file and open a stream for writing.
        await using var stream = new FileStream(
            filePath,
            FileMode.Create
        );

        // Copy the uploaded file's contents into the destination file.
        await file.CopyToAsync(stream);

        // Create a database record containing the image's details.
        var image = new ProductImage
        {
            ProductId = productId,
            FileName = file.FileName,
            FilePath = $"/uploads/{generatedFileName}",
            ContentType = file.ContentType,
            FileSize = file.Length
        };

        // Add the new image record to the database context.
        _context.ProductImages.Add(image);

        // Save the image record to the database.
        await _context.SaveChangesAsync();

        // Convert the database model into the response DTO
        // before returning it to the API controller.
        return image.ToResponseDto();
    }

    public async Task<List<ProductImageResponseDto>> GetProductImages(
        Guid productId)
    {
        // Retrieve all image records associated with the specified product.
        // Return the images in upload-date order, earliest first.
        var images = await _context.ProductImages
            .Where(images => images.ProductId == productId)
            .OrderBy(images => images.UploadedAt)
            .ToListAsync();

        // Convert each image database model into a response DTO,
        // then return the resulting list.
        return images
            .Select(image => image.ToResponseDto())
            .ToList();
    }

    public async Task<bool> DeleteImage(Guid imageId)
    {
        // Find the image record using its unique ID.
        var image = await _context.ProductImages
            .FindAsync(imageId);

        // Return false if the image does not exist in the database.
        if (image == null)
        {
            return false;
        }

        // Resolve the web root directory, using wwwroot as a fallback.
        var webRootPath = _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot"
            );

        // Convert the stored URL path into a physical file path.
        // TrimStart removes the leading slash, while Replace converts
        // URL separators into the current operating system's separators.
        var physicalFilePath = Path.Combine(
            webRootPath,
            image.FilePath
                .TrimStart('/')
                .Replace(
                    '/', 
                    Path.DirectorySeparatorChar
                )
        );

        // Delete the physical image file if it exists.
        if (File.Exists(physicalFilePath))
        {
            File.Delete(physicalFilePath);
        }

        // Remove the image record from the database.
        _context.ProductImages.Remove(image);

        // Save the deletion to the database.
        await _context.SaveChangesAsync();

        // Indicate that the image was successfully deleted.
        return true;
    }
}