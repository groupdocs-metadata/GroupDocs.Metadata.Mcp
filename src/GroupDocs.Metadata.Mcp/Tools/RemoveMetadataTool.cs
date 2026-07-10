using System.ComponentModel;
using GroupDocs.Metadata.Common;
using GroupDocs.Metadata.Options;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using ModelContextProtocol.Server;
// GroupDocs.Metadata's RemoveProperties consumes its own Func delegate — alias it.
using MetadataPredicate = GroupDocs.Metadata.Common.Func<GroupDocs.Metadata.Common.MetadataProperty, bool>;

namespace GroupDocs.Metadata.Mcp.Tools;

[McpServerToolType]
public static class RemoveMetadataTool
{
    [McpServerTool, Description(
        "Removes metadata from a document and saves a cleaned copy to storage. " +
        "By default (no categories) it strips ALL removable metadata — use this for 'remove all metadata before sharing'. " +
        "To remove only SPECIFIC kinds, pass `categories` (one or more of): gps (location/geotags), author (creator/editor), " +
        "comments, company, dates, software (the 'created with' tool fingerprint), copyright, keywords, " +
        "personal (best-effort PII bundle: people + company + location). " +
        "Call this whenever the user asks to remove, strip, clean, or redact metadata — e.g. 'remove GPS from photo.jpg' " +
        "or 'strip the author from report.pdf'. " +
        "Supports PDF, DOCX, XLSX, PPTX, JPEG, PNG, TIFF and 100+ more formats. " +
        "Returns a saved-path message with the number of properties removed. " +
        "On failure the response text starts with 'Metadata removal failed for'.")]
    public static async Task<string> RemoveMetadata(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
        [Description("Optional: remove only specific kinds — any of gps, author, comments, company, dates, software, copyright, keywords, personal. Omit (or 'all') to remove every removable property.")] string[]? categories = null,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        var ext = Path.GetExtension(resolved.FileName);
        var outputName = $"{Path.GetFileNameWithoutExtension(resolved.FileName)}_clean{ext}";
        var tempInput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{ext}");
        var tempOutput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{ext}");

        try
        {
            await using (var fs = File.Create(tempInput))
                await resolved.Stream.CopyToAsync(fs);

            var loadOptions = password != null ? new LoadOptions { Password = password } : null;
            using var metadata = loadOptions != null
                ? new Metadata(tempInput, loadOptions)
                : new Metadata(tempInput);

            var full = categories == null || categories.Length == 0
                       || categories.Any(c => string.Equals(c, "all", StringComparison.OrdinalIgnoreCase));

            int removed;
            string summary;
            if (full)
            {
                removed = metadata.Sanitize();
                summary = $"{removed} metadata package(s)";
            }
            else
            {
                var predicates = categories!.Select(MetadataCategories.Resolve).OfType<MetadataPredicate>().ToList();
                if (predicates.Count == 0)
                {
                    return ToolError.Format("Metadata removal", resolved.FileName, new ArgumentException(
                        $"No known categories in [{string.Join(", ", categories!)}]. Supported: {string.Join(", ", MetadataCategories.Keys)}."));
                }

                removed = metadata.RemoveProperties(p => predicates.Any(pred => pred(p)));
                summary = $"{removed} propert{(removed == 1 ? "y" : "ies")} ({string.Join(", ", categories!)})";
            }

            metadata.Save(tempOutput);

            var bytes = await File.ReadAllBytesAsync(tempOutput);
            var savedPath = await storage.WriteFileAsync(outputName, bytes, rewrite: false);

            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include watermarks.\n\n";
            return await output.BuildFileOutputAsync(savedPath, $"{prefix}Removed {summary} from '{resolved.FileName}'");
        }
        catch (Exception ex)
        {
            return ToolError.Format("Metadata removal", resolved.FileName, ex);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }
}
