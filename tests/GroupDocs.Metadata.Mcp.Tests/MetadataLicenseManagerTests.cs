using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Metadata.Mcp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GroupDocs.Metadata.Mcp.Tests;

public class MetadataLicenseManagerTests
{
    [Fact]
    public void IsLicensed_WithoutLicensePath_ReturnsFalse()
    {
        var options = Options.Create(new McpConfig());
        var manager = new MetadataLicenseManager(options, NullLogger<LicenseManager>.Instance);

        Assert.False(manager.IsLicensed);
    }

    [Fact]
    public void SetLicense_WithoutLicensePath_DoesNotThrow()
    {
        var options = Options.Create(new McpConfig());
        var manager = new MetadataLicenseManager(options, NullLogger<LicenseManager>.Instance);

        var ex = Record.Exception(() => manager.SetLicense());
        Assert.Null(ex);
    }
}
