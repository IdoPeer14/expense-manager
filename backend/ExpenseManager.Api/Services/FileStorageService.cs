namespace ExpenseManager.Api.Services;

public class FileStorageService
{
    private readonly string _uploadPath;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _uploadPath = Path.Combine(environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, Guid userId)
    {
        // Create user-specific directory
        var userDirectory = Path.Combine(_uploadPath, userId.ToString());
        Directory.CreateDirectory(userDirectory);

        // Generate unique filename
        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(userDirectory, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;
    }

    public bool ValidateMimeType(string mimeType)
    {
        var allowedTypes = new[]
        {
            "application/pdf",
            "image/jpeg",
            "image/jpg",
            "image/png"
        };

        return allowedTypes.Contains(mimeType.ToLower());
    }

    public bool ValidateFileSize(long fileSize, long maxSizeMB = 10)
    {
        var maxSizeBytes = maxSizeMB * 1024 * 1024;
        return fileSize <= maxSizeBytes;
    }
}
