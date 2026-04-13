namespace Europiyum.Cms.Application.Abstractions;

public interface IMediaFileStorage
{
    /// <summary>Writes under wwwroot/media/ and returns path segments after media/.</summary>
    Task<MediaStorageResult> WriteAsync(
        string companyCode,
        Stream stream,
        string originalFileName,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes file under wwwroot/media/ if it exists.</summary>
    void TryDeletePhysical(string relativePathUnderMedia);
}

public sealed record MediaStorageResult(string RelativePath, string StoredFileName, long SizeBytes, string ContentType);
