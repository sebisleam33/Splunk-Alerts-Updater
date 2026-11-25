using SplunkBulkUpdater.Application.Models.ChangeTypes;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeTypes;

public record RetrieveChangeTypesPayload(
    IReadOnlyList<ChangeType> ChangeTypes);

