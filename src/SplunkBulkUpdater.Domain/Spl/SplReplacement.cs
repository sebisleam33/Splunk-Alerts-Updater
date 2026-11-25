using SplunkBulkUpdater.Domain.Common;
using System.Text.RegularExpressions;

namespace SplunkBulkUpdater.Domain.Spl
{
    public sealed class SplReplacement
    {
        private readonly Regex _regex;
        private readonly RegexLimits _limits;

        public string Pattern { get; }
        public string Replacement { get; }

        public SplReplacement(string pattern, string replacement, bool ignoreCase = false, RegexLimits? limits = null)
        {
            ArgumentNullException.ThrowIfNull(replacement);

            _limits = limits ?? RegexLimits.Default;
            _regex = CreateRegex(pattern, ignoreCase, _limits);

            Pattern = pattern;
            Replacement = replacement;
        }

        public bool IsMatchIn(string spl) =>
            Run(spl, regex => regex.IsMatch(spl));


        public SplReplacementOutcome ApplyTo(string spl)
        {
            return Run(spl, regex =>
            {
                var count = 0;
                var proposed = regex.Replace(spl, match =>
                {
                    count++;
                    return match.Result(Replacement);
                });
                return new SplReplacementOutcome(count, proposed);
            });
        }

        private T Run<T>(string spl, Func<Regex, T> operation)
        {
            ArgumentNullException.ThrowIfNull(spl);

            if (spl.Length > _limits.MaxSplLength)
                throw new RegexLimitExceededException(
                    $"The SPL is {spl.Length} characters long; the limit is {_limits.MaxSplLength}.");

            try
            {
                return operation(_regex);
            }
            catch (RegexMatchTimeoutException ex)
            {
                throw new RegexLimitExceededException(
                    $"The regular expression exceeded the time limit of {_limits.MatchTimeout.TotalMilliseconds:0} ms.", ex);
            }
        }

        private static Regex CreateRegex(string pattern, bool ignoreCase, RegexLimits limits)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new InvalidSplReplacementException("The pattern cannot be empty.");

            if (pattern.Length > limits.MaxPatternLength)
                throw new InvalidSplReplacementException(
                    $"The pattern is {pattern.Length} characters long; the limit is {limits.MaxPatternLength}.");

            var options = RegexOptions.CultureInvariant;
            if (ignoreCase) options |= RegexOptions.IgnoreCase;

            try
            {
                return new Regex(pattern, options, limits.MatchTimeout);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidSplReplacementException(
                    $"The pattern is not a valid regular expression: {ex.Message}", ex);
            }
        }
    }
}
