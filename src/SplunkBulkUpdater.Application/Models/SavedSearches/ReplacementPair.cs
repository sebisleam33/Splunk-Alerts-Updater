namespace SplunkBulkUpdater.Application.Models.SavedSearches;

public class ReplacementPair
{
    public string Substring { get; set; }
    public string Replacement { get; set; }

    public ReplacementPair(string substring, string replacement)
    {
        Substring = substring;
        Replacement = replacement;
    }
}
