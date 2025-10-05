using MoreSpeakers.Web.Data;
using MoreSpeakers.Web.Models;

namespace MoreSpeakers.Web.Services;


// File Upload Service Interface
public interface IFileUploadService
{
    Task<string?> UploadHeadshotAsync(IFormFile file, Guid userId);
    Task<bool> DeleteHeadshotAsync(string fileName);
    bool IsValidImageFile(IFormFile file);
    string GetHeadshotPath(string fileName);
}