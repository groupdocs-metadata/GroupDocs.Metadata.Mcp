using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Metadata.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Services
    .AddGroupDocsMcp()
    .AddLocalStorage("./Files");
builder.Services.AddSingleton<ILicenseManager, MetadataLicenseManager>();
builder.Services
    .AddMcpServer(options => { options.ServerInfo = new() { Name = "GroupDocs.Metadata.Mcp", Version = "26.3.0" }; })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync();
