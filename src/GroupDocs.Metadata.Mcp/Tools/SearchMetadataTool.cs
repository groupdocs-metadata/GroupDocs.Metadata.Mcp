using System.ComponentModel;
using System.Text.Json;
using GroupDocs.Metadata.Options;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using ModelContextProtocol.Server;

namespace GroupDocs.Metadata.Mcp.Tools;

[McpServerToolType]
public static class SearchMetadataTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description(
        "Searches the metadata inside a single document and returns only the matching properties as JSON — use this " +
        "instead of ReadMetadata when the user asks about a SPECIFIC property rather than the whole set. " +
        "Call it for 'show me the author of report.pdf', 'what is the creation date?', 'does photo.jpg have GPS?', " +
        "or to check a value like 'does contract.docx have Author = ABC?'. " +
        "Filters (combine any): category (person, content, time, tool, legal, corporate, document, gps, comments, keywords), " +
        "nameContains, valueContains — all case-insensitive. " +
        "Supports PDF, DOCX, XLSX, PPTX, JPEG, PNG, TIFF, MP3, MP4 and 100+ more formats. " +
        "Returns a JSON object with fields `count` and `properties` (array of { name, value, category }); " +
        "`count` 0 means no match. On failure the response text starts with 'Metadata search failed for'.")]
    public static async Task<string> SearchMetadata(
        IFileResolver resolver,
        ILicenseManager licenseManager,
        FileInput file,
        [Description("Optional category filter: person, content, time, tool, legal, corporate, document, gps, comments, keywords")] string? category = null,
        [Description("Optional: only properties whose name contains this text (case-insensitive)")] string? nameContains = null,
        [Description("Optional: only properties whose value contains this text (case-insensitive)")] string? valueContains = null,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        var tempInput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{Path.GetExtension(resolved.FileName)}");
        try
        {
            await using (var fs = File.Create(tempInput))
                await resolved.Stream.CopyToAsync(fs);

            var loadOptions = password != null ? new LoadOptions { Password = password } : null;
            using var metadata = loadOptions != null
                ? new Metadata(tempInput, loadOptions)
                : new Metadata(tempInput);

            var categoryPredicate = category != null ? MetadataCategories.Resolve(category) : null;

            var matches = metadata.FindProperties(p =>
                (categoryPredicate == null || categoryPredicate(p)) &&
                (nameContains == null || (p.Name?.Contains(nameContains, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                (valueContains == null || (p.Value?.RawValue?.ToString()?.Contains(valueContains, StringComparison.OrdinalIgnoreCase) ?? false)));

            var properties = matches
                .Select(p => new { name = p.Name, value = Displayable(p.Value?.RawValue), category = MetadataCategories.Label(p) })
                .ToList();

            var result = new { count = properties.Count, properties };
            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex)
        {
            return ToolError.Format("Metadata search", resolved.FileName, ex);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
        }
    }

    // Keep the JSON response small and readable: never emit raw binary blobs (e.g. a
    // Thumbnail property under the Content category would otherwise serialize as a huge
    // base64 array). Replace byte arrays with a compact placeholder.
    private static object? Displayable(object? raw) => raw switch
    {
        byte[] bytes => $"<binary {bytes.Length} bytes>",
        _ => raw,
    };
}
