using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeFields;

public record RetrieveChangeFieldsCommand(ChangeType ChangeType) : ICommand<BaseResult<ChangeTypeFields>>;
