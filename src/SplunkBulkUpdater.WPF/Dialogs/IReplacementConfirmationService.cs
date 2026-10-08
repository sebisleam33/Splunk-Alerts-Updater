using SplunkBulkUpdater.Application.UseCases.BulkUpdateSplString;

namespace SplunkBulkUpdater.WPF.Dialogs;

public interface IReplacementConfirmationService
{
    bool Confirm(IReadOnlyList<SplChangePreview> changes);
}