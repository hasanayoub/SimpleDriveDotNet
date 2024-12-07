using System.Globalization;
using System.Xml.Linq;
using Microsoft.IdentityModel.Tokens;
using SimpleDrive.Helpers;

namespace SimpleDrive.Services;

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class S3StorageService(HttpClient httpClient, string bucketUrl, string? accessKey, string? secretKey, string? region)
    : IStorageService
{
    public async Task<bool> SaveBlobAsync(string id, byte[] data, string contentType)
    {
        await CreateBucketIfNotFound();
        var uri = new Uri($"{bucketUrl}/files/{id}");
        var aws = new AwsV4Agent("s3", region, accessKey, secretKey);
        var request = await aws.PrepareRequestMessageAsync(uri.ToString(), HttpMethod.Put, data);
        var response = await httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private async Task CreateBucketIfNotFound(string bucketName = "files")
    {
        var aws = new AwsV4Agent("s3", region, accessKey, secretKey);
        var uri = new Uri($"{bucketUrl}");
        var request = await aws.PrepareRequestMessageAsync(uri.ToString());
        var response = await httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var doc = XDocument.Parse(content);
            var bucketNames = doc.Descendants("Name").Select(x => x.Value).ToList();

            if (!bucketNames.Contains(bucketName))
            {
                uri = new Uri($"{bucketUrl}/{bucketName}");
                request = await aws.PrepareRequestMessageAsync(uri.ToString(), HttpMethod.Put);
                await httpClient.SendAsync(request);
            }
        }
    }

    public async Task<byte[]?> GetBlobAsync(string id, string contentType)
    {
        var uri = new Uri($"{bucketUrl}/files/{id}");
        var aws = new AwsV4Agent("s3", region, accessKey, secretKey);
        var request = await aws.PrepareRequestMessageAsync(uri.ToString());
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
}