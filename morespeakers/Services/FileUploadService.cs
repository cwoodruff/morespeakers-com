namespace morespeakers.Services;

public class FileUploadService(IWebHostEnvironment environment) : IFileUploadService
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public async Task<string?> UploadHeadshotAsync(IFormFile file, Guid userId)
    {
        if (!IsValidImageFile(file))
            return null;

        try
        {
            var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "headshots");
            Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteHeadshotAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(environment.WebRootPath, "uploads", "headshots", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > MaxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    public string GetHeadshotPath(string fileName)
    {
        return $"/uploads/headshots/{fileName}";
    }
}