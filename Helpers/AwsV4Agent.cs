using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SimpleDrive.Helpers;

public class AwsV4Agent(string? service, string? region, string? accessKey, string? secretKey)
{
    private const string Iso8601DateTimeFormat = "yyyyMMddTHHmmssZ";
    private const string Iso8601DateFormat = "yyyyMMdd";

    public Task<HttpRequestMessage> PrepareRequestMessageAsync(string url, HttpMethod? method = null, byte[]? payload = null)
    {
        payload ??= Encoding.Default.GetBytes("");
        method ??= HttpMethod.Get;

        var request = new HttpRequestMessage(method, url);
        request.Content = new ByteArrayContent(payload);
        request.Headers.Host = request.RequestUri?.Host;

        var utcNow = DateTimeOffset.UtcNow;
        var amzLongDate = utcNow.ToString(Iso8601DateTimeFormat, CultureInfo.InvariantCulture);
        var amzShortDate = utcNow.ToString(Iso8601DateFormat, CultureInfo.InvariantCulture);

        var payloadHash = payload.Length > 0 ? Hash(payload) : "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        request.Headers.Add("x-amz-date", amzLongDate);
        request.Headers.Add("x-amz-content-sha256", payloadHash);
        var signedHeaders = string.Join(";", request.Headers.OrderBy(h => h.Key.ToLowerInvariant()).Select(h => h.Key.ToLowerInvariant()));

        var canonicalRequest = BuildCanonicalRequest(request, signedHeaders, payloadHash);
        var stringToSign = BuildStringToSign(canonicalRequest, amzLongDate, amzShortDate);
        var signature = CalculateSignature(amzShortDate, stringToSign);
        var authorizationValue = $"AWS4-HMAC-SHA256 Credential={accessKey}/{amzShortDate}/{region}/{service}/aws4_request, SignedHeaders={signedHeaders}, Signature={signature}";
        request.Headers.TryAddWithoutValidation("Authorization", authorizationValue);

        Console.WriteLine("CanonicalRequest: " + canonicalRequest);
        Console.WriteLine("String to Sign: " + stringToSign);
        Console.WriteLine("Signature is:" + signature);
        Console.WriteLine("Authorization Value: " + authorizationValue);

        return Task.FromResult(request);
    }

    public static string BuildCanonicalRequest(HttpRequestMessage msg, string signedHeaders, string payloadHash)
    {
        var canonicalUri = string.Join("/", msg.RequestUri?.AbsolutePath.Split('/').Select(Uri.EscapeDataString) ?? Array.Empty<string>());
        var canonicalQueryString = GetCanonicalQueryParams(msg);
        var canonicalHeaders = string.Join("\n", msg.Headers.OrderBy(h => h.Key.ToLowerInvariant()).Select(h => $"{h.Key.ToLowerInvariant()}:{string.Join(",", h.Value)}")) + "\n";
        Console.WriteLine("payloadHash: " + $"{payloadHash}");
        return $"{msg.Method}\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
    }

    private string? BuildStringToSign(string canonicalRequest, string amzLongDate, string? amzShortDate)
    {
        var credentialScope = $"{amzShortDate}/{region}/{service}/aws4_request";
        return $"AWS4-HMAC-SHA256\n{amzLongDate}\n{credentialScope}\n{Hash(Encoding.UTF8.GetBytes(canonicalRequest))}";
    }

    private string CalculateSignature(string? amzShortDate, string? stringToSign)
    {
        var dateKey = HmacSha256(Encoding.UTF8.GetBytes("AWS4" + secretKey), amzShortDate);
        var dateRegionKey = HmacSha256(dateKey, region);
        var dateRegionServiceKey = HmacSha256(dateRegionKey, service);
        var signingKey = HmacSha256(dateRegionServiceKey, "aws4_request");

        return ToHexString(HmacSha256(signingKey, stringToSign));
    }

    public static string GetCanonicalQueryParams(HttpRequestMessage request)
    {
        var querystring = HttpUtility.ParseQueryString(request.RequestUri?.Query ?? string.Empty);
        var sortedParams = querystring.AllKeys
            .Where(k => k != null)
            .OrderBy(k => k, StringComparer.Ordinal)
            .Select(k => $"{Uri.EscapeDataString(k ?? string.Empty)}={Uri.EscapeDataString(querystring[k] ?? string.Empty)}");
        return string.Join("&", sortedParams);
    }

    public static byte[] HmacSha256(byte[] key, string? data) =>
        new HMACSHA256(key).ComputeHash(Encoding.UTF8.GetBytes(data));

    public static string Hash(byte[] bytes)
    {
        var hashData = SHA256.HashData(bytes);
        return ToHexString(hashData);
    }

    public static string ToHexString(byte[] array) =>
        string.Concat(array.Select(b => b.ToString("x2")));
}