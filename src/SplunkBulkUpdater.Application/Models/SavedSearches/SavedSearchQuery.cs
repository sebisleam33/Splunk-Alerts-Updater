using SplunkBulkUpdater.Application.Models.Paging;

namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record SavedSearchQuery(
    string App,
    string Owner,
    bool AlertsOnly,
    PageQuery? Paging = null,
    IReadOnlyDictionary<string, string>? SearchStringFilters = null,
    IReadOnlyDictionary<string, string>? FieldsFilters = null);