using SplunkBulkUpdater.Application.Models.Paging;

namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public sealed record SavedSearchQuery(
    string App,
    string Owner,
    bool AlertsOnly,
    PageQuery? Paging = null,
    IEnumerable<KeyValuePair<string, string>>? SearchStringFilters = null,
    IEnumerable<KeyValuePair<string, string>>? FieldsFilters = null);