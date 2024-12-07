namespace SimpleDrive.Config;

public class JwtTokenSettings
{
    public string JwtSecretKey { get; init; } = null!;
    public string TokenIssuer { get; init; } = null!;
    public string TokenAudience { get; init; } = null!;
}