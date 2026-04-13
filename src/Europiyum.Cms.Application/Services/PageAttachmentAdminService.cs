using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class PageAttachmentAdminService
{
    private readonly CmsDbContext _db;

    public PageAttachmentAdminService(CmsDbContext db) => _db = db;

    public async Task<PageAttachmentListVm?> GetPageWithAttachmentsAsync(int pageId, int companyId, CancellationToken cancellationToken = default)
    {
        var page = await _db.Pages.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pageId && p.CompanyId == companyId, cancellationToken);
        if (page is null)
            return null;

        var defaultLang = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.DefaultLanguageId)
            .FirstAsync(cancellationToken);

        var title = await _db.PageTranslations.AsNoTracking()
            .Where(t => t.PageId == pageId && t.LanguageId == defaultLang)
            .Select(t => t.Title)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await _db.PageTranslations.AsNoTracking()
                .Where(t => t.PageId == pageId)
                .Select(t => t.Title)
                .FirstOrDefaultAsync(cancellationToken)
            ?? "(sayfa)";

        var rows = await _db.PageComponents.AsNoTracking()
            .Where(pc => pc.PageId == pageId)
            .OrderBy(pc => pc.SortOrder)
            .Select(pc => new PageAttachmentRowVm
            {
                PageComponentId = pc.Id,
                ComponentItemId = pc.ComponentItemId,
                ComponentTypeKey = pc.ComponentItem.ComponentType.Key,
                TitlePreview = pc.ComponentItem.Translations.Where(t => t.LanguageId == defaultLang).Select(t => t.Title).FirstOrDefault()
                    ?? pc.ComponentItem.Translations.Select(t => t.Title).FirstOrDefault(),
                SortOrder = pc.SortOrder
            })
            .ToListAsync(cancellationToken);

        return new PageAttachmentListVm
        {
            PageId = page.Id,
            CompanyId = companyId,
            PageTitle = title,
            PageSlug = page.Slug,
            PageType = page.PageType,
            Attachments = rows
        };
    }

    public async Task<CmsOpResult> TryAttachAsync(int pageId, int companyId, int componentItemId, int sortOrder, CancellationToken cancellationToken = default)
    {
        var page = await _db.Pages.AnyAsync(p => p.Id == pageId && p.CompanyId == companyId, cancellationToken);
        if (!page)
            return CmsOpResult.Fail("Sayfa bulunamadı.");

        var comp = await _db.ComponentItems.AnyAsync(c => c.Id == componentItemId && c.CompanyId == companyId, cancellationToken);
        if (!comp)
            return CmsOpResult.Fail("Bileşen bu şirkete ait değil.");

        var duplicate = await _db.PageComponents.AnyAsync(
            pc => pc.PageId == pageId && pc.ComponentItemId == componentItemId, cancellationToken);
        if (duplicate)
            return CmsOpResult.Fail("Bu bileşen sayfada zaten bağlı.");

        _db.PageComponents.Add(new PageComponent
        {
            PageId = pageId,
            ComponentItemId = componentItemId,
            SortOrder = sortOrder
        });
        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }

    public async Task<CmsOpResult> TryDetachAsync(int pageComponentId, int companyId, CancellationToken cancellationToken = default)
    {
        var pc = await _db.PageComponents
            .Include(x => x.Page)
            .FirstOrDefaultAsync(x => x.Id == pageComponentId, cancellationToken);
        if (pc is null || pc.Page.CompanyId != companyId)
            return CmsOpResult.Fail("Kayıt bulunamadı.");

        _db.PageComponents.Remove(pc);
        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }
}
