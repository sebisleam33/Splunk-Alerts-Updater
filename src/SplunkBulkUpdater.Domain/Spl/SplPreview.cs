using SplunkBulkUpdater.Domain.SavedSearches;

namespace SplunkBulkUpdater.Domain.Spl
{
    public sealed record SplPreview(
        SavedSearchRef Ref,
        int MatchCount,
        string OriginalSpl,
        string ProposedSpl)
    {
        public bool GetWouldChange()
        {
            return !string.Equals(OriginalSpl, ProposedSpl, StringComparison.Ordinal);
        }
    }
}
