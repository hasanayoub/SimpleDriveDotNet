namespace SimpleDrive.Services;

public class LocalFileStorageService(string storagePath) : IStorageService
{
    public async Task<bool> SaveBlobAsync(string id, byte[] data, string contentType)
    {
        // ensure that storagePath is existed.
        if (!Directory.Exists(storagePath))
        {
            Directory.CreateDirectory(storagePath);
        }

        var ext = GetExtFromMimeType(contentType);
        var filePath = Path.Combine(storagePath, $"{id}{ext}");
        await File.WriteAllBytesAsync(filePath, data);
        return true;
    }

    private static string GetMimeTypeFromExt(string ext)
    {
        var extension = Path.GetExtension(ext).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => "application/octet-stream", // Fallback for unknown types
        };
    }

    public static string GetExtFromMimeType(string contentType)
    {
        return contentType switch
        {
            "application/pdf" => ".pdf",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "text/plain" => ".txt",
            "text/html" => ".html",
            "application/json" => ".json",
            "application/xml" => ".xml",
            "application/zip" => ".zip",
            _ => ".bin", // Fallback for unknown types
        };
    }

    public async Task<byte[]?> GetBlobAsync(string id, string contentType)  
    {
        var ext = GetExtFromMimeType(contentType);
        var filePath = Path.Combine(storagePath, $"{id}{ext}");
        return await File.ReadAllBytesAsync(filePath);
    }
}