namespace SimpleDrive.Config;

public class FtpSettings
{
    public string FtpUrl { get; init; } = string.Empty;
    public string? FtpUsername { get; set; } = string.Empty;
    public string? FtpPassword { get; set; } = string.Empty;
}