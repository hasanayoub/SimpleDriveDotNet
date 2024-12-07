using System.Text.Json.Serialization;
using SimpleDrive.Helpers;
using SimpleDrive.Services;

namespace SimpleDrive.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/[controller]")]
public class AuthController(UserAuthService userAuthService, TokenAgent tokenAgent) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!userAuthService.VerifyPassword(request.Username, request.Password)) return Unauthorized(new { Message = "Invalid username or password." });
        var token = tokenAgent.GenerateJwtToken(request.Username);
        return Ok(new { Token = token });
    }
}

public record LoginRequest(
    [property: JsonPropertyName("username")]
    string Username,
    [property: JsonPropertyName("password")]
    string Password);