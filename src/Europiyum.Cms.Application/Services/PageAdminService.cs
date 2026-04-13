using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Domain.Enums;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class PageAdminService
{
    private readonly CmsDbContext _db;

    public PageAdminService(CmsDbContext db) => _db = db;

    public async Task<IReadOnlyList<PageListItemVm>> ListForCompanyAsync(
        int companyId,
        PageListFilterVm? filter = null,
        CancellationToken cancellationToken = default)
    {
        var defaultLang = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.DefaultLanguageId)
            .FirstOrDefaultAsync(cancellationToken);

        var query = _db.Pages.AsNoTracking().Where(p => p.CompanyId == companyId);

        if (filter?.ActiveOnly == true)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter?.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Slug, $"%{s}%")
                || p.Translations.Any(t => EF.Functions.ILike(t.Title, $"%{s}%")));
        }

        return await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Slug)
            .Select(p => new PageListItemVm
            {
                Id = p.Id,
                Slug = p.Slug,
                PageType = p.PageType,
                TemplateKey = p.TemplateKey,
                IsActive = p.IsActive,
                SortOrder = p.SortOrder,
                TitlePreview = p.Translations.Where(t => t.LanguageId == defaultLang).Select(t => t.Title).FirstOrDefault()
                    ?? p.Translations.Select(t => t.Title).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PageEditVm?> GetForEditAsync(int pageId, int companyId, CancellationToken cancellationToken = default)
    {
        var page = await _db.Pages
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == pageId && p.CompanyId == companyId, cancellationToken);
        if (page is null)
            return null;

        var langs = await LoadEditorLanguagesAsync(companyId, cancellationToken);

        var vm = new PageEditVm
        {
            Id = page.Id,
            CompanyId = page.CompanyId,
            PageType = page.PageType,
            Slug = page.Slug,
            TemplateKey = page.TemplateKey,
            SortOrder = page.SortOrder,
            IsActive = page.IsActive,
            FormDefinitionId = page.FormDefinitionId
        };

        foreach (var lang in langs)
        {
            var tr = page.Translations.FirstOrDefault(t => t.LanguageId == lang.Id);
            vm.Translations.Add(new PageTranslationEditVm
            {
                LanguageId = lang.Id,
                LanguageCode = lang.Code,
                Title = tr?.Title ?? string.Empty,
                Slug = tr?.Slug,
                HtmlContent = tr?.HtmlContent,
                BreadcrumbHeading = tr?.BreadcrumbHeading,
                BreadcrumbBackgroundPath = tr?.BreadcrumbBackgroundPath
            });
        }

        return vm;
    }

    public async Task SaveAsync(PageEditVm vm, CancellationToken cancellationToken = default)
    {
        var page = await _db.Pages
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == vm.Id && p.CompanyId == vm.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Page not found.");

        page.Slug = vm.Slug.Trim();
        page.TemplateKey = string.IsNullOrWhiteSpace(vm.TemplateKey) ? null : vm.TemplateKey.Trim();
        page.SortOrder = vm.SortOrder;
        page.IsActive = vm.IsActive;
        page.UpdatedAt = DateTimeOffset.UtcNow;

        if (vm.FormDefinitionId is { } fid)
        {
            var formOk = await _db.FormDefinitions.AnyAsync(
                f => f.Id == fid && f.CompanyId == vm.CompanyId, cancellationToken);
            page.FormDefinitionId = formOk ? fid : null;
        }
        else
        {
            page.FormDefinitionId = null;
        }

        var allowedLangIds = (await LoadEditorLanguagesAsync(vm.CompanyId, cancellationToken)).Select(l => l.Id).ToHashSet();

        foreach (var row in vm.Translations.Where(t => allowedLangIds.Contains(t.LanguageId)))
        {
            var tr = page.Translations.FirstOrDefault(t => t.LanguageId == row.LanguageId);
            if (tr is null)
            {
                tr = new PageTranslation { PageId = page.Id, LanguageId = row.LanguageId };
                page.Translations.Add(tr);
            }

            tr.Title = row.Title.Trim();
            tr.Slug = string.IsNullOrWhiteSpace(row.Slug) ? null : row.Slug.Trim();
            tr.HtmlContent = row.HtmlContent;
            tr.BreadcrumbHeading = string.IsNullOrWhiteSpace(row.BreadcrumbHeading) ? null : row.BreadcrumbHeading.Trim();
            tr.BreadcrumbBackgroundPath = string.IsNullOrWhiteSpace(row.BreadcrumbBackgroundPath)
                ? null
                : row.BreadcrumbBackgroundPath.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageOperationResult> TryCreateAsync(PageCreateVm vm, CancellationToken cancellationToken = default)
    {
        var slug = vm.Slug.Trim();
        if (string.IsNullOrEmpty(slug))
            return PageOperationResult.Fail("Slug boş olamaz.");

        var exists = await _db.Pages.AnyAsync(
            p => p.CompanyId == vm.CompanyId && p.Slug == slug, cancellationToken);
        if (exists)
            return PageOperationResult.Fail("Bu slug bu şirket için zaten kullanılıyor.");

        if (vm.PageType == PageType.Home)
        {
            var homeExists = await _db.Pages.AnyAsync(
                p => p.CompanyId == vm.CompanyId && p.PageType == PageType.Home, cancellationToken);
            if (homeExists)
                return PageOperationResult.Fail("Bu şirket için zaten bir anasayfa kaydı var.");
        }

        var langs = await LoadEditorLanguagesAsync(vm.CompanyId, cancellationToken);
        if (langs.Count == 0)
            return PageOperationResult.Fail("Şirkette aktif dil tanımlı değil. Önce site dillerini yapılandırın.");

        var page = new Page
        {
            CompanyId = vm.CompanyId,
            PageType = vm.PageType,
            Slug = slug,
            TemplateKey = string.IsNullOrWhiteSpace(vm.TemplateKey) ? null : vm.TemplateKey.Trim(),
            SortOrder = vm.SortOrder,
            IsActive = true
        };

        _db.Pages.Add(page);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var lang in langs)
        {
            _db.PageTranslations.Add(new PageTranslation
            {
                PageId = page.Id,
                LanguageId = lang.Id,
                Title = vm.PageType == PageType.Home ? "Anasayfa" : "Yeni sayfa",
                Slug = vm.PageType == PageType.Home ? "" : null,
                HtmlContent = null
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return PageOperationResult.Success();
    }

    public async Task<PageOperationResult> TryDeleteAsync(int pageId, int companyId, CancellationToken cancellationToken = default)
    {
        var page = await _db.Pages.FirstOrDefaultAsync(
            p => p.Id == pageId && p.CompanyId == companyId, cancellationToken);
        if (page is null)
            return PageOperationResult.Fail("Sayfa bulunamadı.");

        if (page.PageType == PageType.Home)
            return PageOperationResult.Fail("Anasayfa kaydı silinemez.");

        _db.Pages.Remove(page);
        await _db.SaveChangesAsync(cancellationToken);
        return PageOperationResult.Success();
    }

    public async Task<IReadOnlyList<FormDefinitionOptionVm>> ListFormDefinitionOptionsAsync(
        int companyId,
        CancellationToken cancellationToken = default) =>
        await _db.FormDefinitions.AsNoTracking()
            .Where(f => f.CompanyId == companyId && f.IsActive)
            .OrderBy(f => f.Name)
            .Select(f => new FormDefinitionOptionVm { Id = f.Id, Name = f.Name, Key = f.Key })
            .ToListAsync(cancellationToken);

    private Task<List<Language>> LoadEditorLanguagesAsync(int companyId, CancellationToken cancellationToken) =>
        CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, companyId, cancellationToken);
}
