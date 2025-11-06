namespace MoreSpeakers.Web.Services;


// File Upload Service Interface
public interface IFileUploadService
{
    Task<string?> UploadHeadshotAsync(IFormFile file, Guid userId);
    bool DeleteHeadshotAsync(string fileName);
    bool IsValidImageFile(IFormFile file);
    string GetHeadshotPath(string fileName);
}