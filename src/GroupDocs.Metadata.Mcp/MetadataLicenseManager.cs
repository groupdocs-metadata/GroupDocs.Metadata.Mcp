using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupDocs.Metadata.Mcp;

public class MetadataLicenseManager : LicenseManager
{
    public MetadataLicenseManager(IOptions<McpConfig> config, ILogger<LicenseManager> logger) : base(config, logger) { }
    protected override void SetLicenseFromPath(string licensePath)
    {
        new GroupDocs.Metadata.License().SetLicense(licensePath);
    }
}
