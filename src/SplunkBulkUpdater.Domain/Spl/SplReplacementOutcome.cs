namespace SplunkBulkUpdater.Domain.Spl
{
    public readonly record struct SplReplacementOutcome(int MatchCount, string ProposedSpl);
}
