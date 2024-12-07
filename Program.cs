using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleDrive.Config;
using SimpleDrive.Helpers;
using SimpleDrive.Models;
using SimpleDrive.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

#region Culture

var cultureInfo = new CultureInfo("en-US")
{
    DateTimeFormat =
    {
        Calendar = new GregorianCalendar()
    }
};
CultureInfo.CurrentCulture = cultureInfo;
CultureInfo.CurrentUICulture = cultureInfo;

#endregion

#region Configuration

// Add Configuration
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3"));
builder.Services.Configure<FileSystemSettings>(builder.Configuration.GetSection("FileSystem"));
builder.Services.Configure<UserAuthService>(builder.Configuration.GetSection("UserAuth"));
builder.Services.Configure<StorageSettings>(builder.Configuration);

var ftpSettings = builder.Configuration.GetSection("Ftp").Get<FtpSettings>()!;
var connection = builder.Configuration.GetSection("Database").Get<DatabaseSettings>()!;
var jwtSettings = builder.Configuration.GetSection("JwtToken").Get<JwtTokenSettings>()!;

#endregion

#region Services

// Add services to the container.
builder.Services.AddControllers();

// building database connection string
switch (connection.Rdbms)
{
    case "MySql":
    {
        var connectionString = $"Server={connection.Server};Database={connection.DatabaseName};User={connection.User};Password={connection.Password};";
        builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(connection.RdbmsVersion))));
        break;
    }
    case "Postgres":
    {
        var connectionString = $"Host={connection.Server};Port=5432;Database={connection.DatabaseName};Username={connection.User};Password={connection.Password}";
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        break;
    }
    case "Oracle":
    {
        var connectionString = $"User Id={connection.User};Password={connection.Password};Data Source={connection.Server}";
        builder.Services.AddDbContext<AppDbContext>(options => options.UseOracle(connectionString));
        break;
    }
    case "SqlServer":
    {
        var connectionString = $"Server={connection.Server};Database={connection.DatabaseName};User Id={connection.User};Password={connection.Password}";
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        break;
    }
    default:
    {
        var connectionString = $"Server={connection.Server};Database={connection.DatabaseName};User={connection.User};Password={connection.Password};";
        builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(connection.RdbmsVersion))));
        break;
    }
}

// Add Database Storage Service
builder.Services.AddScoped<DatabaseStorageService>();

// Add S3 services
builder.Services.AddHttpClient<S3StorageService>();
builder.Services.AddScoped<S3StorageService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;
    return new S3StorageService(httpClient, settings.BucketUrl, settings.AccessKey, settings.SecretKey, settings.Region);
});

// Local File Storage Service 
builder.Services.AddScoped<LocalFileStorageService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<FileSystemSettings>>().Value;
    return new LocalFileStorageService(settings.StoragePath);
});

// Add FTP services
builder.Services.AddHttpClient<FtpStorageService>();
builder.Services.AddScoped<FtpStorageService>(_ => new FtpStorageService(ftpSettings.FtpUrl, ftpSettings.FtpUsername, ftpSettings.FtpPassword));

// Add UserAuthService
builder.Services.AddScoped<UserAuthService>(sp =>
{
    var setting = sp.GetRequiredService<IOptions<UserAuthService>>().Value;
    return new UserAuthService { Username = setting.Username, HashedPassword = setting.HashedPassword };
});

// Add StorageServiceFactory
builder.Services.AddScoped<StorageServiceFactory>();

// Add Token Agent Services 
builder.Services.AddScoped<TokenAgent>(_ => new TokenAgent(jwtSettings.JwtSecretKey, jwtSettings.TokenIssuer, jwtSettings.TokenAudience));

#endregion

#region Security

// Add services

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        LogValidationExceptions = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.TokenIssuer,
        ValidAudience = jwtSettings.TokenAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes($"{jwtSettings.JwtSecretKey}")) // Replace it with your secret key
    };
});
builder.Services.AddAuthorization();

#endregion

var app = builder.Build();

#region Security

app.UseAuthentication();
app.UseAuthorization();

#endregion

app.MapControllers();
app.Run();