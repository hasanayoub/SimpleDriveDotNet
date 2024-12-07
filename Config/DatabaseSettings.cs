namespace SimpleDrive.Config;

public class DatabaseSettings
{
    public string Server { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
    public string? Password { get; set; } = null!;
    public string? User { get; set; } = null!;
    public string Rdbms { get; init; } = null!;
    public string RdbmsVersion { get; init; } = null!;
}