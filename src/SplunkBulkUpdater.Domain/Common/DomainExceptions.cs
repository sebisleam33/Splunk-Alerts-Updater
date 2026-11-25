namespace SplunkBulkUpdater.Domain.Common
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message, Exception? inner = null) : base(message, inner) { }
    }
    public sealed class InvalidSplReplacementException : DomainException
    {
        public InvalidSplReplacementException(string message, Exception? inner = null) : base(message, inner) { }
    }

    public sealed class RegexLimitExceededException : DomainException
    {
        public RegexLimitExceededException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
