using Microsoft.AspNetCore.Http;

namespace EventEaseApplication.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folderName);
        Task DeleteImageAsync(string? imageUrl);
    }
}