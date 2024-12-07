using System.Net;

namespace SimpleDrive.Services;

public class FtpStorageService(string ftpServerUrl, string ftpUsername, string ftpPassword) : IStorageService
{
    private readonly string _ftpServerUrl = ftpServerUrl.TrimEnd('/');

    public async Task<bool> SaveBlobAsync(string id, byte[] data, string contentType)
    {
        try
        {
            var ext = LocalFileStorageService.GetExtFromMimeType(contentType);
            var uri = new Uri($"{_ftpServerUrl}/{id}{ext}");
            var request = (FtpWebRequest)WebRequest.Create(uri);

            // Configure the FTP request
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.ContentLength = data.Length;
            request.UseBinary = true;
            request.UsePassive = true;
            request.Timeout = 30000; // 30 seconds

            // Write the data to the request stream
            await using var requestStream = await request.GetRequestStreamAsync();
            await requestStream.WriteAsync(data);
            // Get the response
            using var response = (FtpWebResponse)await request.GetResponseAsync();
            return response.StatusCode is FtpStatusCode.ClosingData or FtpStatusCode.OpeningData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading blob to FTP: {ex.Message}");
            return false;
        }
    }

    public async Task<byte[]?> GetBlobAsync(string id, string contentType)
    {
        try
        {
            var ext = LocalFileStorageService.GetExtFromMimeType(contentType);
            var uri = new Uri($"{_ftpServerUrl}/{id}{ext}");
            var request = (FtpWebRequest)WebRequest.Create(uri);

            // Configure the FTP request
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.UseBinary = true;

            // Get the response
            using var response = (FtpWebResponse)await request.GetResponseAsync();
            await using var responseStream = response.GetResponseStream();

            using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading blob from FTP: {ex.Message}");
            return null;
        }
    }
}