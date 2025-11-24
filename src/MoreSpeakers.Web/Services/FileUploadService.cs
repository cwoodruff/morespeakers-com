namespace MoreSpeakers.Web.Services;


[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger) : IFileUploadService
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".gif"];

    public async Task<string?> UploadHeadshotAsync(IFormFile file, Guid userId)
    {
        if (!IsValidImageFile(file))
        {
            return null;
        }

        var filePath = string.Empty;
        try
        {
            var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "headshots");
            Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}{fileExtension}";
            filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while uploading headshots. FilePath: '{FilePath}' User: '{UserId}'", filePath, userId);
            return null;
        }
    }

    public bool DeleteHeadshotAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(environment.WebRootPath, "uploads", "headshots", fileName);
            if (!File.Exists(filePath))
            {
                return false;
            }

            File.Delete(filePath);
            return true;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete the headshot. Filename: '{Filename}'", fileName );
            return false;
        }
    }

    public bool IsValidImageFile(IFormFile file)
    {
        switch (file.Length)
        {
            case 0:
            case > MaxFileSize:
                return false;
            default:
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    return _allowedExtensions.Contains(extension);
                }
        }
    }

    public string GetHeadshotPath(string fileName)
    {
        return $"/uploads/headshots/{fileName}";
    }
}