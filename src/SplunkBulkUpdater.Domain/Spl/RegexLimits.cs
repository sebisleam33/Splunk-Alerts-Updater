namespace SplunkBulkUpdater.Domain.Spl
{
    public sealed record RegexLimits
    {
        public static RegexLimits Default { get; } =
            new(maxPatternLength: 1_000, maxSplLength: 100_000, matchTimeout: TimeSpan.FromSeconds(2));

        public int MaxPatternLength { get; }
        public int MaxSplLength { get; }
        public TimeSpan MatchTimeout { get; }

        public RegexLimits(int maxPatternLength, int maxSplLength, TimeSpan matchTimeout)
        {
            if (maxPatternLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxPatternLength));
            if (maxSplLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxSplLength));
            if (matchTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(matchTimeout));

            MaxPatternLength = maxPatternLength;
            MaxSplLength = maxSplLength;
            MatchTimeout = matchTimeout;
        }
    }
}
