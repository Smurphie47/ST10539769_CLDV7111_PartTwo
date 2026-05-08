using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEaseApplication.Services
{
    public class AzureBlobImageService : IImageService
    {
        private readonly IConfiguration _configuration;

        private readonly string[] _allowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public AzureBlobImageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private BlobContainerClient GetContainerClient()
        {
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerName = _configuration["AzureBlobStorage:ContainerName"] ?? "eventease-images";

            if (string.IsNullOrWhiteSpace(connectionString) ||
                connectionString == "PASTE_YOUR_AZURE_STORAGE_CONNECTION_STRING_HERE")
            {
                throw new InvalidOperationException("Azure Blob Storage connection string is missing.");
            }

            var containerClient = new BlobContainerClient(connectionString, containerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);

            return containerClient;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No image file was uploaded.");
            }

            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException("Image size cannot exceed 5MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Only JPG, JPEG, PNG, and WEBP image files are allowed.");
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                throw new ArgumentException("Only image files are allowed.");
            }

            var containerClient = GetContainerClient();

            var fileName = $"{folderName}/{Guid.NewGuid()}{extension}";
            var blobClient = containerClient.GetBlobClient(fileName);

            await using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                }
            });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            {
                return;
            }

            var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length < 2)
            {
                return;
            }

            var blobName = string.Join("/", pathParts.Skip(1));
            var containerClient = GetContainerClient();
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}