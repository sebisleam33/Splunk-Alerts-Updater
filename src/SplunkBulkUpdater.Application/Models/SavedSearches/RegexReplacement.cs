namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public class RegexReplacement
{
    public string Regex { get; set; }
    public string Replacement { get; set; }

    public RegexReplacement(string regex, string replacement)
    {
        Regex = regex;
        Replacement = replacement;
    }
}
