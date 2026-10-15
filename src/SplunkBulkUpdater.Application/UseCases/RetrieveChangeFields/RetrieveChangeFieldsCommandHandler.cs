using FluentValidation;
using Microsoft.Extensions.Logging;
using SplunkBulkUpdater.Application.Abstractions.Persistence;
using SplunkBulkUpdater.Application.Models.ChangeTypes;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.RetrieveChangeFields;

public class RetrieveChangeFieldsCommandHandler : AbstractCommandHandler<RetrieveChangeFieldsCommand, BaseResult<ChangeTypeFields>>
{
    private readonly ISavedSearchRepository _savedSearchRepository;

    public RetrieveChangeFieldsCommandHandler(
        IValidator<RetrieveChangeFieldsCommand> validator,
        IApplicationExceptionMapper exceptionMapper,
        ILogger logger,
        ISavedSearchRepository savedSearchRepository) : base(validator, exceptionMapper, logger)
    {
        _savedSearchRepository = savedSearchRepository;
    }

    protected override BaseResult<ChangeTypeFields> CreateExceptionFailure(ApplicationError error)
    {
        return BaseResult<ChangeTypeFields>.Failure(
            error.Code,
            error.Message);
    }

    protected override BaseResult<ChangeTypeFields> CreateValidationFailure(IReadOnlyList<ValidationError> errors)
    {
        return BaseResult<ChangeTypeFields>
            .FromValidationErrors(errors);
    }

    protected override async Task<BaseResult<ChangeTypeFields>> ExecuteAsync(RetrieveChangeFieldsCommand command, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, string>();

        var changeType = command.ChangeType;

        if (changeType.Equals(ChangeType.SPL))
        {

            return BaseResult<ChangeTypeFields>.Success(new ChangeTypeFields(result));
        }

        var schema = await _savedSearchRepository.GetChangeTypeFieldSchema(changeType, cancellationToken);

        return BaseResult<ChangeTypeFields>.Success(schema);
    }
}
