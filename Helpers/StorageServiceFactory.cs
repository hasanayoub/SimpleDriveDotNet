using SimpleDrive.Models;
using SimpleDrive.Services;

namespace SimpleDrive.Helpers;

public class StorageServiceFactory(
    S3StorageService s3StorageService,
    LocalFileStorageService localStorageService,
    DatabaseStorageService databaseStorageService,
    FtpStorageService ftpStorageService)
{
    public IStorageService GetStorageService(BlobStorageType storageType)
    {
        return storageType switch
        {
            BlobStorageType.S3 => s3StorageService,
            BlobStorageType.Local => localStorageService,
            BlobStorageType.Database => databaseStorageService,
            BlobStorageType.Ftp => ftpStorageService,
            _ => throw new ArgumentException("Invalid storage type"),
        };
    }
}