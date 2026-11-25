namespace SplunkBulkUpdater.Application.UseCases.RetrieveOwnershipFilter;

public record RetrieveOwnershipFilterPayload(
    IReadOnlyList<OwnershipFilter> OwnershipFilters);
