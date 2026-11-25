using SplunkBulkUpdater.Domain.Spl;

namespace SplunkBulkUpdater.Domain.SavedSearches
{
    public sealed class SavedSearch
    {
        public SavedSearchRef Ref { get; }
        public string Spl { get; }

        public SavedSearch(SavedSearchRef reference, string spl)
        {
            ArgumentNullException.ThrowIfNull(reference);
            ArgumentNullException.ThrowIfNull(spl);

            Ref = reference;
            Spl = spl;
        }

        public bool SplMatches(SplReplacement replacement) =>
            replacement.IsMatchIn(Spl);

        public SplPreview PreviewReplacement(SplReplacement replacement)
        {
            var outcome = replacement.ApplyTo(Spl);
            return new SplPreview(Ref, outcome.MatchCount, Spl, outcome.ProposedSpl);
        }
    }
}
