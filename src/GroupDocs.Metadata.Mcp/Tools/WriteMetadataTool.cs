using System.ComponentModel;
using GroupDocs.Metadata.Common;
using GroupDocs.Metadata.Options;
using GroupDocs.Metadata.Tagging;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using ModelContextProtocol.Server;
// GroupDocs.Metadata's SetProperties/AddProperties consume its own Func delegate — alias it.
using MetadataPredicate = GroupDocs.Metadata.Common.Func<GroupDocs.Metadata.Common.MetadataProperty, bool>;

namespace GroupDocs.Metadata.Mcp.Tools;

[McpServerToolType]
public static class WriteMetadataTool
{
    [McpServerTool, Description(
        "Sets, changes, or adds a single metadata property on a document and saves the updated file to storage. " +
        "Call this whenever the user wants to write a property value — 'set the author of report.pdf to ABC', " +
        "'add title \"Q3 Report\" to file.docx', 'change the subject', 'put my company name in the metadata'. " +
        "`property` is one of Author, Title, Subject, Keywords, Comments, Copyright, Company, Manager; " +
        "`value` is the text to write; `mode` is 'set' (replace, default) or 'add' (append, for list fields like Keywords). " +
        "The tool maps the friendly name to the correct field per format automatically (PDF Info/XMP, Office document " +
        "properties, EXIF/IPTC for images) and creates the field if it is absent. " +
        "Supports PDF, DOCX, XLSX, PPTX, JPEG, PNG, TIFF and 100+ more formats. " +
        "Returns a saved-path message plus how many fields changed; if the property does not apply to the file's " +
        "format it reports 0 changed. On failure the response text starts with 'Metadata write failed for'.")]
    public static async Task<string> WriteMetadata(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
        [Description("Property to write: Author, Title, Subject, Keywords, Comments, Copyright, Company, or Manager")] string property,
        [Description("The text value to write")] string value,
        [Description("'set' to replace the value (default), or 'add' to append (for list fields like Keywords)")] string mode = "set",
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        var predicate = PropertyPredicate(property);
        if (predicate == null)
        {
            return ToolError.Format("Metadata write", resolved.FileName, new ArgumentException(
                $"Unknown property '{property}'. Supported: Author, Title, Subject, Keywords, Comments, Copyright, Company, Manager."));
        }

        var ext = Path.GetExtension(resolved.FileName);
        var outputName = $"{Path.GetFileNameWithoutExtension(resolved.FileName)}_updated{ext}";
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

            var propertyValue = new PropertyValue(value);
            var isAdd = string.Equals(mode, "add", StringComparison.OrdinalIgnoreCase);

            var affected = isAdd
                ? metadata.AddProperties(predicate, propertyValue)
                : metadata.SetProperties(predicate, propertyValue);

            // SetProperties creates known fields when absent for most formats; fall back to
            // AddProperties for the rare format where it doesn't, so 'set' still writes.
            if (affected == 0 && !isAdd)
                affected = metadata.AddProperties(predicate, propertyValue);

            if (affected == 0)
                return $"Metadata write for '{resolved.FileName}': property '{property}' is not applicable to this format (0 fields changed).";

            metadata.Save(tempOutput);

            var bytes = await File.ReadAllBytesAsync(tempOutput);
            var savedPath = await storage.WriteFileAsync(outputName, bytes, rewrite: false);

            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include watermarks.\n\n";
            return await output.BuildFileOutputAsync(savedPath, $"{prefix}Set {property} = '{value}' ({affected} field(s) updated) in '{resolved.FileName}'");
        }
        catch (Exception ex)
        {
            return ToolError.Format("Metadata write", resolved.FileName, ex);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }

    // Friendly property name → tag predicate. Each maps to the right per-format field
    // via the GroupDocs.Metadata Tags taxonomy (verified against 26.6.0).
    private static MetadataPredicate? PropertyPredicate(string property) => property?.Trim().ToLowerInvariant() switch
    {
        "author" or "creator"   => p => p.Tags.Contains(Tags.Person.Creator),
        "title"                 => p => p.Tags.Contains(Tags.Content.Title),
        "subject"               => p => p.Tags.Contains(Tags.Content.Subject),
        "keywords"              => p => p.Tags.Contains(Tags.Content.Keywords),
        "comments" or "comment" => p => p.Tags.Contains(Tags.Content.Comment),
        "copyright"             => p => p.Tags.Contains(Tags.Legal.Copyright),
        "company"               => p => p.Tags.Contains(Tags.Corporate.Company),
        "manager"               => p => p.Tags.Contains(Tags.Corporate.Manager),
        _ => null,
    };
}
