namespace SplunkBulkUpdater.Application.Models.ChangeTypes;

public static class ChangeTypeExtensions
{
    public static string ToSplunkActionKey(this ChangeType changeType) =>
    changeType switch
    {
        ChangeType.SPL => "search",
        ChangeType.Email => "action.email",
        ChangeType.Webhook => "action.webhook",
        _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null)
    };
}