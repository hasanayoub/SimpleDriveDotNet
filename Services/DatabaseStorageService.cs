using Microsoft.EntityFrameworkCore;
using SimpleDrive.Models;

namespace SimpleDrive.Services;

public class DatabaseStorageService(AppDbContext context) : IStorageService
{
    public async Task<bool> SaveBlobAsync(string id, byte[] data, string contentType)
    {
        var blob = new BlobData()
        {
            BlobId = id,
            MediumBlobData = data,
            ContentType = contentType
        };
        context.BlobData.Add(blob);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]?> GetBlobAsync(string id, string contentType)  
    {
        var blob = await context.BlobData.FirstOrDefaultAsync(b => b.BlobId == id);
        return blob?.MediumBlobData;
    }
}