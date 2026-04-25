using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Europiyum.Cms.Application.Storage;

public class LocalMediaFileStorage : IMediaFileStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly MediaStorageOptions _options;
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".pdf"
    };

    private const long MaxBytes = 12L * 1024 * 1024;

    public LocalMediaFileStorage(IWebHostEnvironment env, IOptions<MediaStorageOptions> options)
    {
        _env = env;
        _options = options.Value;
    }

    public async Task<MediaStorageResult> WriteAsync(
        string companyCode,
        Stream stream,
        string originalFileName,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
            throw new InvalidOperationException("Şirket kodu gerekli.");

        if (contentLength > MaxBytes)
            throw new InvalidOperationException("Dosya en fazla 12 MB olabilir.");

        var ext = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext))
            throw new InvalidOperationException("İzin verilen uzantılar: jpg, png, gif, webp, svg, pdf.");

        var safeCompany = string.Join("-", companyCode.Split(Path.GetInvalidFileNameChars()));
        if (string.IsNullOrWhiteSpace(safeCompany))
            throw new InvalidOperationException("Geçersiz şirket kodu.");

        var stored = $"{Guid.NewGuid():N}{ext}";
        var now = DateTime.UtcNow;
        var year = now.Year.ToString("D4");
        var month = now.Month.ToString("D2");
        var relative = $"{safeCompany}/{year}/{month}/{stored}".Replace('\\', '/');

        var mediaRoot = ResolveMediaRoot();
        var dir = Path.Combine(mediaRoot, safeCompany, year, month);
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, stored);

        await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await stream.CopyToAsync(fs, cancellationToken);
        }

        var len = new FileInfo(fullPath).Length;
        return new MediaStorageResult(relative, stored, len, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
    }

    public void TryDeletePhysical(string relativePathUnderMedia)
    {
        if (string.IsNullOrWhiteSpace(relativePathUnderMedia))
            return;

        var parts = relativePathUnderMedia.Replace('/', Path.DirectorySeparatorChar);
        var full = Path.Combine(ResolveMediaRoot(), parts);
        try
        {
            if (File.Exists(full))
                File.Delete(full);
        }
        catch
        {
            /* ignore IO cleanup failures */
        }
    }

    private string ResolveMediaRoot()
    {
        var configured = _options.RootPath?.Trim();
        if (string.IsNullOrWhiteSpace(configured))
            return Path.Combine(_env.WebRootPath, "media");

        if (Path.IsPathRooted(configured))
            return configured;

        return Path.GetFullPath(Path.Combine(_env.ContentRootPath, configured));
    }
}
