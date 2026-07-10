using System.ComponentModel;
using System.Text.Json;
using GroupDocs.Metadata.Options;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using ModelContextProtocol.Server;

namespace GroupDocs.Metadata.Mcp.Tools;

[McpServerToolType]
public static class GetDocumentInfoTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description(
        "Returns the file type, size, page count, and encryption status of a document as JSON — a lightweight structural check that does NOT enumerate metadata properties (use ReadMetadata for that). " +
        "Supports PDF, DOCX, XLSX, PPTX, JPEG, PNG, TIFF, MP3, MP4, WAV, AVI, and 50+ more document, image, and media formats. " +
        "Call this tool whenever the user asks about a document's format, page count, byte size, or whether it is encrypted, without needing the full metadata dump. " +
        "Do NOT pre-check whether the file exists — just pass the filename the user provided. " +
        "Returns a JSON object with fields `fileName`, `fileFormat` (engine-reported format name), `mimeType`, `pageCount`, `sizeBytes`, and `isEncrypted`. " +
        "On failure, the response text starts with 'Document-info lookup failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> GetDocumentInfo(
        IFileResolver resolver,
        ILicenseManager licenseManager,
        FileInput file,
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

            var info = metadata.GetDocumentInfo();

            var payload = new
            {
                fileName = resolved.FileName,
                fileFormat = info.FileType.FileFormat.ToString(),
                mimeType = info.FileType.MimeType,
                pageCount = info.PageCount,
                sizeBytes = info.Size,
                isEncrypted = info.IsEncrypted,
            };

            return JsonSerializer.Serialize(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            return ToolError.Format("Document-info lookup", resolved.FileName, ex);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
        }
    }
}
