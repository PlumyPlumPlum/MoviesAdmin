
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text.RegularExpressions;

namespace MoviesAdmin.Services;

public class PosterStorageService
{
    private readonly BlobContainerClient _container;

    public PosterStorageService(BlobServiceClient blobServiceClient)
    {
        _container = blobServiceClient
            .GetBlobContainerClient("movie-posters");
    }

    public async Task<string> UploadAsync(IFormFile posterFile, string movieTitle)
    {
        // Make sure the file isn't empty or too large.
        if (posterFile.Length == 0 ||
            posterFile.Length > 5 * 1024 * 1024)
        {
            throw new InvalidDataException(
                "The poster must be between 1 byte and 5 MB.");
        }

        // Identify the file extension.
        string extension = Path.GetExtension(
            posterFile.FileName).ToLowerInvariant();

        string contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new InvalidDataException(
                "Only JPG, PNG and WebP images are allowed.")
        };

        // Create the container if it doesn't exist.
        await _container.CreateIfNotExistsAsync(
            PublicAccessType.Blob);

        // Generate a unique filename.

        string safeTitle = Regex.Replace(movieTitle.Trim().ToLowerInvariant(),
            @"[^a-z0-9]+", "-").Trim('-');

        if (string.IsNullOrWhiteSpace(safeTitle))
        {
            safeTitle = "movie";
        }

        string blobName =
            $"{safeTitle}-{Guid.NewGuid():N}{extension}";


        // Get a reference to the new blob.
        var blobClient = _container.GetBlobClient(blobName);

        // Open the uploaded file and send it to Azurite.
        await using var stream = posterFile.OpenReadStream();

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },
                Conditions = new BlobRequestConditions
                {
                    IfNoneMatch = ETag.All
                }
            });

        // Return the URL of the uploaded poster.
        return blobClient.Uri.ToString();
    }
}
