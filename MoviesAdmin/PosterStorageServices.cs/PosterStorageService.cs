
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

        // Verify that the file contains a recognized image signature.
        if (posterFile.Length < 12)
        {
            throw new InvalidDataException(
                "The uploaded file is not a valid image.");
        }

        byte[] header = new byte[12];

        await using (var headerStream = posterFile.OpenReadStream())
        {
            await headerStream.ReadExactlyAsync(header);
        }

        bool validImage = extension switch
        {
            ".jpg" or ".jpeg" =>
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            ".png" =>
                header.AsSpan(0, 8).SequenceEqual(
                    new byte[]
                    {
                        0x89, 0x50, 0x4E, 0x47,
                        0x0D, 0x0A, 0x1A, 0x0A
                    }),

            ".webp" =>
                header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                header.AsSpan(8, 4).SequenceEqual("WEBP"u8),

            _ => false
        };

        if (!validImage)
        {
            throw new InvalidDataException(
                "The file content does not match its image extension.");
        }

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

        // Open the uploaded file and send it to Blob Storage.
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
