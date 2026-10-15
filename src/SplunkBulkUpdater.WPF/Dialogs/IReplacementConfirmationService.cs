using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

namespace SplunkBulkUpdater.WPF.Dialogs;

public interface IReplacementConfirmationService
{
    IReadOnlyList<SplChangePreview>? Confirm(IReadOnlyList<SplChangePreview> changes);
}
