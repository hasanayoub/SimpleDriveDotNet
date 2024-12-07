namespace SimpleDrive.Config;

public class JwtTokenSettings
{
    public string? JwtSecretKey { get; set; }
    public string TokenIssuer { get; init; } = null!;
    public string TokenAudience { get; init; } = null!;
}