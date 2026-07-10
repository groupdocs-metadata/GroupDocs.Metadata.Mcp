using GroupDocs.Metadata.Common;
using GroupDocs.Metadata.Tagging;
// GroupDocs.Metadata ships its own Func<T,TResult> delegate (Java-port artifact) that
// FindProperties / SetProperties / RemoveProperties consume. Alias it so predicate
// types are unambiguous against System.Func.
using MetadataPredicate = GroupDocs.Metadata.Common.Func<GroupDocs.Metadata.Common.MetadataProperty, bool>;

namespace GroupDocs.Metadata.Mcp.Tools;

// Maps friendly category keywords to GroupDocs.Metadata tag predicates. Shared by
// search_metadata (the `category` filter) and remove_metadata (selective removal),
// so the taxonomy lives in exactly one place. All tags referenced here are verified
// present in the GroupDocs.Metadata 26.6.0 Tags taxonomy.
internal static class MetadataCategories
{
    // Category keys advertised to agents (for the remove_metadata `categories` arg
    // and the search_metadata `category` filter). Kept in sync with the tool [Description]s.
    public static readonly string[] Keys =
    {
        "gps", "author", "comments", "company", "dates",
        "software", "copyright", "keywords", "personal",
    };

    // Returns a property predicate for a category keyword, or null if unknown.
    public static MetadataPredicate? Resolve(string? key) => key?.Trim().ToLowerInvariant() switch
    {
        "gps" or "location"       => p => p.Tags.Contains(Tags.PropertyType.Location),
        "author" or "person" or "creator" => p => p.Tags.Any(t => t.Category is PersonTagCategory),
        "comments" or "comment"   => p => p.Tags.Contains(Tags.Content.Comment),
        "company" or "corporate"  => p => p.Tags.Any(t => t.Category is CorporateTagCategory),
        "dates" or "date" or "time" => p => p.Tags.Any(t => t.Category is TimeTagCategory),
        "software" or "tool"      => p => p.Tags.Any(t => t.Category is ToolTagCategory),
        "copyright" or "legal"    => p => p.Tags.Any(t => t.Category is LegalTagCategory),
        "keywords"                => p => p.Tags.Contains(Tags.Content.Keywords),
        "content"                 => p => p.Tags.Any(t => t.Category is ContentTagCategory),
        "document"                => p => p.Tags.Any(t => t.Category is DocumentTagCategory),
        "personal" or "pii"       => p => p.Tags.Any(t => t.Category is PersonTagCategory || t.Category is CorporateTagCategory)
                                          || p.Tags.Contains(Tags.PropertyType.Location),
        _ => null,
    };

    // Human-readable category label for a property (its first recognised tag category,
    // e.g. "Person", "Content"). Null when the property carries no categorised tag.
    public static string? Label(MetadataProperty property)
    {
        foreach (var tag in property.Tags)
        {
            var name = tag.Category?.GetType().Name;
            if (!string.IsNullOrEmpty(name))
                return name.EndsWith("TagCategory") ? name[..^"TagCategory".Length] : name;
        }
        return null;
    }
}
