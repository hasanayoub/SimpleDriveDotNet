using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using SimpleDrive.Controllers;
using SimpleDrive.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace SimpleDrive.Tests;

public class BlobControllerIntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient;
    private static IConfigurationRoot? _configuration;
    private static string? _filePath;
    private static string? _fileHashValue;
    private static string? _serverUrl;

    public BlobControllerIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        BuildConfiguration();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_serverUrl!)
        };
    }

    private static void BuildConfiguration()
    {
        _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        _filePath = _configuration["Testing:FilePath"];
        _serverUrl = _configuration["Testing:ServerUrl"];
        _fileHashValue = _configuration["Testing:FileHashValue"];
    }

    [Fact]
    public async Task UploadBlob_TestHash()
    {
        var fileContent = await File.ReadAllBytesAsync(_filePath!);
        var awsHash = AwsV4Agent.Hash(fileContent);
        Assert.Equal(_fileHashValue, awsHash);
    }

    [Fact]
    public async Task UploadBlob_ShouldReturnOk()
    {
        // read file content from disk.
        var fileContent = await File.ReadAllBytesAsync(_filePath!);
        var fileContentBase64 = Convert.ToBase64String(fileContent);
        // Arrange
        var blobRequest = new
        {
            id = Guid.NewGuid().ToString(),
            data = $"data:image/jpeg;base64,{fileContentBase64}"
        };

        var token = await GetToken();

        var jsonContent = new StringContent(JsonSerializer.Serialize(blobRequest), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Act
        var response = await _httpClient.PostAsync("/api/v1/blobs", jsonContent);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var blobResponse = JsonSerializer.Deserialize<BlobResponse>(responseBody);
        Assert.NotNull(blobResponse);
    }

    [Fact]
    public async Task UploadBlob_ShouldReturn401Response()
    {
        var response = await _httpClient.GetAsync("/api/v1/blobs");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadBlob_GetBlobsListData() // todo: fix this test
    {
        var storageType = _configuration!["StorageType"] ?? "Local";
        // read file content from disk.
        var token = await GetToken();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync("/api/v1/blobs?storageType=" + storageType);
        _testOutputHelper.WriteLine("Response: " + response.StatusCode);

        // Assert
        response.EnsureSuccessStatusCode();
        var blobsMetaData = await JsonSerializer.DeserializeAsync<BlobResponse[]>(await response.Content.ReadAsStreamAsync());

        if (blobsMetaData != null)
        {
            var blobCount = blobsMetaData.Length;

            _testOutputHelper.WriteLine("Blobs: " + blobsMetaData.Length);

            var blobsWithData = new List<BlobResponse>();
            foreach (var blob in blobsMetaData)
            {
                _testOutputHelper.WriteLine("Blob: " + blob.Id);
                var blobData = await _httpClient.GetAsync($"/api/v1/blobs/{blob.Id}");
                blobData.EnsureSuccessStatusCode();
                _testOutputHelper.WriteLine("Blob Data: " + blobData.StatusCode);
                var blobContent = await blobData.Content.ReadAsStringAsync();
                var blobMetaData = JsonSerializer.Deserialize<BlobResponse>(blobContent);
                if (blobMetaData != null) blobsWithData.Add(blobMetaData);
            }

            Assert.Equal(blobCount, blobsWithData.Count);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Fact]
    public async Task UploadBlob_CheckInvalidLogin()
    {
        var response = await _httpClient.PostAsync("/api/v1/auth/login",
            new StringContent(JsonSerializer.Serialize(new { username = "admin", password = "admin" }),
                Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private Task<string?> GetToken()
    {
        var response = _httpClient.PostAsync("/api/v1/auth/login",
            new StringContent(JsonSerializer.Serialize(new { username = _configuration!["Testing:Username"], password = _configuration["Testing:Password"] }),
                Encoding.UTF8, "application/json")).Result;

        response.EnsureSuccessStatusCode();
        var responseBody = response.Content.ReadAsStringAsync().Result;
        var token = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        return Task.FromResult(token?["token"]);
    }

    [Fact]
    public async Task UploadBlob_CheckLogin()
    {
        var token = await GetToken();
        Assert.NotNull(token);
    }

    [Fact]
    public async Task UploadBlob_GetBlobsList() // todo: fix this test
    {
        var token = await GetToken();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // read file content from disk.
        var response = await _httpClient.GetAsync("/api/v1/blobs");

        // Assert
        response.EnsureSuccessStatusCode();
        var blobsMetaData = await JsonSerializer.DeserializeAsync<BlobResponse[]>(await response.Content.ReadAsStreamAsync());
        // assert blobsMetaData is not null and has data
        Assert.NotNull(blobsMetaData);
        Assert.NotEmpty(blobsMetaData);
    }

    [Fact]
    public async Task UploadBlob_WithInvalidData_ShouldReturnBadRequest() // todo: fix this test
    {
        // Arrange
        var invalidBlobRequest = new
        {
            Id = "",
            Data = ""
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(invalidBlobRequest),
            Encoding.UTF8,
            "application/json");
        var token = await GetToken();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _httpClient.PostAsync("/api/v1/blobs", jsonContent);

        // Assert
        Assert.False(response.IsSuccessStatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid input", responseBody);
    }
}