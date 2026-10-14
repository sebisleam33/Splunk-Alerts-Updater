using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeFields;

public record RetrieveChangeFieldsCommand(ChangeType ChangeType) : ICommand<ChangeTypeFields>;
