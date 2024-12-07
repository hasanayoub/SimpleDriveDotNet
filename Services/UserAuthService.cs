namespace SimpleDrive.Services;

using BCrypt.Net;

public class UserAuthService
{
    public string Username { get; set; } = null!;
    public string HashedPassword { get; set; } = null!;

    public bool VerifyPassword(string username, string password)
    {
        return Username == username && BCrypt.Verify(password, HashedPassword);
    }
}