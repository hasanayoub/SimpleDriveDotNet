namespace SimpleDrive.Config;

public class S3Settings
{
    public string BucketUrl { get; init; } = null!;
    public string? Region { get; init; } = null!;
    public string? AccessKey { get; init; } = null!;
    public string? SecretKey { get; init; } = null!;
}