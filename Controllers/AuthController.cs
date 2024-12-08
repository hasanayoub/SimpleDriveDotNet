using System.Text.Json.Serialization;
using SimpleDrive.Helpers;
using SimpleDrive.Services;

namespace SimpleDrive.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// The AuthController class is responsible for handling authentication requests.  
/// </summary>
[ApiController]
[Route("/api/v1/[controller]")]
public class AuthController(UserAuthService userAuthService, TokenAgent tokenAgent) : ControllerBase
{
    /// <summary>
    /// The login endpoint is used to authenticate a user and return a JWT token.  
    /// </summary>
    /// <param name="request">
    /// The login request object containing the username and password.
    /// </param>
    /// <returns>
    /// A JWT token if the user is authenticated.
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!userAuthService.VerifyPassword(request.Username, request.Password)) return Unauthorized(new { Message = "Invalid username or password." });
        var token = tokenAgent.GenerateJwtToken(request.Username);
        return Ok(new { Token = token });
    }
}

/// <summary>
/// The LoginRequest class is used to deserialize the login request object. 
/// </summary>
/// <param name="Username">
/// The username of the user attempting to log in.
/// </param>
/// <param name="Password">
/// The password of the user attempting to log in.
/// </param>
public record LoginRequest(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("password")]
    string Password);

/// <summary>
/// The LoginResponse class is used to serialize the login response object. 
/// </summary>
/// <param name="Token">
/// The JWT token to return to the user.
/// </param>
public record LoginResponse(
    [property: JsonPropertyName("token")] string Token);