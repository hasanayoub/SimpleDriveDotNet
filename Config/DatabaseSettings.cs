namespace SimpleDrive.Config;

public class DatabaseSettings
{
    public string Server { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string User { get; init; } = null!;
    public string Rdbms { get; init; } = null!;
    public string RdbmsVersion { get; init; } = null!;
}