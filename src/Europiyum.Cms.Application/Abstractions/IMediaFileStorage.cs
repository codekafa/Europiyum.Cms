namespace Europiyum.Cms.Application.Abstractions;

public interface IMediaFileStorage
{
    /// <summary>Writes under configured media root and returns path segments after media request root.</summary>
    Task<MediaStorageResult> WriteAsync(
        string companyCode,
        Stream stream,
        string originalFileName,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes file under configured media root if it exists.</summary>
    void TryDeletePhysical(string relativePathUnderMedia);
}

public sealed record MediaStorageResult(string RelativePath, string StoredFileName, long SizeBytes, string ContentType);
