using SplunkBulkUpdater.Application.Abstractions.Messaging;
using SplunkBulkUpdater.Application.UseCases.Common;

namespace SplunkBulkUpdater.Application.UseCases.LogIn;

public record LoginCommand(
    string Username,
    string Password) : ICommand<BaseResult>;
