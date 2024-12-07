namespace SimpleDrive.Config;

public class S3Settings
{
    public string BucketUrl { get; init; } = null!;
    public string? Region { get; init; } = null!;
    public string? AccessKey { get; set; } = null!;
    public string? SecretKey { get; set; } = null!;
}