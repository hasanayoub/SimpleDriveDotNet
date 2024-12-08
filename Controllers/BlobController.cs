using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SimpleDrive.Config;
using SimpleDrive.Helpers;
using SimpleDrive.Models;

namespace SimpleDrive.Controllers;

[ApiController]
[Route("/api/v1/blobs")]
[Authorize] // This secures the endpoint
public class BlobController(AppDbContext context, StorageServiceFactory storageServiceFactory, IOptions<StorageSettings> options) : ControllerBase
{
    private readonly BlobStorageType _storageType = Enum.Parse<BlobStorageType>(options.Value.StorageType);


    /// <summary>
    ///  Get a blob by id
    /// </summary>
    /// <description>
    /// This endpoint returns a blob by id from the database, S3, Local file system, or the storage service based on the storage type. 
    /// </description>
    /// <param name="id">
    /// The id of the blob to retrieve
    /// </param>
    /// <returns>
    /// A blob response object
    /// </returns>
    /// <response code="200">Returns the blob</response>
    /// <response code="404">If the blob is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BlobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlob([FromRoute] string id)
    {
        var metadata = await context.BlobMetadata.FirstOrDefaultAsync(b => b.BlobId == id);
        if (metadata == null) return NotFound(new { message = "Blob not found" });

        var storageService = storageServiceFactory.GetStorageService(metadata.StorageType);
        var data = await storageService.GetBlobAsync(id, metadata.ContentType);
        var base64 = Convert.ToBase64String(data ?? []);

        return Ok(new BlobResponse()
        {
            Data = base64,
            CreatedAt = metadata.CreatedAt,
            Id = metadata.BlobId,
            Size = metadata.Size
        });
    }

    /// <summary>
    /// Get all blobs or blobs by storage type from the database. 
    /// </summary>
    /// <param name="storageType">The storage type of the blobs to retrieve. Valid values are "Local", "S3", "Ftp", "Database".</param>
    /// <returns>An array of blob response objects.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BlobResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<BlobResponse[]> GetBlobs([FromQuery] BlobStorageType? storageType)
    {
        var blobs = storageType != null
            ? await context.BlobMetadata.Where(m => m.StorageType == storageType).ToListAsync()
            : await context.BlobMetadata.ToListAsync();

        var blobResponses = blobs.Select(blob => new BlobResponse()
        {
            Data = "",
            CreatedAt = blob.CreatedAt,
            Id = blob.BlobId,
            Size = blob.Size
        }).ToList();

        return blobResponses.ToArray();
    }

    /// <summary>
    /// Upload a blob to the database and the storage service based on the storage type. This storage type is set in the appsettings.json file.      
    /// </summary>
    /// <param name="request">
    /// The blob request object
    /// </param>
    /// <returns>
    /// A blob response object 
    /// </returns>
    /// <response code="200">Returns the blob</response>
    /// <response code="400">If the request is invalid or the blob id already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(BlobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBlob([FromBody] BlobRequest request)
    {
        // Validate the request
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new { message = "Invalid input", errors });
        }

        var (contentType, base64DataString) = ExtractContentTypeAndData(request.Data);
        var metadata = new BlobMetadata
        {
            BlobId = request.Id,
            Size = request.Data.Length,
            CreatedAt = DateTime.UtcNow,
            StorageType = _storageType,
            ContentType = contentType
        };

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            context.BlobMetadata.Add(metadata);

            var existed = await context.BlobMetadata.FirstOrDefaultAsync(blobMetadata => blobMetadata.BlobId == metadata.BlobId);

            if (existed != null) return BadRequest(new { message = "Blob Id already existed" });

            var rowAffected = await context.SaveChangesAsync();
            if (rowAffected <= 0) return BadRequest(new { message = "Failed to store blob" });


            var data = Convert.FromBase64String(base64DataString);

            var storageService = storageServiceFactory.GetStorageService(_storageType);
            var result = await storageService.SaveBlobAsync(request.Id, data, contentType);
            if (!result) return BadRequest(new { message = "Failed to store blob" });

            // commit transaction
            await transaction.CommitAsync();
            return Ok(new BlobResponse()
            {
                Data = Convert.ToBase64String(data),
                CreatedAt = metadata.CreatedAt,
                Id = metadata.BlobId,
                Size = metadata.Size
            });
        }
        catch (Exception e)
        {
            // rollback transaction
            return BadRequest(new { message = e.Message });
        }
    }

    private static (string ContentType, string Base64Data) ExtractContentTypeAndData(string base64Input)
    {
        // Check if the Base64 string includes content type
        if (!base64Input.StartsWith("data:")) return ("", base64Input);
        var split = base64Input.Split(';');
        var contentType = split[0].Replace("data:", string.Empty);
        var base64Data = split[1].Replace("base64,", string.Empty);
        return (contentType, base64Data);
    }
}

/// <summary>
/// A blob request object. 
/// </summary>
public class BlobRequest
{
    /// <summary>
    /// The id of the blob.
    /// </summary>
    [JsonPropertyName("id")]
    [Required(ErrorMessage = "Invalid input. Id is required.")]
    [StringLength(50, ErrorMessage = "Invalid input. Id must be less than 50 characters.")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The base64 encoded data of the blob. 
    /// </summary>
    [JsonPropertyName("data")]
    [Required(ErrorMessage = "Invalid input. Data is required.")]
    [RegularExpression(@"^data:image\/(png|jpeg|jpg|gif);base64,[A-Za-z0-9+/=]+$",
        ErrorMessage = "Invalid input. Data must be a valid base64 image.")]
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// A blob response object.  
/// </summary>
public class BlobResponse
{
    /// <summary>
    /// The id of the blob 
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The base64 encoded data of the blob
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; init; } = string.Empty;

    /// <summary>
    /// The size of the blob in bytes 
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// The creation date of the blob 
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }
}