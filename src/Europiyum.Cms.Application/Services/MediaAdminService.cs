using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class MediaAdminService
{
    private readonly CmsDbContext _db;
    private readonly IMediaFileStorage _storage;

    public MediaAdminService(CmsDbContext db, IMediaFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<IReadOnlyList<MediaListItemVm>> ListForCompanyAsync(
        int companyId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var q = _db.MediaFiles.AsNoTracking().Where(m => m.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(m => EF.Functions.ILike(m.OriginalFileName, $"%{s}%")
                || EF.Functions.ILike(m.RelativePath, $"%{s}%"));
        }

        return await q
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MediaListItemVm
            {
                Id = m.Id,
                OriginalFileName = m.OriginalFileName,
                RelativePath = m.RelativePath,
                ContentType = m.ContentType,
                SizeBytes = m.SizeBytes,
                AltText = m.AltText
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CmsOpResult> TryUploadAsync(
        int companyId,
        string companyCode,
        Stream stream,
        string originalFileName,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        if (contentLength <= 0)
            return CmsOpResult.Fail("Boş dosya.");

        try
        {
            var written = await _storage.WriteAsync(companyCode, stream, originalFileName, contentType, contentLength, cancellationToken);

            var entity = new MediaFile
            {
                CompanyId = companyId,
                OriginalFileName = originalFileName,
                StoredFileName = written.StoredFileName,
                ContentType = written.ContentType,
                SizeBytes = written.SizeBytes,
                RelativePath = written.RelativePath
            };
            _db.MediaFiles.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return CmsOpResult.Success();
        }
        catch (InvalidOperationException ex)
        {
            return CmsOpResult.Fail(ex.Message);
        }
    }

    /// <summary>Yüklenen dosyanın public URL’sini (<c>/media/...</c>) döndürür.</summary>
    public async Task<(CmsOpResult result, string? publicUrl)> TryUploadWithPublicUrlAsync(
        int companyId,
        string companyCode,
        Stream stream,
        string originalFileName,
        string contentType,
        long contentLength,
        CancellationToken cancellationToken = default)
    {
        if (contentLength <= 0)
            return (CmsOpResult.Fail("Boş dosya."), null);

        try
        {
            var written = await _storage.WriteAsync(companyCode, stream, originalFileName, contentType, contentLength, cancellationToken);

            var entity = new MediaFile
            {
                CompanyId = companyId,
                OriginalFileName = originalFileName,
                StoredFileName = written.StoredFileName,
                ContentType = written.ContentType,
                SizeBytes = written.SizeBytes,
                RelativePath = written.RelativePath
            };
            _db.MediaFiles.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            var url = "/media/" + written.RelativePath.Replace('\\', '/');
            return (CmsOpResult.Success(), url);
        }
        catch (InvalidOperationException ex)
        {
            return (CmsOpResult.Fail(ex.Message), null);
        }
    }

    public async Task<CmsOpResult> TryDeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
    {
        var media = await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId, cancellationToken);
        if (media is null)
            return CmsOpResult.Fail("Dosya bulunamadı.");

        var usedByComponent = await _db.ComponentItems.AnyAsync(c => c.PrimaryMediaId == id, cancellationToken);
        var usedBySeo = await _db.SeoMetadata.AnyAsync(s => s.OgImageMediaId == id, cancellationToken);
        if (usedByComponent || usedBySeo)
            return CmsOpResult.Fail("Dosya bileşen veya SEO kaydında kullanılıyor; önce referansı kaldırın.");

        var path = media.RelativePath;
        _db.MediaFiles.Remove(media);
        await _db.SaveChangesAsync(cancellationToken);
        _storage.TryDeletePhysical(path);
        return CmsOpResult.Success();
    }

    public async Task<IReadOnlyList<MediaListItemVm>> ListRecentForPickerAsync(int companyId, int take, CancellationToken cancellationToken = default) =>
        await _db.MediaFiles.AsNoTracking()
            .Where(m => m.CompanyId == companyId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .Select(m => new MediaListItemVm
            {
                Id = m.Id,
                OriginalFileName = m.OriginalFileName,
                RelativePath = m.RelativePath,
                ContentType = m.ContentType,
                SizeBytes = m.SizeBytes,
                AltText = m.AltText
            })
            .ToListAsync(cancellationToken);
}
