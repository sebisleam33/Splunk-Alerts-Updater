namespace SplunkBulkUpdater.Domain.SavedSearches
{
    public sealed record SavedSearchRef
    {
        public string Name { get; }
        public string App { get; }
        public string Owner { get; }

        public SavedSearchRef(string name, string app, string owner)
        {
            Name = Require(name, nameof(name));
            App = Require(app, nameof(app));
            Owner = Require(owner, nameof(owner));
        }

        private static string Require(string value, string parameter) =>
            string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value cannot be empty.", parameter): value;
    }
}
