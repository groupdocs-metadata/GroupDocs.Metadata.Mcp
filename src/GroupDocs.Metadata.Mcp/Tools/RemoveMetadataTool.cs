using System.ComponentModel;
using System.Text;
using GroupDocs.Metadata.Options;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using ModelContextProtocol.Server;

namespace GroupDocs.Metadata.Mcp.Tools;

[McpServerToolType]
public static class RemoveMetadataTool
{
    [McpServerTool, Description(
        "Removes all metadata from a document and saves the cleaned file to storage. " +
        "Call this tool immediately whenever the user asks to remove metadata, strip properties, or clean a document before sharing. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "The tool resolves files from storage and returns an error with available files if a name is not found.")]
    public static async Task<string> RemoveMetadata(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
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

            var removed = metadata.Sanitize();
            metadata.Save(tempOutput);

            var bytes = await File.ReadAllBytesAsync(tempOutput);
            var savedPath = await storage.WriteFileAsync(outputName, bytes, rewrite: false);

            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include watermarks.\n\n";
            return await output.BuildFileOutputAsync(savedPath, $"{prefix}Removed {removed} metadata package(s) from '{resolved.FileName}'");
        }
        catch (Exception ex)
        {
            return FormatException("Metadata removal", resolved.FileName, ex);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }

    // Surface engine failures as a descriptive text response instead of MCP's opaque
    // "An error occurred invoking 'remove_metadata'". Note: in evaluation mode
    // GroupDocs.Metadata.Save() throws "Could not save the file. Evaluation only." —
    // that now returns here as "Metadata removal failed for '<file>': ...". The
    // response text starts with "Metadata removal failed for '<file>': ...".
    private static string FormatException(string op, string file, Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append($"{op} failed for '{file}': {ex.GetType().FullName}: {ex.Message}");
        var inner = ex.InnerException;
        for (int d = 0; inner != null && d < 5; d++, inner = inner.InnerException)
        {
            sb.Append($" | inner({d}): {inner.GetType().FullName}: {inner.Message}");
        }
        return sb.ToString();
    }
}
