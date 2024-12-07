namespace SimpleDrive.Services;

public interface IStorageService
{
    Task<bool> SaveBlobAsync(string id, byte[] data, string contentType);
    Task<byte[]?> GetBlobAsync(string id, string contentType);
}
